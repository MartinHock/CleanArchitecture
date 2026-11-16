# Optional local CI gate; invoked by scripts/git-hooks/pre-push.
# Never publishes GitHub commit statuses or changes branch protection.
param([Parameter(Mandatory = $true)][string]$RepositoryRoot)
$ErrorActionPreference = 'Stop'
$watch = [Diagnostics.Stopwatch]::StartNew()
function Invoke-Dotnet([string]$Label, [string[]]$Arguments) {
    Write-Host "Local CI: $Label"
    & dotnet @Arguments
    if ($LASTEXITCODE -ne 0) { throw "$Label failed with exit code $LASTEXITCODE" }
}
try {
    if (-not (Test-Path (Join-Path $RepositoryRoot 'Clean.Architecture.slnx'))) {
        throw "Clean.Architecture.slnx not found at $RepositoryRoot"
    }
    Push-Location $RepositoryRoot
    try {
        Invoke-Dotnet 'restore' @('restore', 'Clean.Architecture.slnx', '--verbosity', 'quiet')
        Invoke-Dotnet 'Release build' @('build', 'Clean.Architecture.slnx', '--configuration', 'Release', '--no-restore', '--verbosity', 'quiet')
        Invoke-Dotnet 'unit tests (xUnit v3 / MTP)' @('test', '--project', 'tests/Clean.Architecture.UnitTests/Clean.Architecture.UnitTests.csproj', '--configuration', 'Release', '--no-build', '--minimum-expected-tests', '1')
        Write-Host "Local CI PASS ($([math]::Round($watch.Elapsed.TotalSeconds, 1))s)"
        exit 0
    } finally { Pop-Location }
} catch {
    [Console]::Error.WriteLine("Local CI BLOCK: $($_.Exception.Message) ($([math]::Round($watch.Elapsed.TotalSeconds, 1))s)")
    exit 1
}
