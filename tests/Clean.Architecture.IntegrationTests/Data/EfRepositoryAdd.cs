using Clean.Architecture.Core.ContributorAggregate;

namespace Clean.Architecture.IntegrationTests.Data;

public class EfRepositoryAdd : BaseEfRepoTestFixture
{
  [Fact]
  public async Task AddsContributorAndSetsId()
  {
    var repository = GetRepository();

    var testContributorName = ContributorName.From($"testContributor-{Guid.NewGuid()}");
    var expectedStatus = ContributorStatus.NotSet;

    var contributor = new Contributor(testContributorName);

    await repository.AddAsync(contributor);

    var contributors = await repository.ListAsync();

    var newContributor = contributors
      .Single(c => c.Name == testContributorName);

    newContributor.ShouldNotBeNull();
    newContributor.Name.ShouldBe(testContributorName);
    newContributor.Status.ShouldBe(expectedStatus);
    newContributor.Id.Value.ShouldBeGreaterThan(0);
  }

  [Fact]
  public async Task AddsTwoContributorsWithDistinctDbGeneratedIds()
  {
    var repository = GetRepository();

    var firstName = ContributorName.From($"first-{Guid.NewGuid()}");
    var secondName = ContributorName.From($"second-{Guid.NewGuid()}");

    var first = new Contributor(firstName);
    var second = new Contributor(secondName);

    await repository.AddRangeAsync([first, second]);

    var contributors = await repository.ListAsync();

    var addedFirst = contributors.Single(c => c.Name == firstName);
    var addedSecond = contributors.Single(c => c.Name == secondName);

    addedFirst.Id.Value.ShouldBeGreaterThan(0);
    addedSecond.Id.Value.ShouldBeGreaterThan(0);
    addedFirst.Id.ShouldNotBe(addedSecond.Id);
  }
}
