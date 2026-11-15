using System.Diagnostics;

var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../"));
var fixture = Path.Combine(root, "analyzer-profiles", "Fixture", "Fixture.csproj");
var cases = new[]
{
    (Profile: "Default", Ca1822: "none", Ca2000: "warning", Ca1062: "none", Ca1502: "none", ExitCode: 0),
    (Profile: "Strict", Ca1822: "warning", Ca2000: "error", Ca1062: "warning", Ca1502: "warning", ExitCode: 1),
    (Profile: "Regulated", Ca1822: "warning", Ca2000: "error", Ca1062: "warning", Ca1502: "error", ExitCode: 1),
    (Profile: "AllEnabled", Ca1822: "warning", Ca2000: "warning", Ca1062: "warning", Ca1502: "warning", ExitCode: 0)
};
var failures = 0;
foreach (var test in cases)
{
    var start = new ProcessStartInfo("dotnet")
    {
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
        WorkingDirectory = root
    };
    foreach (var arg in new[] { "build", fixture, "--no-restore", "-t:Rebuild", $"-p:AnalyzerProfile={test.Profile}", "-p:TreatWarningsAsErrors=false", "-v:minimal" })
        start.ArgumentList.Add(arg);
    using var process = Process.Start(start) ?? throw new InvalidOperationException("Could not start dotnet");
    var outputTask = process.StandardOutput.ReadToEndAsync();
    var errorTask = process.StandardError.ReadToEndAsync();
    await process.WaitForExitAsync();
    var output = await outputTask + await errorTask;
    var valid = process.ExitCode == test.ExitCode && Matches(output, "CA1822", test.Ca1822) && Matches(output, "CA2000", test.Ca2000) && Matches(output, "CA1062", test.Ca1062) && Matches(output, "CA1502", test.Ca1502);
    if (!valid)
    {
        Console.Error.WriteLine($"FAIL {test.Profile}: expected CA1822={test.Ca1822}, CA2000={test.Ca2000}, CA1062={test.Ca1062}, CA1502={test.Ca1502}, exit={test.ExitCode}; actual exit={process.ExitCode}.\n{output}");
        failures++;
    }
    else
        Console.WriteLine($"PASS {test.Profile}: CA1822={test.Ca1822}, CA2000={test.Ca2000}, CA1062={test.Ca1062}, CA1502={test.Ca1502}, exit={test.ExitCode}");
}
// Verify that a file-local .editorconfig overrides the generated global configuration.
var editorConfigPath = Path.Combine(root, "analyzer-profiles", "Fixture", ".editorconfig");
if (File.Exists(editorConfigPath))
    throw new InvalidOperationException("Fixture .editorconfig already exists; refusing to overwrite it.");
try
{
    await File.WriteAllTextAsync(editorConfigPath, "root = true\n\n[*.cs]\ndotnet_diagnostic.CA1822.severity = none\n");
    var start = new ProcessStartInfo("dotnet")
    {
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
        WorkingDirectory = root
    };
    foreach (var arg in new[] { "build", fixture, "--no-restore", "-t:Rebuild", "-p:AnalyzerProfile=Strict", "-p:TreatWarningsAsErrors=false", "-v:minimal" })
        start.ArgumentList.Add(arg);
    using var process = Process.Start(start) ?? throw new InvalidOperationException("Could not start dotnet");
    var outputTask = process.StandardOutput.ReadToEndAsync();
    var errorTask = process.StandardError.ReadToEndAsync();
    await process.WaitForExitAsync();
    var output = await outputTask + await errorTask;
    if (process.ExitCode != 1 || !Matches(output, "CA1822", "none") || !Matches(output, "CA2000", "error"))
    {
        Console.Error.WriteLine($"FAIL editorconfig precedence: expected CA1822 suppressed and CA2000 error; exit={process.ExitCode}.\n{output}");
        failures++;
    }
    else
        Console.WriteLine("PASS editorconfig precedence: CA1822 suppressed while CA2000 remains error");
}
finally
{
    File.Delete(editorConfigPath);
}
// Slice B: isolated candidate diagnostics; production matrix remains unchanged.
var candidateFixture = Path.Combine(root, "analyzer-profiles", "CandidateFixture", "CandidateFixture.csproj");
var candidateStart = new ProcessStartInfo("dotnet")
{
    RedirectStandardOutput = true,
    RedirectStandardError = true,
    UseShellExecute = false,
    WorkingDirectory = root
};
foreach (var arg in new[] { "build", candidateFixture, "--no-restore", "-t:Rebuild", "-p:TreatWarningsAsErrors=false", "-v:minimal" })
    candidateStart.ArgumentList.Add(arg);
using (var candidateProcess = Process.Start(candidateStart) ?? throw new InvalidOperationException("Could not start candidate build"))
{
    var candidateStdout = candidateProcess.StandardOutput.ReadToEndAsync();
    var candidateStderr = candidateProcess.StandardError.ReadToEndAsync();
    await candidateProcess.WaitForExitAsync();
    var candidateOutput = await candidateStdout + await candidateStderr;
    var expected = new[] { "CA1050", "CA2208", "CA1508" };
    var valid = candidateProcess.ExitCode == 0 && expected.All(rule => Matches(candidateOutput, rule, "warning"));
    if (!valid)
    {
        Console.Error.WriteLine($"FAIL candidate diagnostics: expected warnings for {string.Join(", ", expected)}, exit=0; actual exit={candidateProcess.ExitCode}.\n{candidateOutput}");
        failures++;
    }
    else
        Console.WriteLine("PASS candidate diagnostics: CA1050, CA2208, CA1508 are warnings, exit=0");
}
return failures == 0 ? 0 : 1;

static bool Matches(string output, string rule, string severity)
{
    var diagnostics = output.Split('\n').Where(line => line.Contains($"{rule}:", StringComparison.Ordinal)).ToArray();
    return severity == "none"
        ? diagnostics.Length == 0
        : diagnostics.Length > 0 && diagnostics.All(line => line.Contains($"{severity} {rule}:", StringComparison.Ordinal));
}
