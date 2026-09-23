using Clean.Architecture.Infrastructure.Data;
using DotNet.Testcontainers.Builders;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Testcontainers.MsSql;

namespace Clean.Architecture.FunctionalTests;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram>, IAsyncLifetime where TProgram : class
{
  private MsSqlContainer? _dbContainer;
  private readonly string _sqliteConnectionString = $"Data Source=clean-architecture-functional-{Guid.NewGuid():N};Mode=Memory;Cache=Shared";
  private SqliteConnection? _sqliteKeepAliveConnection;

  public async ValueTask InitializeAsync()
  {
    // Hosted Windows runners use Windows containers; SQL Server's image requires Linux.
    // The macOS runner has no Docker daemon. Both run against SQLite in CI.
    if (string.Equals(Environment.GetEnvironmentVariable("SKIP_SQL_SERVER_CONTAINER"), "true", StringComparison.OrdinalIgnoreCase))
    {
      OpenSqliteDatabase();
      return;
    }

    try
    {
      _dbContainer = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2025-latest")
        .WithPassword("Your_password123!")
        .Build();
      await _dbContainer.StartAsync();
    }
    catch (Exception ex) when (ex is ArgumentException or DockerUnavailableException)
    {
      // Docker is not available; fall back to SQLite (configured via appsettings.Testing.json)
      _dbContainer = null;
      OpenSqliteDatabase();
    }
  }

  public new async ValueTask DisposeAsync()
  {
    // Clean up environment variable
    Environment.SetEnvironmentVariable("USE_SQL_SERVER", null);
    await base.DisposeAsync();
    if (_dbContainer != null)
    {
      await _dbContainer.DisposeAsync();
    }
    _sqliteKeepAliveConnection?.Dispose();
  }

  private void OpenSqliteDatabase()
  {
    // A named in-memory database stays available to EF's separate connections
    // until the last connection closes.
    _sqliteKeepAliveConnection = new SqliteConnection(_sqliteConnectionString);
    _sqliteKeepAliveConnection.Open();
  }

  /// <summary>
  /// Overriding CreateHost to avoid creating a separate ServiceProvider per this thread:
  /// https://github.com/dotnet-architecture/eShopOnWeb/issues/465
  /// </summary>
  /// <param name="builder"></param>
  /// <returns></returns>
  protected override IHost CreateHost(IHostBuilder builder)
  {
    builder.UseEnvironment("Testing"); // will not send real emails
    var host = builder.Build();
    host.Start();

    // Get service provider.
    var serviceProvider = host.Services;

    // Create a scope to obtain a reference to the database
    // context (AppDbContext).
    using (var scope = serviceProvider.CreateScope())
    {
      var scopedServices = scope.ServiceProvider;
      var db = scopedServices.GetRequiredService<AppDbContext>();

      var logger = scopedServices
          .GetRequiredService<ILogger<CustomWebApplicationFactory<TProgram>>>();

      try
      {
        // Functional tests use EnsureCreated to avoid migration-script coupling.
        db.Database.EnsureCreated();

        // Seed the database with test data only if it has not been seeded yet.
        // This is safe for container reuse across test runs and multiple fixture instances.
        SeedData.InitializeAsync(db).GetAwaiter().GetResult();
      }
      catch (Exception ex)
      {
        logger.LogError(ex, "An error occurred seeding the database with test messages. Error: {exceptionMessage}", ex.Message);
        throw;
      }
    }

    return host;
  }

  protected override void ConfigureWebHost(IWebHostBuilder builder)
  {
    if (_dbContainer != null)
    {
      // Force SQL Server mode even on non-Windows platforms for functional tests
      Environment.SetEnvironmentVariable("USE_SQL_SERVER", "true");
    }

    builder
        .ConfigureAppConfiguration((context, config) =>
        {
          if (_dbContainer != null)
          {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
              ["ConnectionStrings:DefaultConnection"] = _dbContainer.GetConnectionString()
            });
          }
        })
        .ConfigureServices(services =>
        {
          // Program registers its DbContext before WebApplicationFactory applies
          // test configuration, so replace that registration for both providers.
          var descriptors = services.Where(
            d => d.ServiceType == typeof(AppDbContext) ||
                 d.ServiceType == typeof(DbContextOptions<AppDbContext>) ||
                 d.ServiceType == typeof(IDbContextOptionsConfiguration<AppDbContext>))
            .ToList();

          foreach (var descriptor in descriptors)
          {
            services.Remove(descriptor);
          }

          services.AddDbContext<AppDbContext>((provider, options) =>
          {
            if (_dbContainer != null)
            {
              options.UseSqlServer(_dbContainer.GetConnectionString());
            }
            else
            {
              options.UseSqlite(_sqliteConnectionString);
            }

            options.AddInterceptors(provider.GetRequiredService<EventDispatchInterceptor>());
          });
        });
  }
}
