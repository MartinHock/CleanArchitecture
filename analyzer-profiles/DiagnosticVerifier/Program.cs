using System.Diagnostics;

var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../"));
var fixture = Path.Combine(root, "analyzer-profiles", "Fixture", "Fixture.csproj");
var failures = 0;
foreach (var (profile, expectedSeverity) in new[] { ("Default", "none"), ("Strict", "warning") })
{
    var start = new ProcessStartInfo("dotnet")
    {
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
        WorkingDirectory = root
    };
    foreach (var arg in new[] { "build", fixture, "--no-restore", "-t:Rebuild", $"-p:AnalyzerProfile={profile}", "-p:TreatWarningsAsErrors=false", "-v:minimal" })
        start.ArgumentList.Add(arg);
    using var process = Process.Start(start) ?? throw new InvalidOperationException("Could not start dotnet");
    var outputTask = process.StandardOutput.ReadToEndAsync();
    var errorTask = process.StandardError.ReadToEndAsync();
    await process.WaitForExitAsync();
    var output = await outputTask + await errorTask;
    var diagnostic = output.Split('\n').FirstOrDefault(line => line.Contains("CA1822:", StringComparison.Ordinal));
    var valid = process.ExitCode == 0 && (expectedSeverity == "none"
        ? diagnostic is null
        : diagnostic is not null && diagnostic.Contains($"{expectedSeverity} CA1822:", StringComparison.Ordinal));
    if (!valid)
    {
        Console.Error.WriteLine($"FAIL {profile}: expected CA1822={expectedSeverity}, exit 0; actual: {diagnostic ?? "absent"}.\n{output}");
        failures++;
    }
    else
        Console.WriteLine($"PASS {profile}: CA1822={expectedSeverity}");
}
return failures == 0 ? 0 : 1;
