using System.Net;
using Ardalis.HttpClientTestExtensions;
using Clean.Architecture.Infrastructure.Data;
using Clean.Architecture.Web.Contributors;
using FluentAssertions;
using Xunit;

namespace Clean.Architecture.FunctionalTests.ApiEndpoints;

[Collection("Sequential")]
public class ContributorGetById(CustomWebApplicationFactory<Program> factory)
  : IClassFixture<CustomWebApplicationFactory<Program>>
{
  private readonly HttpClient _client = factory.CreateClient();

  [Fact]
  public async Task ReturnsSeedContributorGivenId1()
  {
    // Act
    var result = await _client.GetAndDeserializeAsync<ContributorRecord>(GetContributorByIdRequest.BuildRoute(1));
    // Assert
    Assert.Equal(1, result.Id);
    Assert.Equal(SeedData.Contributor1.Name, result.Name);
  }

  [Fact]
  public async Task ReturnsNotFoundGivenId1000()
  {
    // Arrange
    var route = GetContributorByIdRequest.BuildRoute(1000);
    // Act
    var msg = await _client.GetAndEnsureNotFoundAsync(route);
    // Assert
    msg.StatusCode.Should().Be(HttpStatusCode.NotFound);
  }

  [Fact]
  public async Task ReturnsNotFoundGivenId1()
  {
    // Arrange
    var route = GetContributorByIdRequest.BuildRoute(1);
    // Act
    var msg = async () => await _client.GetAndEnsureNotFoundAsync(route);
    // Assert
    await msg.Should().ThrowAsync<HttpRequestException>().WithMessage("*Expected 404 NotFound*");
  }
}
