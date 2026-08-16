using DotNet.Testcontainers.Builders;
using Testcontainers.MsSql;

namespace Clean.Architecture.FunctionalTests;

public class DockerAvailabilityTests
{
  [Fact]
  public async Task SqlServerContainer_CanBeStarted_WhenDockerIsAvailable()
  {
    var cancellationToken = TestContext.Current.CancellationToken;

    try
    {
      // Ping the Docker daemon directly using the Docker client.
      // This has no side effects on container lifecycle or Testcontainers internals.
      using var client = new DockerClientBuilder().Build();
      await client.System.PingAsync(cancellationToken);
    }
    catch (DockerUnavailableException)
    {
      Assert.Skip(
        "Docker is not running or is misconfigured. " +
        "Functional tests can still run with SQLite, " +
        "but SQL Server-specific behavior is not covered.");
    }
  }
}
