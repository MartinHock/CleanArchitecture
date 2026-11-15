# Analyzer profile PoC

Opt-in build: `dotnet build src/Clean.Architecture.Core/Clean.Architecture.Core.csproj -p:AnalyzerProfile=Strict`.
The profile is resolved from `matrix.json` into an intermediate `.globalconfig`.
Without `AnalyzerProfile`, existing builds are unchanged.

Precedence: `ruleOverrides` > `categoryOverrides` > selected profile category.
Supported severities: `none`, `silent`, `suggestion`, `warning`, `error`.
The six rule IDs are initial proof-of-concept examples, not a production coverage matrix.

Validate with `dotnet run --project analyzer-profiles/Generator -- verify analyzer-profiles/matrix.json`.
The generator is C# and has no PowerShell dependency.

Note: `.editorconfig` can override global analyzer config for the same rule; this PoC
must be validated against the full repository configuration before rollout.

## Diagnostic regression fixture

`analyzer-profiles/Fixture` intentionally contains a public instance method that triggers CA1822.
`dotnet run --project analyzer-profiles/DiagnosticVerifier/DiagnosticVerifier.csproj`
rebuilds it separately with `Default` and `Strict`, checks the actual compiler output,
and fails if CA1822 is absent. The fixture explicitly enables `AnalysisMode=All`
so it proves analyzer execution but **does not yet prove profile-specific severity**:
currently both profiles emit CA1822 as a warning despite `Default` specifying
`suggestion`. That mismatch remains an open acceptance blocker, not a passing
profile-severity test. The fixture is isolated from the production project.
