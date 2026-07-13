using Clean.Architecture.Core.ContributorAggregate;
using Clean.Architecture.Infrastructure.Data;
using Microsoft.Data.Sqlite;

namespace Clean.Architecture.IntegrationTests.Data;

public abstract class BaseEfRepoTestFixture : IDisposable, IAsyncDisposable
{
  private readonly AppDbContext _dbContext;

  protected AppDbContext DbContext => _dbContext;

  protected BaseEfRepoTestFixture()
  {
    var fakeEventDispatcher = Substitute.For<IDomainEventDispatcher>();

    var serviceProvider = new ServiceCollection()
      .AddEntityFrameworkSqlite()
      .AddScoped<IDomainEventDispatcher>(_ => fakeEventDispatcher)
      .AddScoped<EventDispatchInterceptor>()
      .BuildServiceProvider();

    _connection = new SqliteConnection("Filename=:memory:");
    _connection.Open();

    var interceptor = serviceProvider.GetRequiredService<EventDispatchInterceptor>();

    var options = new DbContextOptionsBuilder<AppDbContext>()
      .UseSqlite(_connection)
      .UseInternalServiceProvider(serviceProvider)
      .AddInterceptors(interceptor)
      .Options;

    _dbContext = new AppDbContext(options);

    _dbContext.Database.EnsureDeleted();
    _dbContext.Database.EnsureCreated();
  }

  protected EfRepository<Contributor> GetRepository()
  {
    return new EfRepository<Contributor>(_dbContext);
  }

  public void Dispose()
  {
    GC.SuppressFinalize(this);
    _dbContext.Dispose();
  }

  public async ValueTask DisposeAsync()
  {
    GC.SuppressFinalize(this);
    await _dbContext.DisposeAsync();
  }
}
