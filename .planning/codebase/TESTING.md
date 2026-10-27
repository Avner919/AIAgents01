---
last_mapped_commit: 6a4a0e6426a45c1bc286ebdcf3bb27e0193469a3
last_mapped_at: 2026-10-27
---
# Testing Patterns

**Analysis Date:** 2026-10-27

## Test Framework

**Runner:**
- xUnit `2.9.3`, Visual Studio adapter `3.1.5`, and Microsoft.NET.Test.Sdk `17.14.1` in `Tests/Exchange.Tests.csproj`.
- Target framework: `net8.0`; configuration is in `Tests/Exchange.Tests.csproj`. No separate xUnit runner configuration is detected.
- Python standard-library `unittest` runs `.codex/hooks/tests/test_format_after_edit.py` and `.codex/hooks/tests/test_protect_files.py` independently of the .NET solution.

**Assertion Library:**
- xUnit `Assert.True` and `Assert.Contains` in `Tests/ProgramEndToEndTests.cs`.
- `unittest.TestCase` assertions and `unittest.mock` in `.codex/hooks/tests/test_format_after_edit.py` and `.codex/hooks/tests/test_protect_files.py`.

**Run Commands:**

```powershell
dotnet test AIPractice1.slnx
dotnet test Tests/Exchange.Tests.csproj
dotnet test Tests/Exchange.Tests.csproj --filter "FullyQualifiedName~Exchange.Tests.ProgramEndToEndTests"
python -m unittest discover -s .codex/hooks/tests -p "test_*.py" -v
```

- Run from the repository root, as required by `AGENTS.md` and `.codex/rules/unit-tests.md`.
- Use a .NET SDK supporting `.slnx` for `AIPractice1.slnx`; project-level invocation avoids that solution-format requirement (`AGENTS.md`).
- Watch mode and coverage commands are not documented/configured in `Tests/README.md` or `Tests/Exchange.Tests.csproj`.
- This mapping inspects source and configuration only. It does not run tests and makes no claim about current test pass counts.

## Test File Organization

**Location:**
- Application tests are separate from production code in `Tests/`; add new classes there under namespace `Exchange.Tests` (`.codex/rules/unit-tests.md`).
- Hook tests are co-located under `.codex/hooks/tests/` relative to their Python hook modules.

**Naming:**
- C# classes/files use `<Subject>Tests`; methods follow `<MethodOrScenario>_<Condition>_<ExpectedResult>` and async methods end in `Async` (`.codex/rules/unit-tests.md`).
- Python files use `test_*.py`, classes derive from `unittest.TestCase`, and methods begin with `test_` (`.codex/hooks/tests/test_format_after_edit.py`).

**Structure:**

```text
Tests/
  Exchange.Tests.csproj
  ProgramEndToEndTests.cs
  README.md
.codex/hooks/tests/
  test_format_after_edit.py
  test_protect_files.py
```

## Test Structure

**Suite Organization:**

```csharp
// Tests/ProgramEndToEndTests.cs
namespace Exchange.Tests
{
    public class ProgramEndToEndTests
    {
        [Fact]
        public async Task InvalidConfiguration_ReportsErrorAndReturnsFailureAsync()
        {
            // Create isolated files; start the real executable; inspect exit/stderr.
        }
    }
}
```

**Patterns:**
- Use Arrange / Act / Assert and observable behavior, including meaningful boundary/error cases (`.codex/rules/unit-tests.md`).
- The single C# fact stages build outputs in a unique temporary directory, writes synthetic configuration, launches `Exchange.dll`, and asserts exit code `99` and configuration diagnostics (`Tests/ProgramEndToEndTests.cs`).
- Read stdout and stderr concurrently before awaiting child exit to avoid pipe blocking (`Tests/ProgramEndToEndTests.cs`).
- Always bound waits and clean resources: the C# test uses a 30-second cancellation deadline, kills the child process tree on timeout, awaits exit, and deletes its temporary directory in `finally` (`Tests/ProgramEndToEndTests.cs`).
- Python setup uses `TemporaryDirectory`, `addCleanup`, and patched module roots so tests do not touch repository files (`.codex/hooks/tests/test_format_after_edit.py`, `.codex/hooks/tests/test_protect_files.py`).

## Mocking

**Framework:** Python `unittest.mock`; no C# mocking package is referenced by `Tests/Exchange.Tests.csproj`.

**Patterns:**

```python

# .codex/hooks/tests/test_format_after_edit.py

with patch.object(hook.subprocess, "run", return_value=subprocess.CompletedProcess([], 0)) as run:
    self.assertEqual(hook.main(), 0)
```

