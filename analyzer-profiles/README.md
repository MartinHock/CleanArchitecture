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

Run `dotnet run --project analyzer-profiles/DiagnosticVerifier/DiagnosticVerifier.csproj`.
The isolated fixture deliberately triggers CA1822 (Performance), CA2000 (Reliability), CA1062 (Design), and CA1502 (Maintainability).
The verifier clean-rebuilds the fixture for all four profiles, checks actual Roslyn diagnostic severities and build exit codes:

| Profile | CA1822 | CA2000 | CA1062 | CA1502 | Build exit |
| --- | --- | --- | --- | --- | --- |
| Default | No build diagnostic (suggestion) | warning | No build diagnostic (suggestion) | No build diagnostic (suggestion) | 0 |
| Strict | warning | error | warning | warning | 1 |
| Regulated | warning | error | warning | error | 1 |
| AllEnabled | warning | warning | warning | warning | 0 |

The verifier also checks CA1502 against actual Roslyn diagnostics (`Strict=warning`, `Regulated=error`) and confirms that a fixture-local `.editorconfig` override can suppress CA1822 while the generated Strict profile still reports CA2000 as an error. The temporary `.editorconfig` is removed after verification.
The six rule IDs are PoC examples, not production coverage. The generated globalconfig is passed to Roslyn through `EditorConfigFiles`.
