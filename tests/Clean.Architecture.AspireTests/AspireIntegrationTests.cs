namespace Clean.Architecture.AspireTests.Tests;

public class AspireIntegrationTests
{
    [Fact]
    public async Task AppHostDefinesExpectedResources()
    {
        await using var builder = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.Clean_Architecture_AspireHost>();

        Assert.Contains(builder.Resources, resource => resource.Name == "sqlserver");
        Assert.Contains(builder.Resources, resource => resource.Name == "papercut");
        Assert.Contains(builder.Resources, resource => resource.Name == "web");
    }
}
