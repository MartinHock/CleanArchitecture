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
and fails if CA1822 is absent. The fixture performs clean rebuilds for all four profiles and checks actual CA1822/CA2000 severities and build exit codes. `Default` suppresses CA1822 build output and reports CA2000 as a warning; `Strict` and `Regulated` report CA2000 as an error; `AllEnabled` reports both rules as warnings. This proves the tested severities, not every rule in the matrix. The generated globalconfig is supplied to the compiler through `EditorConfigFiles`; `GlobalAnalyzerConfigFiles` alone was not passed to csc in the tested SDK. The fixture is isolated from the production project.
