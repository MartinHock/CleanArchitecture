# Analyzer profile architecture proposal (#254)

Status: design only; no production rollout authorized.

## Existing architecture

The opt-in MSBuild property AnalyzerProfile selects Default, Strict, Regulated or AllEnabled. With no property, the existing build is unchanged. matrix.json maps analyzer IDs to categories, defines category severities by profile, and permits category and rule overrides. Resolution order: rule > category > profile. The C# generator produces profile-specific intermediate .globalconfig files supplied via EditorConfigFiles. File-local .editorconfig precedence is covered by the Roslyn fixture.

The six current rules are proof-of-concept examples, not production coverage. Existing editorconfigs, warning policies, project-specific exceptions, xUnit v3 and Microsoft Testing Platform remain unchanged.

## Current profile semantics

| Profile | Design | Performance | Security | Reliability | Maintainability | Style |
| --- | --- | --- | --- | --- | --- | --- |
| Default | suggestion | suggestion | warning | warning | suggestion | silent |
| Strict | warning | warning | error | error | warning | suggestion |
| Regulated | warning | warning | error | error | error | warning |
| AllEnabled | warning | warning | warning | warning | warning | warning |

AllEnabled configures every *listed* rule as warning; it does not enable every SDK analyzer. Severity values: none, silent, suggestion, warning, error. TreatWarningsAsErrors is separate. Suggestions may not appear in build output.

Current examples: CA1062 Design, CA1822 Performance, CA2100 Security, CA2000 Reliability, CA1502 Maintainability, IDE0005 Style. Actual Roslyn fixture coverage exists for CA1062, CA1822, CA2000 and CA1502.

## Proposed expansion

1. Inventory root, MinimalClean and sample .editorconfig files, SDK analyzer availability and project warning exceptions.
2. Select a small reviewed batch of additional IDs per category, documenting rationale, intended profile severity, false-positive risk and verification.
3. Add focused tests and matrix entries in separate implementation PRs after approval; check Windows/Ubuntu behavior and build performance.
4. Start with opt-in non-blocking CI checks; preserve default builds and required CI gates until separately approved.

## Developer and CI selection

CLI: dotnet build src/Clean.Architecture.Core/Clean.Architecture.Core.csproj -p:AnalyzerProfile=Strict.

Visual Studio: pass AnalyzerProfile as an MSBuild property for the relevant build; exact IDE configuration and design-time parity still require validation. CI should explicitly pass the same property in an advisory job. Profile switching Strict -> Regulated -> Strict must use separate outputs; matrix/generator changes must invalidate them.

## Proposed exception governance

Prefer code fixes. Repository-wide rule/category overrides require rationale, impact assessment and independent review. Scope local .editorconfig exceptions narrowly and document reasons. Inline suppressions need justification and review. Avoid blanket NoWarn changes. Revisit exceptions after SDK/analyzer upgrades.

## Open decisions

- Which new IDs and analyzer packages are in scope?
- Should Visual Studio have a default profile, and how should it persist without changing CI?
- What advisory CI failure policy and performance budget are acceptable?
- Who approves exceptions and when are they revisited?

## Existing validation

Generator regression tests use xUnit v3, MTP and Shouldly. The diagnostic fixture checks four profiles and local .editorconfig precedence. Previous PRs had green hosted Windows, Ubuntu and macOS checks. Expansion requires per-rule verification.