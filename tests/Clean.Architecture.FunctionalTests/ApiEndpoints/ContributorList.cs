using Clean.Architecture.Infrastructure.Data;
using Clean.Architecture.Web.Contributors;

namespace Clean.Architecture.FunctionalTests.ApiEndpoints;

[Collection("Sequential")]
public class ContributorList(CustomWebApplicationFactory<Program> factory)
  : IClassFixture<CustomWebApplicationFactory<Program>>
{
  private readonly HttpClient _client = factory.CreateClient();

  [Fact]
  public async Task ReturnsSeedContributors()
  {
    using var scope = factory.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    var seededContributors = dbContext.Contributors
      .AsEnumerable()
      .Select(c => c.Name.Value)
      .ToList();

    seededContributors.ShouldNotBeEmpty();

    var result = await _client.GetAndDeserializeAsync<ContributorListResponse>(
      "/Contributors");

    result.Items.ShouldNotBeNull();
    result.Items.ShouldNotBeEmpty();

    result.Items.ShouldContain(c => c.Name == SeedData.Contributor1Name);
    result.Items.ShouldContain(c => c.Name == SeedData.Contributor2Name);

    result.TotalCount.ShouldBe(SeedData.NUMBER_OF_CONTRIBUTORS);
  }
}
