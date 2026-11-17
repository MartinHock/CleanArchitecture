using System.Text.Json;
using System.Text.RegularExpressions;
using Shouldly;
using Xunit;

public sealed class RepositoryEditorConfigTests
{
    private static readonly string Root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../"));

    [Fact]
    public void MatrixRulesMustNotHaveConflictingRepositoryEditorConfigSeverities()
    {
        using var matrix = JsonDocument.Parse(File.ReadAllText(Path.Combine(Root, "matrix.json")));
        var rules = matrix.RootElement.GetProperty("rules").EnumerateObject()
            .Select(rule => rule.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var repository = Path.GetFullPath(Path.Combine(Root, ".."));
        var configs = new[]
        {
            ".editorconfig",
            "MinimalClean/.editorconfig",
            "sample/.editorconfig"
        };
        var collisions = new List<string>();
        foreach (var relative in configs)
        {
            var path = Path.Combine(repository, relative.Replace('/', Path.DirectorySeparatorChar));
            File.Exists(path).ShouldBeTrue($"Expected repository editorconfig: {relative}");
            var lineNumber = 0;
            foreach (var line in File.ReadLines(path))
            {
                lineNumber++;
                var match = Regex.Match(line, @"^\s*dotnet_diagnostic\.([A-Za-z]+\d+)\.severity\s*=", RegexOptions.IgnoreCase);
                if (match.Success && rules.Contains(match.Groups[1].Value))
                    collisions.Add($"{relative}:{lineNumber}: {match.Groups[1].Value}");
            }
        }
        collisions.ShouldBeEmpty("Repository .editorconfig severity overrides can take precedence over generated .globalconfig. Review any collision explicitly.");
    }
}

