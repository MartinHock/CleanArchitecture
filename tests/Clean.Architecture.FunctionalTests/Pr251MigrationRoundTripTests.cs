using Microsoft.Data.SqlClient;
using Testcontainers.MsSql;
using Xunit;

namespace Clean.Architecture.FunctionalTests;

public sealed class Pr251MigrationRoundTripTests
{
  [Fact]
  public async Task SqlServer_Up_Down_PreservesDataAndIdentity()
  {
    if (!OperatingSystem.IsLinux()) Assert.Skip("SQL Server Testcontainers requires a Linux Docker runner.");
    var ct = TestContext.Current.CancellationToken;
    await using var container = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2025-latest")
      .WithPassword("Your_password123!").Build();
    await container.StartAsync(ct);
    await using var connection = new SqlConnection(container.GetConnectionString());
    await connection.OpenAsync(ct);
    await ExecuteAsync(connection, """
      CREATE TABLE [Contributors] (
        [Id] int NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        [Status] int NOT NULL,
        [PhoneNumber_CountryCode] nvarchar(max) NULL,
        [PhoneNumber_Number] nvarchar(max) NULL,
        [PhoneNumber_Extension] nvarchar(max) NULL,
        CONSTRAINT [PK_Contributors] PRIMARY KEY ([Id]));
      CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL PRIMARY KEY,
        [ProductVersion] nvarchar(32) NOT NULL);
      INSERT INTO [__EFMigrationsHistory] VALUES
        (N'20251113164108_UpdateForNet10', N'11.0.0');
      INSERT INTO [Contributors] ([Id],[Name],[Status]) VALUES
        (42,N'Existing contributor',1);
      """,ct);

    await RunScriptAsync(connection, Path.Combine(AppContext.BaseDirectory, "MigrationScripts", "Pr251Up.sql"),ct);
    Assert.Equal(1, await ScalarAsync(connection, "SELECT COUNT(*) FROM [Contributors] WHERE [Id]=42 AND [Name]=N'Existing contributor'",ct));
    Assert.Equal(1, await ScalarAsync(connection, "SELECT CONVERT(int, COLUMNPROPERTY(OBJECT_ID('Contributors'), 'Id', 'IsIdentity'))",ct));
    await ExecuteAsync(connection, "INSERT INTO [Contributors] ([Name],[Status]) VALUES (N'Generated',1)",ct);
    Assert.Equal(1, await ScalarAsync(connection, "SELECT COUNT(*) FROM [Contributors] WHERE [Name]=N'Generated' AND [Id]>42",ct));
    await RunScriptAsync(connection, Path.Combine(AppContext.BaseDirectory, "MigrationScripts", "Pr251Down.sql"),ct);
    Assert.Equal(2, await ScalarAsync(connection, "SELECT COUNT(*) FROM [Contributors]",ct));
    Assert.Equal(0, await ScalarAsync(connection, "SELECT CONVERT(int, COLUMNPROPERTY(OBJECT_ID('Contributors'), 'Id', 'IsIdentity'))",ct));
    Assert.Equal(0, await ScalarAsync(connection, "SELECT COUNT(*) FROM [__EFMigrationsHistory] WHERE [MigrationId]=N'20260424000000_UseDbGeneratedIds'",ct));
  }

  private static async Task RunScriptAsync(SqlConnection connection, string path, CancellationToken ct)
  {
    var script = await File.ReadAllTextAsync(path,ct);
    foreach (var batch in System.Text.RegularExpressions.Regex.Split(script, @"(?im)^\s*GO\s*$"))
      if (!string.IsNullOrWhiteSpace(batch)) await ExecuteAsync(connection,batch,ct);
  }

  private static async Task ExecuteAsync(SqlConnection connection, string sql, CancellationToken ct)
  {
    await using var command = new SqlCommand(sql,connection);
    await command.ExecuteNonQueryAsync(ct);
  }

  private static async Task<int> ScalarAsync(SqlConnection connection, string sql, CancellationToken ct)
  {
    await using var command = new SqlCommand(sql,connection);
    return Convert.ToInt32(await command.ExecuteScalarAsync(ct));
  }
}
