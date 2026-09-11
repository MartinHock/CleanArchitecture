# Renovate grouped-update POC

This branch is intentionally based on commit `fbdc0951879f5e8dca1bebc273d4b28cb2934469`, before the later Dependabot update wave in this fork.

The goal is to demonstrate a lower-noise dependency-maintenance strategy for the CleanArchitecture repository:

- NuGet is the only enabled package manager.
- Patch and minor NuGet updates are grouped into one pull request.
- Major NuGet updates stay separate and require manual approval through the Dependency Dashboard.
- Automerge is not enabled.

The configuration is in `renovate.json`.

## Why a dedicated branch?

Keeping the POC on its own historical branch makes it possible to compare Renovate's grouped result with the series of individual dependency PRs that were later merged into `main`, without changing the current production branch.

## Running the POC

Renovate normally reads repository configuration from the default branch. For a branch-only POC, run self-hosted Renovate with base-branch configuration enabled and target this branch explicitly. For example, provide a GitHub token through the environment and set:

```text
RENOVATE_PLATFORM=github
RENOVATE_REPOSITORIES=["MartinHock/CleanArchitecture"]
RENOVATE_BASE_BRANCH_PATTERNS=["poc/renovate-grouped-updates"]
RENOVATE_USE_BASE_BRANCH_CONFIG=merge
RENOVATE_ONBOARDING=false
RENOVATE_REQUIRE_CONFIG=optional
```

Then run Renovate. The expected result is one grouped non-major NuGet update PR, while major upgrades remain gated for manual approval.
