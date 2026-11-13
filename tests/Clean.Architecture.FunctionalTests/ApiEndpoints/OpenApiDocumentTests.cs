using System.Net;
using System.Text.Json;

namespace Clean.Architecture.FunctionalTests.ApiEndpoints;

[Collection("Sequential")]
public sealed class OpenApiDocumentTests(CustomWebApplicationFactory<Program> factory)
  : IClassFixture<CustomWebApplicationFactory<Program>>
{
  private readonly HttpClient _client = factory.CreateClient();

  [Fact]
  public async Task ServesValidOpenApiDocument()
  {
    using var response = await _client.GetAsync("/openapi/v1.json", TestContext.Current.CancellationToken);
    response.StatusCode.ShouldBe(HttpStatusCode.OK);

    await using var stream = await response.Content.ReadAsStreamAsync(TestContext.Current.CancellationToken);
    using var document = await JsonDocument.ParseAsync(stream, cancellationToken: TestContext.Current.CancellationToken);
    var root = document.RootElement;

    root.TryGetProperty("openapi", out var version).ShouldBeTrue();
    version.GetString().ShouldNotBeNullOrWhiteSpace();
    root.GetProperty("info").GetProperty("version").GetString().ShouldBe("v1");
    root.GetProperty("paths").EnumerateObject().Any().ShouldBeTrue();
  }
}
