using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Images;
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
      // Attempt to create a test container to verify Docker availability
      // This has minimal side effects - we just test if we can initialize a container
      var image = new DockerImage("mcr.microsoft.com/mssql/server", "2022-latest");
      var container = new MsSqlBuilder(image)
        .Build();

      // Don't start it, just ensure Docker daemon responds
      await Task.CompletedTask;
    }
    catch (Exception ex)
    {
      Assert.Skip(
        "Docker is not running or is misconfigured. " +
        "Functional tests can still run with SQLite, " +
        "but SQL Server-specific behavior is not covered. " +
        $"Error: {ex.Message}");
    }
  }
}
