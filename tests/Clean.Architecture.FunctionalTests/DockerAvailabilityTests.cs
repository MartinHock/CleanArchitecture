using Docker.DotNet;

namespace Clean.Architecture.FunctionalTests;

public class DockerAvailabilityTests
{
  [Fact]
  public async Task Docker_ShouldBeRunning_ForFullFunctionalTestCoverage()
  {
    var cancellationToken = TestContext.Current.CancellationToken;

    using var configuration = new DockerClientConfiguration();
    using var client = configuration.CreateClient();

    var exception = await Record.ExceptionAsync(
      () => client.System.PingAsync(cancellationToken))
      .ConfigureAwait(true);

    if (exception is not null)
    {
      Assert.Fail(
        "Docker is not running or is misconfigured. " +
        "Functional tests that use SQL Server will fall back to SQLite, " +
        "which may not catch SQL Server-specific issues. " +
        "For full test coverage, please start Docker Desktop and re-run the tests. " +
        $"Underlying error: {exception.Message}");
    }
  }
}
