using Clean.Architecture.Core.ContributorAggregate;
using Clean.Architecture.Infrastructure.Data;

public static class SeedData
{
  public const int NUMBER_OF_CONTRIBUTORS = 27; // including the 2 below
  public static readonly ContributorName Contributor1Name = ContributorName.From("Ardalis");
  public static readonly ContributorName Contributor2Name = ContributorName.From("Ilyana");

  public const string Contributor1Name = "Ardalis";
  public const string Contributor2Name = "Ilyana";

  public static async Task InitializeAsync(AppDbContext dbContext)
  {
    if (await dbContext.Contributors.AnyAsync()) return;

    await PopulateTestDataAsync(dbContext);
  }

  public static async Task PopulateTestDataAsync(AppDbContext dbContext)
  {
    // Use SQL inserts to avoid key generation/conversion issues with value object IDs.
    await dbContext.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO [Contributors] ([Name], [Status]) VALUES ({Contributor1Name.Value}, {ContributorStatus.NotSet.Value})");
    await dbContext.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO [Contributors] ([Name], [Status]) VALUES ({Contributor2Name.Value}, {ContributorStatus.NotSet.Value})");

    // Add a bunch more contributors to support demonstrating paging.
    for (int i = 1; i <= NUMBER_OF_CONTRIBUTORS - 2; i++)
    {
      await dbContext.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO [Contributors] ([Name], [Status]) VALUES ({$"Contributor {i}"}, {ContributorStatus.NotSet.Value})");
    }
  }

  private static Contributor CreateContributor(string name)
  {
    return new Contributor(ContributorName.From(name));
  }
}
