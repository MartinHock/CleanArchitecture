using Clean.Architecture.Core.ContributorAggregate;

namespace Clean.Architecture.IntegrationTests.Data;

public class EfRepositoryAdd : BaseEfRepoTestFixture
{
  [Fact]
  public async Task AddsContributorAndSetsId()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var testContributorName = ContributorName.From("testContributor");
    var testContributorStatus = ContributorStatus.NotSet;
    var repository = GetRepository();

    await repository.AddAsync(Contributor, cancellationToken);

    var newContributor = (await repository.ListAsync(cancellationToken))
                    .FirstOrDefault();

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
    var cancellationToken = TestContext.Current.CancellationToken;
    var repository = GetRepository();

    var firstName = ContributorName.From($"first-{Guid.NewGuid()}");
    var secondName = ContributorName.From($"second-{Guid.NewGuid()}");

    var first = new Contributor(firstName);
    var second = new Contributor(secondName);

    await repository.AddRangeAsync([first, second]);

    var contributors = await repository.ListAsync();

    await repository.AddAsync(first, cancellationToken);
    await repository.AddAsync(second, cancellationToken);

    var all = await repository.ListAsync(cancellationToken);
    all.Count.ShouldBe(2);
    all[0].Id.Value.ShouldBeGreaterThan(0);
    all[1].Id.Value.ShouldBeGreaterThan(0);
    all[0].Id.ShouldNotBe(all[1].Id);
  }
}
