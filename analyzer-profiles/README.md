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
