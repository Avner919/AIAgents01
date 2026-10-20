# Repository guidance
Repository: [Avner919/AIAgents01](https://github.com/Avner919/AIAgents01.git)

## Repository-specific development rule
- Applies only to `C:\Projects\GitHub\AIAgents01` and its subdirectories: before marking code changes complete, run `dotnet test AIPractice1.slnx` from the repository root and fix any test failures caused by the changes. If validation is blocked, state the blocker and do not claim the tests passed.

## Repository operation permissions
- Allow: run repository tests with `dotnet test`, run `dotnet format`, and create local commits with `git commit` without an additional permission question when those actions are part of the current task. Stage only intended changes; preserve unrelated user work.
- Ask: obtain user approval for each `git push` invocation. Do not publish through an alternate tool or wrapper to avoid that approval.
- Deny: do not rename files or directories, including moves that change their paths. This applies to shell commands, editor operations, patches, scripts, and filesystem APIs. Do not simulate a rename through copy-and-delete. If a requested implementation requires a rename, report the conflict rather than bypassing this rule.
- Run these operations from this repository's root. `.codex/config.toml` sets the approval mode; `.codex/rules/permissions.rules` defines command-prefix decisions. Prefix rules do not inspect arbitrary scripts or provide a filesystem-wide rename lock, and managed policies may impose additional restrictions. These permissions do not independently request a commit or push.

## Build and validation
For test changes, follow `.codex/rules/unit-tests.md` for test paths, conventions, isolation requirements, and validation commands.

```powershell
dotnet restore Exchange/Exchange.csproj
dotnet build Exchange/Exchange.csproj --no-restore
dotnet test Tests/Exchange.Tests.csproj
```

The project targets `net8.0`; use a compatible SDK. Solution-level commands require an SDK that supports `.slnx` (the installed .NET 10 SDK recognizes this solution). Project-level commands avoid that requirement.

The xUnit test launches the actual executable with invalid configuration in an isolated temporary directory and verifies its failure exit code and stderr diagnostics. It exits before external exchange-client process management. Run `dotnet test AIPractice1.slnx` to test the solution. There is currently no CI workflow. A successful startup test does not verify successful exchange with the external Windows client. For behavioral changes, add focused tests where practical and state which checks actually ran.

## Project skills
- Automatically use `.agents/skills/run-and-fix-tests/SKILL.md` after code changes or when repairing failing tests, before reporting the task complete. This is an agent procedure during an active task, not a background monitor. During a review-only task, report failures without making repairs.
- Use `.agents/skills/review-pull-request/SKILL.md` only when the user explicitly requests a pull-request review, for example `$review-pull-request` followed by a PR URL or number.

## Runtime and configuration
- Actual exchange requires Windows, the external `m_exchng.exe` client in `Exchange:AgentCatalog`, and configured input/output directories.
- Configuration loads in this order: `embeddedsettings.json`, `appsettings.json`, environment variables. Later values override earlier values. Environment variable keys use double underscores, for example `Exchange__Agent__Timeout`.
- `appsettings.Development.json` is not loaded by the current entry point. Do not assume setting a development environment activates it.
- Both loaded JSON files and `exchangeNlog.config` are copied to the build output. Run from that output directory because the entry point uses relative configuration paths.
- `Exchange:Agent:Timeout` is in milliseconds. Keep option names aligned with the JSON section and model properties.
- Treat credentials and tokens in settings as sensitive; do not repeat their values in documentation, logs, tests, or reports. Use environment overrides for local credentials.
- NLog is configured by `Exchange/exchangeNlog.config`, with a Windows log directory under ProgramData. Validate logging configuration when changing NLog or its package versions.
- Only run the utility in an explicitly authorized exchange environment: startup and cleanup call `StopElpClients()`, which kills all processes named `m_exchng`. Execution also initiates real exchange and can wait for the configured timeout.

## Editing conventions
- Match the surrounding C# style: four-space indentation, Allman braces, block-scoped namespaces, PascalCase types/methods/properties, and camelCase parameters/locals. Keep established Russian comments and user-facing log messages consistent.
- Follow the root `.editorconfig`: UTF-8, CRLF, system-first usings, and async method names ending in `Async`.
- Preserve external protocol identifiers (`CloseExchng`, `OneTimeExchng`, `ExchngEnd`, `LoadEnd`) and the client's argument order unless intentionally changing the integration contract.
- Keep changes focused. Preserve existing user edits and untracked IDE files. Do not commit generated `bin/`, `obj/`, logs, or local credentials.
