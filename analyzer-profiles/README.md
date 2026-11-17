# Analyzer profile PoC

Opt-in build: `dotnet build src/Clean.Architecture.Core/Clean.Architecture.Core.csproj -p:AnalyzerProfile=Strict`.
The profile is resolved from `matrix.json` into an intermediate `.globalconfig`.
Without `AnalyzerProfile`, existing builds are unchanged.

Precedence: `ruleOverrides` > `categoryOverrides` > selected profile category.
Supported severities: `none`, `silent`, `suggestion`, `warning`, `error`.
The nine rule IDs are reviewed proof-of-concept candidates, not a production coverage matrix.

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
The nine rule IDs are PoC examples, not production coverage. The generated globalconfig is passed to Roslyn through `EditorConfigFiles`.

## Incremental build integration

The MSBuild target uses matrix.json and the generator source/project as inputs and a profile-specific .globalconfig as output. Repeated builds with unchanged inputs skip the generator. Profile-specific output paths keep Strict and Regulated configurations independent. Changes to matrix or generator inputs invalidate the corresponding output. The generator itself also preserves unchanged file content.

## Slice B candidate fixture (isolated)

CandidateFixture tests CA1050 (type outside namespace), CA2208 (incorrect argument name), and CA1508 (redundant condition). Its local .editorconfig explicitly sets these diagnostics to warning. The three rules are now also included in the opt-in matrix; default builds remain unchanged.

After restoring the fixture, run dotnet run --project analyzer-profiles/DiagnosticVerifier/DiagnosticVerifier.csproj to check the actual Roslyn diagnostics. The verifier asserts only the three expected rules and the build exit code.

## Analyzer exception and suppression review (#254)

The profile matrix is opt-in. Its nine mapped IDs are PoC coverage, not a repository-wide policy. Do not enable a profile by default or alter required CI gates as part of exception handling.

1. Prefer fixing the diagnostic. Record the analyzer ID, affected project/file, selected profile, reason, and whether the finding is a false positive before proposing an exception.
2. Use the narrowest practical scope: a justified inline suppression or a file-scoped `.editorconfig` override. Avoid blanket `NoWarn`, category-wide overrides, or repository-wide disabling without separate architectural review.
3. In a PR, show the before/after diagnostic and document why a fix is inappropriate, the scope, owner, and when the exception should be revisited (for example, after an SDK or analyzer update).
4. Review changes to `matrix.json`, `.editorconfig`, or suppression attributes independently. The opt-in generator's `.globalconfig` does not automatically win over file-local `.editorconfig` settings.
5. Run the generator tests and actual diagnostic verifier on Windows and Ubuntu. Confirm the default build is unchanged; treat profile-specific warnings and errors as advisory until rollout is separately approved.

The functional test `RepositoryEditorConfigTests` detects direct per-rule severity collisions for the nine mapped IDs in the root, MinimalClean, and sample `.editorconfig` files. It intentionally does not claim to validate every Roslyn/MSBuild precedence case or analyzer availability.
