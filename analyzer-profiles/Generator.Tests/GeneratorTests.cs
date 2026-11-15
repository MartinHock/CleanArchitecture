using System.Diagnostics;
using System.Text.Json.Nodes;
using Shouldly;
using Xunit;

public sealed class GeneratorTests
{
    private static readonly string Root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../"));
    private static readonly string Matrix = Path.Combine(Root, "matrix.json");
    private static readonly string Generator = Path.Combine(AppContext.BaseDirectory, "Generator.dll");

    [Theory]
    [InlineData("Default", "suggestion", "suggestion", "warning", "warning", "suggestion", "silent")]
    [InlineData("Strict", "warning", "warning", "error", "error", "warning", "suggestion")]
    [InlineData("Regulated", "warning", "warning", "error", "error", "error", "warning")]
    [InlineData("AllEnabled", "warning", "warning", "warning", "warning", "warning", "warning")]
    public async Task Generates_expected_severities(string profile, string design, string performance, string security, string reliability, string maintainability, string style)
    {
        using var temp = new Temp();
        var output = Path.Combine(temp.Directory, "nested", "output.globalconfig");
        var result = await Run("generate", Matrix, profile, output);
        result.Code.ShouldBe(0, result.Error);
        var content = File.ReadAllText(output);
        content.ShouldContain("is_global = true");
        content.ShouldContain("global_level = 100");
        foreach (var (id, severity) in new[] { ("CA1062", design), ("CA1822", performance), ("CA2100", security), ("CA2000", reliability), ("CA1502", maintainability), ("IDE0005", style) })
            content.ShouldContain($"dotnet_diagnostic.{id}.severity = {severity}");
    }

    [Theory]
    [InlineData(true, "none")]
    [InlineData(false, "error")]
    public async Task Overrides_follow_rule_category_profile_order(bool ruleOverride, string expected)
    {
        using var temp = new Temp();
        var node = JsonNode.Parse(File.ReadAllText(Matrix))!;
        node["categoryOverrides"]!["Design"] = "error";
        if (ruleOverride) node["ruleOverrides"]!["CA1062"] = "none";
        var input = Path.Combine(temp.Directory, "matrix.json");
        var output = Path.Combine(temp.Directory, "output.globalconfig");
        File.WriteAllText(input, node.ToJsonString());
        (await Run("generate", input, "Default", output)).Code.ShouldBe(0);
        (File.ReadAllText(output)).ShouldContain($"dotnet_diagnostic.CA1062.severity = {expected}");
    }

    [Fact]
    public async Task Output_is_deterministic_sorted_and_utf8_without_bom()
    {
        using var temp = new Temp();
        var a = Path.Combine(temp.Directory, "a.globalconfig");
        var b = Path.Combine(temp.Directory, "b.globalconfig");
        (await Run("generate", Matrix, "Strict", a)).Code.ShouldBe(0);
        (await Run("generate", Matrix, "Strict", b)).Code.ShouldBe(0);
        var bytes = File.ReadAllBytes(a);
        bytes.ShouldBe(File.ReadAllBytes(b));
        bytes.Take(3).ToArray().ShouldNotBe(new byte[] { 0xEF, 0xBB, 0xBF });
        var lines = (File.ReadAllLines(a)).Where(x => x.StartsWith("dotnet_diagnostic.", StringComparison.Ordinal)).ToArray();
        lines.ShouldBe(lines.OrderBy(x => x, StringComparer.Ordinal).ToArray());
    }

    [Theory]
    [InlineData("unknown-profile", "Unknown profile")]
    [InlineData("unknown-category", "Unknown category")]
    [InlineData("invalid-severity", "Invalid severity")]
    [InlineData("invalid-json", "")]
    public async Task Invalid_input_fails_without_writing_output(string scenario, string message)
    {
        using var temp = new Temp();
        var input = Path.Combine(temp.Directory, "matrix.json");
        var output = Path.Combine(temp.Directory, "output.globalconfig");
        var node = JsonNode.Parse(File.ReadAllText(Matrix))!;
        if (scenario == "unknown-category") node["rules"]!["CA1062"] = "MissingCategory";
        if (scenario == "invalid-severity") node["profiles"]!["Default"]!["Design"] = "fatal";
        File.WriteAllText(input, scenario == "invalid-json" ? "not json" : node.ToJsonString());
        var result = await Run("generate", input, scenario == "unknown-profile" ? "Unknown" : "Default", output);
        result.Code.ShouldBe(1);
        if (message.Length > 0) result.Error.ShouldContain(message);
        File.Exists(output).ShouldBeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData("generate")]
    [InlineData("generate|matrix.json|Default")]
    [InlineData("verify|matrix.json|extra")]
    [InlineData("unknown|matrix.json")]
    public async Task Invalid_arguments_report_usage(string arguments)
    {
        var result = await Run(arguments.Length == 0 ? [] : arguments.Split('|'));
        result.Code.ShouldBe(2);
        result.Error.ShouldContain("Usage:");
    }

    [Fact]
    public async Task Verify_passes_on_repository_matrix()
    {
        var result = await Run("verify", Matrix);
        result.Code.ShouldBe(0, result.Error);
        result.Output.ShouldContain("PASS:");
    }

    private static async Task<(int Code, string Output, string Error)> Run(params string[] args)
    {
        var start = new ProcessStartInfo("dotnet") { RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false };
        start.ArgumentList.Add(Generator);
        foreach (var arg in args) start.ArgumentList.Add(arg);
        using var process = Process.Start(start) ?? throw new InvalidOperationException("Generator process could not start");
        var stdout = process.StandardOutput.ReadToEndAsync();
        var stderr = process.StandardError.ReadToEndAsync();
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        await process.WaitForExitAsync(timeout.Token);
        return (process.ExitCode, await stdout, await stderr);
    }

    private sealed class Temp : IDisposable
    {
        public string Directory { get; } = Path.Combine(Path.GetTempPath(), "analyzer-generator-tests", Guid.NewGuid().ToString("N"));
        public Temp() => System.IO.Directory.CreateDirectory(Directory);
        public void Dispose() => System.IO.Directory.Delete(Directory, recursive: true);
    }
}
