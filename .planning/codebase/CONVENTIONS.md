---
last_mapped_commit: 6a4a0e6426a45c1bc286ebdcf3bb27e0193469a3
last_mapped_at: 2026-10-27
---
# Coding Conventions

**Analysis Date:** 2026-10-27

## Naming Patterns

**Files:**
- Match C# file names to their main type: `Exchange/ExchangeProcess.cs`, `Exchange/Options/AgentOptions.cs`.
- Name test files `<Subject>Tests.cs`; process startup tests live in `Tests/ProgramEndToEndTests.cs`.
- Name Python hooks with snake_case and their tests `test_<hook>.py`: `.codex/hooks/format_after_edit.py`, `.codex/hooks/tests/test_format_after_edit.py`.

**Functions:**
- Use PascalCase C# methods, as in `StartExchangeProcess` in `Exchange/ExchangeProcess.cs`.
- End async methods with `Async`; `.editorconfig` assigns a warning to violations. Example: `InvalidConfiguration_ReportsErrorAndReturnsFailureAsync` in `Tests/ProgramEndToEndTests.cs`.
- Use snake_case Python functions, as in `edited_files` in `.codex/hooks/format_after_edit.py`.

**Variables:**
- Use camelCase C# locals and parameters: `exchangeOptions`, `processPath`, `testDirectory` in `Exchange/ExchangeProcess.cs` and `Tests/ProgramEndToEndTests.cs`.
- Use PascalCase constants such as `ExchangeProcessName` in `Exchange/ExchangeProcess.cs`.
- Private readonly fields have an underscore/camelCase naming suggestion in `.editorconfig`; the mutable `_exchangeEventHandlers` field in `Exchange/ExchangeProcess.cs` also uses that form.

**Types:**
- Use PascalCase types and properties: `ExchangeOptions`, `AgentOptions`, `AgentCatalog`, `Timeout` in `Exchange/Options/`.
- Prefix interfaces with `I` according to `.editorconfig`; no application interface is present in the inspected C# files.

## Code Style

**Formatting:**
- Follow four-space indentation, Allman braces, UTF-8, and CRLF for C# in `.editorconfig` and `AGENTS.md`.
- Preserve block-scoped namespaces, as used in `Exchange/Program.cs` and `Tests/ProgramEndToEndTests.cs`.
- `.editorconfig` prefers `var`, predefined type keywords, explicit accessibility, and usings outside namespaces. It does not require a final newline.
- `.codex/hooks/format_after_edit.py` invokes `dotnet format whitespace AIPractice1.slnx --no-restore --include <edited paths> --verbosity minimal` for successful C# file edits. It excludes generated directories and uses a 120-second deadline.
- Hook activation depends on configured and trusted lifecycle hooks; the existence of `.codex/hooks/format_after_edit.py` does not establish that it is active (`AGENTS.md`).

**Linting:**
- Use .NET analyzer settings in `.editorconfig`; no separate ESLint, Prettier, or Python lint configuration is detected in the inspected project files.
- `.editorconfig` sets Style diagnostics to warning and async naming to warning, with several rule-specific overrides and a general analyzer severity of none. Do not assume all analyzer categories are enforced.
- `Exchange/ExchangeProcess.cs` locally suppresses CA1416 around `EventWaitHandle.TryOpenExisting`; preserve the Windows platform requirement when modifying this area.
- `Exchange/Exchange.csproj` does not enable nullable reference types; `Tests/Exchange.Tests.csproj` enables nullable and implicit usings. Match the project containing the change.

## Import Organization

**Order:**
1. Put `System` namespaces first, using `.editorconfig` as the authority.
2. Group external namespaces such as `Xunit` and `NLog` separately.
3. Put project namespaces such as `Exchange.Options` with application imports.

- `Tests/ProgramEndToEndTests.cs` demonstrates a blank line between system and external imports. Existing application import order in `Exchange/Program.cs` and `Exchange/ExchangeProcess.cs` is not fully normalized; use the configured order for edits.

**Path Aliases:**
- Not applicable to the current C# namespaces. Use `Exchange`, `Exchange.Options`, and `Exchange.Tests` as shown in `Exchange/Program.cs`, `Exchange/Options/ExchangeOptions.cs`, and `Tests/ProgramEndToEndTests.cs`.

## Error Handling