- Formatter tests patch stdin/stdout/stderr and `subprocess.run`, then inspect exact formatter arguments and working directory (`.codex/hooks/tests/test_format_after_edit.py`).
- Hook tests dynamically import the target module with `importlib.util.spec_from_file_location` and patch its `ROOT` to an owned temporary path (`.codex/hooks/tests/test_protect_files.py`).

**What to Mock:**
- Replace external services, installed client programs, and shared user settings with test implementations (`.codex/rules/unit-tests.md`).
- Mock formatter subprocess invocation for argument/failure/timeout tests (`.codex/hooks/tests/test_format_after_edit.py`).

**What NOT to Mock:**
- Keep the executable real in startup process tests; use test-owned malformed configuration to stop before external client management (`Tests/ProgramEndToEndTests.cs`).
- Keep actual protected-file hook subprocess protocol tests for valid JSON deny output and malformed input (`.codex/hooks/tests/test_protect_files.py`).

## Fixtures and Factories

**Test Data:**

```csharp
// Tests/ProgramEndToEndTests.cs
var testDirectory = Path.Combine(Path.GetTempPath(), "Exchange.Tests", Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(testDirectory);
await File.WriteAllTextAsync(Path.Combine(testDirectory, "embeddedsettings.json"), "{}");
await File.WriteAllTextAsync(Path.Combine(testDirectory, "appsettings.json"), "{ invalid json }");
```

**Location:**
- Synthetic settings and a null-target NLog configuration are created inline in `Tests/ProgramEndToEndTests.cs`; no checked-in deployment fixture is used.
- `CopyExchangeApplication` in `Tests/Exchange.Tests.csproj` stages DLLs plus dependency/runtime manifests under the test output's `ExchangeApp/` directory, without deployment settings.
- Python `event` helpers construct hook payload dictionaries in `.codex/hooks/tests/test_format_after_edit.py` and `.codex/hooks/tests/test_protect_files.py`.
- Do not add real credentials, working deployment settings, or user logs to fixtures or diagnostic messages (`.codex/rules/unit-tests.md`).

## Coverage

**Requirements:**
- No numeric coverage threshold or coverage collector is configured in `Tests/Exchange.Tests.csproj`.
- `AGENTS.md` and `.agents/skills/run-and-fix-tests/SKILL.md` require the full solution test after code changes, expected test discovery, and repair of failures caused by changes.
- Tests are not run for this documentation-only map; previous session pass reports do not establish current results.

**View Coverage:**
- Not configured; no repository coverage report command is present in `Tests/README.md` or `Tests/Exchange.Tests.csproj`.

## Test Types

**Unit Tests:**
- Python hook tests exercise path filtering, formatter arguments, failure feedback, timeout handling, protected-file decisions, payload validation, and output protocol (`.codex/hooks/tests/test_format_after_edit.py`, `.codex/hooks/tests/test_protect_files.py`).
- No isolated application C# unit tests are detected beyond the process test in `Tests/ProgramEndToEndTests.cs`.

**Integration Tests:**
- `Tests/ProgramEndToEndTests.cs` integrates the real built executable, configuration loader, logging startup, stderr, and exit status.
- `.codex/hooks/tests/test_protect_files.py` also launches the actual hook script through `sys.executable` with JSON stdin and a 10-second subprocess timeout.

**E2E Tests:**
- `Tests/ProgramEndToEndTests.cs` is a process-level startup failure test. It does not verify successful exchange or Windows named-event coordination with the external client (`Tests/README.md`).
- Actual exchange tests require a separately authorized Windows environment; application startup/cleanup kills all processes named `ExchangeClient` (`AGENTS.md`, `Exchange/ExchangeProcess.cs`). Test-rule and skill prose also contains the older `m_exchng.exe` label; use checked source for the current executable name.
- No CI workflow is present according to `AGENTS.md`; running the .NET command does not include the separate Python unittest suite.

## Common Patterns

**Async Testing:**

```csharp
// Tests/ProgramEndToEndTests.cs
var standardOutput = process.StandardOutput.ReadToEndAsync();
var standardError = process.StandardError.ReadToEndAsync();
using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
await process.WaitForExitAsync(timeout.Token);
```

- Use child-process configuration rather than global environment changes, unique temporary files, and bounded synchronization; avoid arbitrary sleeps (`.codex/rules/unit-tests.md`).

**Error Testing:**

```csharp
// Tests/ProgramEndToEndTests.cs
Assert.Contains("appsettings.json", error);
Assert.Contains("InvalidDataException", error);
```

- The same C# test checks exit code with `Assert.True` and diagnostic context. Python tests use `subTest` for path/operation variants and mock `TimeoutExpired` to verify formatter failure behavior (`.codex/hooks/tests/test_protect_files.py`, `.codex/hooks/tests/test_format_after_edit.py`).

---

*Testing analysis: 2026-10-27*
