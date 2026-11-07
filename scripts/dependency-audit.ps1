param([string]$RepositoryRoot=(Resolve-Path (Join-Path $PSScriptRoot '..')).Path,[string]$OutputDirectory=(Join-Path $env:TEMP 'ardalis-dependency-audit-results'),[switch]$IncludeTransitive)
$ErrorActionPreference='Stop'
New-Item -ItemType Directory -Force $OutputDirectory | Out-Null
$solutions=@(@{Name='Root';Path='Clean.Architecture.slnx';Key='root'},@{Name='MinimalClean';Path='MinimalClean/MinimalClean.Architecture.slnx';Key='minimal'},@{Name='Sample';Path='sample/NimblePros.SampleToDo.slnx';Key='sample'})
$results=@()
foreach($s in $solutions){
  $report=Join-Path $OutputDirectory ($s.Key+'.json')
  $log=Join-Path $OutputDirectory ($s.Key+'.log')
  Remove-Item $report -ErrorAction SilentlyContinue
  $a=@('outdated',(Join-Path $RepositoryRoot $s.Path),'--pre-release','Never','--output',$report,'--output-format','json')
  if($IncludeTransitive){$a+='--transitive'}
  try { $ErrorActionPreference='Continue'; & dotnet @a *> $log; $code=$LASTEXITCODE } catch { $_ | Out-String | Add-Content $log; $code=1 } finally { $ErrorActionPreference='Stop' }
  $deps=@()
  if($code -eq 0 -and (Test-Path $report)){
    $j=Get-Content $report -Raw|ConvertFrom-Json
    $deps=@($j.Projects|ForEach-Object {$_.TargetFrameworks}|ForEach-Object {$_.Dependencies}|Where-Object {$null -ne $_}|Sort-Object Name -Unique)
  }
  $results += [pscustomobject]@{Solution=$s.Name;Status=$(if($code -eq 0){'OK'}else{'FAILED'});ExitCode=$code;Packages=$deps.Count;Major=@($deps|Where-Object UpgradeSeverity -eq 'Major').Count;Minor=@($deps|Where-Object UpgradeSeverity -eq 'Minor').Count;Patch=@($deps|Where-Object UpgradeSeverity -eq 'Patch').Count}
}
$results|ConvertTo-Json -Depth 4|Set-Content (Join-Path $OutputDirectory 'summary.json') -Encoding utf8
$lines=@('# Dependency audit (read-only)','','No package upgrade is performed.','','| Solution | Status | Distinct packages | Major | Minor | Patch |','|---|---|---:|---:|---:|---:|')
foreach($r in $results){$lines+="| $($r.Solution) | $($r.Status) | $($r.Packages) | $($r.Major) | $($r.Minor) | $($r.Patch) |"}
$lines+='','Failed solutions have no complete inventory; inspect their log.',"Transitive analysis: $([bool]$IncludeTransitive)"
$lines|Set-Content (Join-Path $OutputDirectory 'summary.md') -Encoding utf8
$results|Format-Table -AutoSize
if(@($results|Where-Object Status -eq 'FAILED').Count -gt 0){exit 1}


