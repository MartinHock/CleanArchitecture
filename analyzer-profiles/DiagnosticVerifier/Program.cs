using System.Diagnostics;

var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../"));
var fixture = Path.Combine(root, "analyzer-profiles", "Fixture", "Fixture.csproj");
var failures = 0;
foreach (var profile in new[] { "Default", "Strict" })
{
    var start = new ProcessStartInfo("dotnet")
    {
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
        WorkingDirectory = root
    };
    foreach (var arg in new[] { "build", fixture, "--no-restore", "-t:Rebuild", $"-p:AnalyzerProfile={profile}", "-p:AnalysisMode=All", "-p:TreatWarningsAsErrors=false", "-v:minimal" })
        start.ArgumentList.Add(arg);
    using var process = Process.Start(start) ?? throw new InvalidOperationException("Could not start dotnet");
    var outputTask = process.StandardOutput.ReadToEndAsync();
    var errorTask = process.StandardError.ReadToEndAsync();
    await process.WaitForExitAsync();
    var output = await outputTask + await errorTask;
    var diagnostic = output.Split('\n').FirstOrDefault(line => line.Contains("CA1822:", StringComparison.Ordinal));
    if (process.ExitCode != 0 || diagnostic is null)
    {
        Console.Error.WriteLine($"FAIL {profile}: expected CA1822 diagnostic and successful build.\n{output}");
        failures++;
    }
    else
        Console.WriteLine($"PASS {profile}: {diagnostic.Trim()}");
}
return failures == 0 ? 0 : 1;
