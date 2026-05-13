using Clean.Architecture.Core.ContributorAggregate;

namespace Clean.Architecture.IntegrationTests.Data;

public class EfRepositoryAdd : BaseEfRepoTestFixture
{
  [Fact]
  public async Task AddsContributorAndSetsId()
  {
    var testContributorName = ContributorName.From("testContributor");
    var testContributorStatus = ContributorStatus.NotSet;
    var repository = GetRepository();
    var Contributor = new Contributor(testContributorName);

    await repository.AddAsync(Contributor, TestContext.Current.CancellationToken);

    var newContributor = (await repository.ListAsync(TestContext.Current.CancellationToken))
                    .FirstOrDefault();

    newContributor.ShouldNotBeNull();
    testContributorName.ShouldBe(newContributor.Name);
    testContributorStatus.ShouldBe(newContributor.Status);
    newContributor.Id.Value.ShouldBeGreaterThan(0);
  }

  [Fact]
  public async Task AddsTwoContributorsWithDistinctDbGeneratedIds()
  {
    var repository = GetRepository();
    var first = new Contributor(ContributorName.From("first"));
    var second = new Contributor(ContributorName.From("second"));

    await repository.AddAsync(first, TestContext.Current.CancellationToken);
    await repository.AddAsync(second, TestContext.Current.CancellationToken);

    var all = await repository.ListAsync(TestContext.Current.CancellationToken);
    all.Count.ShouldBe(2);
    all[0].Id.Value.ShouldBeGreaterThan(0);
    all[1].Id.Value.ShouldBeGreaterThan(0);
    all[0].Id.ShouldNotBe(all[1].Id);
  }
}
