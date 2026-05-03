using Clean.Architecture.Infrastructure.Data;
using DotNet.Testcontainers.Builders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Testcontainers.MsSql;

namespace Clean.Architecture.FunctionalTests;

public class CustomWebApplicationFactory<TProgram>
  : WebApplicationFactory<TProgram>, IAsyncLifetime
  where TProgram : class
{
  private MsSqlContainer? _dbContainer;

  public async Task InitializeAsync()
  {
    try
    {
      _dbContainer = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2025-latest")
        .WithPassword("Your_password123!")
        .WithEnvironment("MSSQL_PID", "Evaluation")
        .Build();

      await _dbContainer.StartAsync();
    }
    catch (DockerUnavailableException)
    {
      // Docker not available → fallback to SQLite
      _dbContainer = null;
    }
  }

  public new async Task DisposeAsync()
  {
    Environment.SetEnvironmentVariable("USE_SQL_SERVER", null);

    if (_dbContainer != null)
    {
      await _dbContainer.DisposeAsync();
    }
  }

  protected override IHost CreateHost(IHostBuilder builder)
  {
    builder.UseEnvironment("Testing");

    var host = builder.Build();
    host.Start();

    using var scope = host.Services.CreateScope();
    var services = scope.ServiceProvider;

    var db = services.GetRequiredService<AppDbContext>();
    var logger = services.GetRequiredService<ILogger<CustomWebApplicationFactory<TProgram>>>();

    try
    {
      if (_dbContainer != null)
      {
        // SQL Server → run migrations
        db.Database.Migrate();
      }
      else
      {
        // SQLite fallback
        db.Database.EnsureCreated();
      }

      // 🔥 CRITICAL: run seeding and FAIL HARD if it breaks
      SeedData.InitializeAsync(db)
        .GetAwaiter()
        .GetResult();

      // 🔍 Verify seeding worked
      var contributorCount = db.Contributors.Count();

      logger.LogInformation("Seeded {Count} contributors.", contributorCount);

      if (contributorCount == 0)
      {
        throw new InvalidOperationException(
          "Database seeding completed, but no contributors were created.");
      }
    }
    catch (Exception ex)
    {
      logger.LogError(ex,
        "An error occurred seeding the database. Error: {Message}",
        ex.Message);

      throw; // 🔥 DO NOT SWALLOW — fail tests immediately
    }

    return host;
  }

  protected override void ConfigureWebHost(IWebHostBuilder builder)
  {
    if (_dbContainer != null)
    {
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
        if (_dbContainer != null)
        {
          // Remove existing DbContext registrations
          var descriptors = services
            .Where(d =>
              d.ServiceType == typeof(AppDbContext) ||
              d.ServiceType == typeof(DbContextOptions<AppDbContext>))
            .ToList();

          foreach (var descriptor in descriptors)
          {
            services.Remove(descriptor);
          }

          // Register DbContext with Testcontainer SQL Server
          services.AddDbContext<AppDbContext>((provider, options) =>
          {
            options.UseSqlServer(_dbContainer.GetConnectionString());

            var interceptor = provider.GetRequiredService<EventDispatchInterceptor>();
            options.AddInterceptors(interceptor);
          });
        }
      });
  }
}
