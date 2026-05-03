using Clean.Architecture.Core.ContributorAggregate;
using Clean.Architecture.Infrastructure.Data;
using Microsoft.Data.Sqlite;

namespace Clean.Architecture.IntegrationTests.Data;

public abstract class BaseEfRepoTestFixture : IDisposable
{
  private readonly SqliteConnection _connection;

  protected AppDbContext _dbContext;

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
    _dbContext.Dispose();
    _connection.Dispose();

    GC.SuppressFinalize(this);
  }
}
