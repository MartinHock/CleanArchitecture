using Shouldly;

namespace Clean.Architecture.AspireTests.Tests;



public class AspireIntegrationTests
{
  // Follow the link below to write you tests with Aspire
  // https://learn.microsoft.com/en-us/dotnet/aspire/testing/write-your-first-test?pivots=xunit
  [Fact]
  public async Task GetWebResourceRootReturnsOkStatusCode()
  {
    // Arrange
    var builder = await DistributedApplicationTestingBuilder
      .CreateAsync<Projects.Clean_Architecture_AspireHost>();

    builder.Services.ConfigureHttpClientDefaults(clientBuilder =>
    {
      clientBuilder.AddStandardResilienceHandler();
    });

    await using var app = await builder.BuildAsync();

    await app.StartAsync();

    // Act
    Action act = () =>
    {
      using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
      var httpClient = app.CreateHttpClient("webfrontend");
      var result = httpClient.GetAsync("/", cts.Token).GetAwaiter().GetResult();
    };

    // Assert
    act.ShouldThrow<ArgumentException>();
  }
}
