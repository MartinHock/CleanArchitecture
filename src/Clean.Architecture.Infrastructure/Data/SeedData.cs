using Clean.Architecture.Core.ContributorAggregate;
using Clean.Architecture.Infrastructure.Data;

public static class SeedData
{
  public const int NUMBER_OF_CONTRIBUTORS = 27;

  public const string Contributor1Name = "Ardalis";
  public const string Contributor2Name = "Ilyana";

  public static async Task InitializeAsync(AppDbContext dbContext)
  {
    if (await dbContext.Contributors.AnyAsync()) return;

    await PopulateTestDataAsync(dbContext);
  }

  public static async Task PopulateTestDataAsync(AppDbContext dbContext)
  {
    var contributor1 = CreateContributor(Contributor1Name);
    var contributor2 = CreateContributor(Contributor2Name);

    dbContext.Contributors.AddRange(contributor1, contributor2);
    await dbContext.SaveChangesAsync();

    for (int i = 1; i <= NUMBER_OF_CONTRIBUTORS - 2; i++)
    {
      dbContext.Contributors.Add(CreateContributor($"Contributor {i}"));
    }

    await dbContext.SaveChangesAsync();
  }

  private static Contributor CreateContributor(string name)
  {
    return new Contributor(ContributorName.From(name));
  }
}