**Patterns:**
- `Exchange/Program.cs` catches startup/exchange exceptions, writes them to NLog and stderr, derives a numeric exit code from a leading `<number>:` message, and uses `99` when no prefix matches.
- `Exchange/ExchangeProcess.cs` encodes distinct failure conditions with numbered exception messages, logs the exception, and wraps it in `InvalidOperationException` with the original exception as inner exception.
- Cleanup is placed in `finally` in `Exchange/Program.cs`, `Exchange/ExchangeProcess.cs`, and `Tests/ProgramEndToEndTests.cs`. Preserve cleanup when adding failure paths.
- Python formatter failures return blocking status `2` with stderr diagnostics in `.codex/hooks/format_after_edit.py`; test those outcomes with isolated mocks in `.codex/hooks/tests/test_format_after_edit.py`.

## Logging

**Framework:** NLog, configured by relative file name in `Exchange/Program.cs`; package references are in `Exchange/Exchange.csproj`.

**Patterns:**
- Use `logger.Info` for lifecycle progress and `logger.Error(exception, message)` for failures, following `Exchange/ExchangeProcess.cs`.
- Preserve Russian comments and user-facing messages (`AGENTS.md`, `Exchange/Program.cs`, `Exchange/ExchangeProcess.cs`).
- Always call `LogManager.Shutdown()` during program cleanup as in `Exchange/Program.cs`.
- Keep credentials and deployment settings out of new logs and assertion diagnostics (`AGENTS.md`, `.codex/rules/unit-tests.md`).

## Comments

**When to Comment:**
- Explain protocol events, lifecycle ordering, and non-obvious event resets, following `Exchange/ExchangeProcess.cs`.
- Explain test isolation where process side effects matter: `Tests/ProgramEndToEndTests.cs` documents why malformed configuration must fail before external-client management.
- Preserve existing Russian documentation and comments when editing C# (`AGENTS.md`).

**JSDoc/TSDoc:**
- Not applicable. C# XML documentation uses `<summary>`, `<param>`, `<returns>`, and `<remarks>` in `Exchange/Options/ExchangeOptions.cs` and `Exchange/ExchangeProcess.cs`.
- Missing public XML documentation is not enforced: `.editorconfig` disables CS1591.
- Python module docstrings describe hook intent in `.codex/hooks/format_after_edit.py`.

## Function Design

**Size:**
- No formal function-length limit is configured in `.editorconfig`. Keep lifecycle phases in named private helpers as in `Exchange/ExchangeProcess.cs`.

**Parameters:**
- `Exchange/ExchangeProcess.cs` uses a C# primary constructor to receive `Logger` and `ExchangeOptions`; reuse this explicit dependency shape when extending the current class.
- Configuration is represented by mutable property classes in `Exchange/Options/AgentOptions.cs` and `Exchange/Options/ExchangeOptions.cs`.
- Hook helpers accept event dictionaries and return selected paths or decisions, demonstrated by `.codex/hooks/format_after_edit.py` and `.codex/hooks/tests/test_protect_files.py`.

**Return Values:**
- Lifecycle methods use `void`, failures use exceptions, and `Main` returns an integer status in `Exchange/ExchangeProcess.cs` and `Exchange/Program.cs`.
- Async tests return `Task`, use bounded waits, and await stream reads in `Tests/ProgramEndToEndTests.cs`.
- Existing event-opening helpers can return null in `Exchange/ExchangeProcess.cs`; account for this when using their results.

## Module Design

**Exports:**
- Use explicit public classes in application namespaces and keep orchestration helpers private (`Exchange/Program.cs`, `Exchange/ExchangeProcess.cs`).
- Put new application C# tests in `Tests/` under `Exchange.Tests`, following `.codex/rules/unit-tests.md`.
- Run the test skill after code changes, following `.agents/skills/run-and-fix-tests/SKILL.md`; use the PR review skill only on explicit request, following `.agents/skills/review-pull-request/SKILL.md`.
- Preserve the fixed external protocol names and client argument order in `Exchange/ExchangeProcess.cs`; these are integration contracts (`AGENTS.md`).

**Barrel Files:**
- Not applicable. `Exchange/Exchange.csproj` and `Tests/Exchange.Tests.csproj` define C# project boundaries; there are no barrel exports.

---

*Convention analysis: 2026-10-27*
