using Clean.Architecture.Core.ContributorAggregate;

namespace Clean.Architecture.IntegrationTests.Data;

public class EfRepositoryDelete : BaseEfRepoTestFixture
{
  [Fact]
  public async Task DeletesItemAfterAddingIt()
  {
    // add a Contributor
    var repository = GetRepository();
    var initialName = ContributorName.From(Guid.NewGuid().ToString());
    var Contributor = new Contributor(initialName);
    await repository.AddAsync(Contributor, TestContext.Current.CancellationToken);

    // delete the item
    await repository.DeleteAsync(Contributor, TestContext.Current.CancellationToken);

    // verify it's no longer there
    (await repository.ListAsync(TestContext.Current.CancellationToken)).ShouldNotContain(Contributor => Contributor.Name == initialName);
  }
}
