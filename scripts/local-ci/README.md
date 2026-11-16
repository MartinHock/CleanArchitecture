# Optional local pre-push CI (proof of concept)

This tool is **disabled by default**. It is not a replacement for required GitHub-hosted CI. It does not post commit statuses, alter branch protection, or run integration/functional tests.

From the repository root, enable it **only in the current checkout**:

```powershell
git config --local core.hooksPath scripts/git-hooks
```

A normal `git push` then invokes `scripts/git-hooks/pre-push`, which runs `dotnet restore`, `dotnet build Clean.Architecture.slnx -c Release --no-restore`, and the .NET 11 unit tests through xUnit v3 / Microsoft Testing Platform. A failed command exits nonzero and blocks the push. PowerShell 7 (`pwsh`) or Windows PowerShell (`powershell.exe`) and the repository's .NET SDK are required.

To test without pushing anything, create a disposable local bare remote and use `git push --dry-run`. For the failure path, set `LOCAL_CI_FORCE_FAIL=1` in the environment before the dry-run. **Never run the forced-failure check against a real remote unless you intend to abort the push.** Unset the variable afterward. The hook can also be bypassed with `git push --no-verify`; it is a convenience, not a security boundary.

To disable:

```powershell
git config --local --unset core.hooksPath
```

Check any existing custom `core.hooksPath` before enabling: this setting replaces that hook directory for the current checkout. Do not configure it globally. On a Git worktree with a shared Git config, check the effective config scope before enabling. No hook is installed or enabled merely by checking out this repository.

Initial isolated Windows PoC (2026-11-16): clean restore/build/unit tests passed (12/12, ~66 seconds cold and ~17 seconds warm); forced failure blocked a dry-run push, successful checks allowed a dry-run push. These measurements are environment-specific, not CI performance guarantees.
