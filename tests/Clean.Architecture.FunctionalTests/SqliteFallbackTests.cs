using Clean.Architecture.Infrastructure.Data;
using Clean.Architecture.Web.Contributors;
using Microsoft.EntityFrameworkCore;

namespace Clean.Architecture.FunctionalTests;

public class SqliteFallbackTests
{
  [Fact]
  public async Task AutoProvider_FallsBackToSqlite_WhenDockerIsUnavailable()
  {
    await using var factory =
      CustomWebApplicationFactory<Program>.CreateWithDockerUnavailable(
        TestDatabaseProvider.Auto);

    await factory.InitializeAsync();
    using var client = factory.CreateClient();
    var result = await client.GetAndDeserializeAsync<ContributorRecord>(
      GetContributorByIdRequest.BuildRoute(1));

    factory.ActiveDatabaseProvider.ShouldBe(TestDatabaseProvider.Sqlite);
    result.Id.ShouldBe(1);
    result.Name.ShouldBe(SeedData.Contributor1Name.Value);
  }

  [Fact]
  public async Task ExplicitSqlServer_DoesNotFallback_WhenDockerIsUnavailable()
  {
    await using var factory =
      CustomWebApplicationFactory<Program>.CreateWithDockerUnavailable(
        TestDatabaseProvider.SqlServer);

    await Should.ThrowAsync<DotNet.Testcontainers.Builders.DockerUnavailableException>(
      async () => await factory.InitializeAsync());
  }

  [Fact]
  public async Task UsesSqlite_WhenSqliteProviderIsSelected()
  {
    await using var factory =
      CustomWebApplicationFactory<Program>.CreateWithProvider(
        TestDatabaseProvider.Sqlite);

    using var scope = factory.Services.CreateScope();

    var dbContext =
      scope.ServiceProvider.GetRequiredService<AppDbContext>();

    factory.ActiveDatabaseProvider
      .ShouldBe(TestDatabaseProvider.Sqlite);
    dbContext.Database.IsSqlite().ShouldBeTrue();
  }

  [Fact]
  public async Task ContributorEndpoint_WorksWithSqlite()
  {
    await using var factory =
      CustomWebApplicationFactory<Program>.CreateWithProvider(
        TestDatabaseProvider.Sqlite);

    using var client = factory.CreateClient();

    var result =
      await client.GetAndDeserializeAsync<ContributorRecord>(
        GetContributorByIdRequest.BuildRoute(1));

    factory.ActiveDatabaseProvider
      .ShouldBe(TestDatabaseProvider.Sqlite);
    result.Id.ShouldBe(1);
    result.Name.ShouldBe(SeedData.Contributor1Name.Value);
  }
}
