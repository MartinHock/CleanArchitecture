using Clean.Architecture.Core.ContributorAggregate;

namespace Clean.Architecture.IntegrationTests.Data;

public class EfRepositoryDelete : BaseEfRepoTestFixture
{
  [Fact]
  public async Task DeletesItemAfterAddingIt()
  {
    var repository = GetRepository();

    var initialName = ContributorName.From(Guid.NewGuid().ToString());
    var contributor = new Contributor(initialName);

    await repository.AddAsync(contributor);

    var persistedContributor = (await repository.ListAsync())
      .Single(c => c.Name == initialName);

    await repository.DeleteAsync(persistedContributor);

    var contributors = await repository.ListAsync();

    contributors.ShouldNotContain(c => c.Name == initialName);
  }
}
