using Clean.Architecture.Infrastructure.Data;
using Clean.Architecture.Web.Contributors;


namespace Clean.Architecture.FunctionalTests.ApiEndpoints;

[Collection("Sequential")]
public class ContributorGetById(CustomWebApplicationFactory<Program> factory) : IClassFixture<CustomWebApplicationFactory<Program>>
{
  private readonly HttpClient _client = factory.CreateClient();

  [Fact]
  public async Task ReturnsSeedContributorGivenId()
  {
    using var scope = factory.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    var contributors = dbContext.Contributors
      .AsEnumerable()
      .ToList();

    var seedContributor = contributors
      .Single(c => c.Name.Value == SeedData.Contributor1Name);

    var result = await _client.GetAndDeserializeAsync<ContributorRecord>(
      GetContributorByIdRequest.BuildRoute(seedContributor.Id.Value));

    result.Id.ShouldBe(seedContributor.Id.Value);
    result.Name.ShouldBe(SeedData.Contributor1Name);
  }

  [Fact]
  public async Task ReturnsNotFoundGivenId1000()
  {
    string route = GetContributorByIdRequest.BuildRoute(1000);
    _ = await _client.GetAndEnsureNotFoundAsync(route);
  }
}
