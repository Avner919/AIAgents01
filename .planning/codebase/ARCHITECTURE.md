---
last_mapped_commit: 6a4a0e6426a45c1bc286ebdcf3bb27e0193469a3
last_mapped_at: 2026-10-27
---
<!-- refreshed: 2026-10-27 -->

# Architecture

**Analysis Date:** 2026-10-27

## System Overview

```text
Console executable: Exchange/Program.cs
  ConfigurationBuilder -> Exchange.Options models -> NLog logger
                            |
                            v
Exchange coordinator: Exchange/ExchangeProcess.cs
  Register events -> stop clients -> launch client -> wait for LoadEnd
                            |
                            v
Windows integration boundary
  ExchangeClient.exe + named EventWaitHandle objects
  OneTimeExchng -> external file exchange -> ExchngEnd
                            |
                            v
Cleanup -> process exit code + stderr + NLog output
```

## Component Responsibilities

| Component | Responsibility | File |
|-----------|----------------|------|
| Program | Load configuration, bind options, construct coordinator, translate exceptions into exit codes | `Exchange/Program.cs` |
| ExchangeProcess | Own process launch/termination and named-event exchange lifecycle | `Exchange/ExchangeProcess.cs` |
| ExchangeOptions | Represent exchange section and input/output/client paths | `Exchange/Options/ExchangeOptions.cs` |
| AgentOptions | Represent client authentication fields and timeout in milliseconds | `Exchange/Options/AgentOptions.cs` |
| NLog configuration | Configure runtime logging independently of exchange orchestration | `Exchange/exchangeNlog.config` |
| Process test | Exercise invalid configuration through the real executable in an isolated directory | `Tests/ProgramEndToEndTests.cs` |
| Hook utilities | Guard direct protected-file operations and format edited C# files | `.codex/hooks/protect_files.py`, `.codex/hooks/format_after_edit.py` |

## Pattern Overview

**Overall:** Small synchronous console application with manual composition and one stateful integration coordinator (`Exchange/Program.cs`, `Exchange/ExchangeProcess.cs`).

**Key Characteristics:**
- Construct dependencies explicitly: `Program.Main` creates the logger, configuration, options, and `ExchangeProcess`; no dependency-injection container or generic host is present (`Exchange/Program.cs`).
- Delegate actual exchange to an external Windows executable; this repository coordinates processes and events rather than implementing the remote gateway protocol (`Exchange/ExchangeProcess.cs`).
- Use configuration models as the boundary between configuration providers and orchestration (`Exchange/Options/ExchangeOptions.cs`, `Exchange/Options/AgentOptions.cs`).
- Keep executable tests separate from runtime implementation and stage the built application without deployment settings (`Tests/Exchange.Tests.csproj`).

## Layers

**Composition and process boundary:**
- Purpose: Initialize configuration/logging and expose one OS process result.
- Location: `Exchange/Program.cs`.
- Contains: `Main`, top-level exception handler, `GetExitCode`, logging shutdown.
- Depends on: Microsoft configuration extensions, NLog, options classes, and `ExchangeProcess`.
- Used by: The executable host and the isolated process test in `Tests/ProgramEndToEndTests.cs`.

**Exchange orchestration:**
- Purpose: Sequence client startup, readiness, exchange request, completion, and cleanup.
- Location: `Exchange/ExchangeProcess.cs`.
- Contains: Process discovery/launch/kill, named-event registration/opening, blocking waits.
- Depends on: `System.Diagnostics.Process`, `System.Threading.EventWaitHandle`, NLog, bound options.
- Used by: `Program.Main` in `Exchange/Program.cs`.

**Configuration contracts:**
- Purpose: Define property names consumed by configuration binding and orchestration.
- Location: `Exchange/Options/ExchangeOptions.cs`, `Exchange/Options/AgentOptions.cs`.
- Contains: Mutable property models and the `Exchange` section-name constant.
- Depends on: No application service layer.
- Used by: `Exchange/Program.cs` for binding and `Exchange/ExchangeProcess.cs` for runtime parameters.

**Developer automation:**
- Purpose: Apply repository operation guidance and formatting during agent edits.
- Location: `.codex/config.toml`, `.codex/hooks/`, `.codex/rules/`, `.agents/skills/`.
- Contains: Python hook scripts, hook tests, command rules, and skill procedures.
- Depends on: Trusted Codex hook execution, Python, and the .NET formatter.
- Used by: Agent tooling; these files are outside the application execution path (`.codex/config.toml`).

## Data Flow

### Primary Request Path

1. Configure NLog before entering the main `try` block (`Exchange/Program.cs:14`).
2. Load `embeddedsettings.json`, then `appsettings.json`, then environment variables; bind the `Exchange` section (`Exchange/Program.cs:21`).
3. Construct `ExchangeProcess` with the logger and bound options, then call `StartExchangeProcess` (`Exchange/Program.cs:29`).
4. Register/open `CloseExchng`, `OneTimeExchng`, and `LoadEnd`; reserve an entry for `ExchngEnd` (`Exchange/ExchangeProcess.cs:69`).
5. Terminate all matching existing clients; resolve the configured client executable; launch it with login, password, input path, and output path in that order (`Exchange/ExchangeProcess.cs:85`).
6. Wait synchronously for `LoadEnd` and reset it; opening `ExchngEnd` must then succeed (`Exchange/ExchangeProcess.cs:130`).
7. Reset `ExchngEnd`, signal `OneTimeExchng`, wait for exchange completion, and reset the completion event (`Exchange/ExchangeProcess.cs:141`).
8. Close selected event handles and terminate matching clients in `finally`; return zero if no exception propagates (`Exchange/ExchangeProcess.cs:60`, `Exchange/Program.cs:33`).

### Failure and Exit-Code Flow

1. Exchange lifecycle errors are logged and wrapped in `InvalidOperationException`, retaining the original message and inner exception (`Exchange/ExchangeProcess.cs:52`).
2. The entry point logs and writes the exception to stderr (`Exchange/Program.cs:36`).
3. `GetExitCode` parses a leading numeric `N:` message prefix or defaults to `99`; documented lifecycle prefixes in source are `1` through `4` (`Exchange/Program.cs:49`, `Exchange/ExchangeProcess.cs`).
4. NLog shuts down in `finally` after the main guarded execution (`Exchange/Program.cs:43`).

### Isolated Startup-Test Flow

1. The test project copies executable assemblies and runtime metadata into its `ExchangeApp` output directory (`Tests/Exchange.Tests.csproj`).
2. The test copies those files to a unique temporary directory and supplies test-owned malformed JSON plus a null logging target (`Tests/ProgramEndToEndTests.cs`).
3. It launches `dotnet Exchange.dll`, captures output, enforces a 30-second limit, checks exit code `99` and configuration diagnostics, and deletes its directory (`Tests/ProgramEndToEndTests.cs`).
4. Failure happens before exchange process management; this test does not exercise real client startup or successful exchange (`Tests/ProgramEndToEndTests.cs`).

**State Management:**
- Configuration is bound once and supplied directly to one coordinator; no reload subscription is present (`Exchange/Program.cs`).
- The coordinator stores named handles in a mutable `_exchangeEventHandlers` dictionary; native event state is shared with the external client (`Exchange/ExchangeProcess.cs`).
- No database, cache, HTTP request state, message broker, or persistence layer is implemented in the application files (`Exchange/Program.cs`, `Exchange/ExchangeProcess.cs`).

## Key Abstractions

**ExchangeProcess:**
- Purpose: Represent a complete one-time exchange operation.
- Examples: `Exchange/ExchangeProcess.cs`, construction in `Exchange/Program.cs`.
- Pattern: Concrete coordinator with a primary constructor and private lifecycle helpers; there is no process/event adapter interface.

**ExchangeOptions / AgentOptions:**
- Purpose: Represent configuration structure without provider-specific logic.
- Examples: `Exchange/Options/ExchangeOptions.cs`, `Exchange/Options/AgentOptions.cs`.
- Pattern: Property-based configuration binding; preserve model/property alignment when adding settings.

**Named native events:**
- Purpose: Provide the client readiness/request/completion protocol.
- Examples: `Exchange/ExchangeProcess.cs` constants and `CreateEventByName`/`OpenEventByName`.
- Pattern: Open existing events where available, otherwise create manual-reset events for coordinator-owned names; completion requires an existing client-created event.

## Entry Points

**Application:**
- Location: `Exchange/Program.cs`, private static `Main`.
- Triggers: Running the compiled console application.
- Responsibilities: Configuration, logger setup, dependency composition, exit status, stderr diagnostics.

**Automated process check:**
- Location: `Tests/ProgramEndToEndTests.cs`.
- Triggers: xUnit execution through `Tests/Exchange.Tests.csproj` or `AIPractice1.slnx`.
- Responsibilities: Verify the malformed-configuration failure boundary safely.

**Agent hooks:**
- Location: `.codex/config.toml`, `.codex/hooks/protect_files.py`, `.codex/hooks/format_after_edit.py`.
- Triggers: Configured pre-tool and post-tool events, when trusted by the host.
- Responsibilities: Inspect direct tool inputs for protected paths and run whitespace formatting on edited C# files.

## Architectural Constraints

- **Threading:** Production execution blocks the calling thread with `WaitOne`; the configured timeout applies separately to readiness and exchange completion (`Exchange/ExchangeProcess.cs`).
- **Global state:** NLog configuration/shutdown is process-wide; OS event names and client process-name matching are shared beyond one coordinator instance (`Exchange/Program.cs`, `Exchange/ExchangeProcess.cs`).
- **Circular imports:** No circular project reference is detected: tests reference the application; the application does not reference tests (`Tests/Exchange.Tests.csproj`, `Exchange/Exchange.csproj`).
- **Platform:** `net8.0` is the target, but real exchange requires Windows named events and `ExchangeClient.exe`; the source suppresses CA1416 around event opening (`Exchange/Exchange.csproj`, `Exchange/ExchangeProcess.cs`, `AGENTS.md`).
- **Working directory:** Runtime configuration paths are relative; use the build output directory with copied configuration assets (`Exchange/Program.cs`, `Exchange/Exchange.csproj`, `AGENTS.md`).
- **Protocol:** Preserve `CloseExchng`, `OneTimeExchng`, `ExchngEnd`, `LoadEnd`, argument order, and millisecond timeout semantics (`Exchange/ExchangeProcess.cs`, `AGENTS.md`).
- **Process scope:** `StopClients` kills every process matching `ExchangeClient`, both before launch and during cleanup; concurrent real exchanges are not isolated (`Exchange/ExchangeProcess.cs:168`).
- **Repository operations:** Do not rename files/directories or modify protected environment files; obtain approval for each push (`AGENTS.md`).

## Anti-Patterns

### Cleanup Assumes Complete Initialization

**What happens:** `CloseEventHandlers` dereferences `_exchangeEventHandlers` and closes only two of the four protocol entries (`Exchange/ExchangeProcess.cs:186`).
**Why it's wrong:** Initialization failure can leave the dictionary unset, cleanup can mask the initial failure, and readiness/completion handles lack matching explicit disposal (`Exchange/ExchangeProcess.cs`).
**Do this instead:** When modifying this area, make cleanup tolerant of partial initialization and dispose every successfully acquired handle; preserve the lifecycle order in `StartExchangeProcess` and add isolated regression tests under `Tests/` (`Exchange/ExchangeProcess.cs`, `.codex/rules/unit-tests.md`).

### Raw String Construction of Client Arguments

**What happens:** `StartClient` interpolates all four arguments into one unquoted `Arguments` string (`Exchange/ExchangeProcess.cs:85`).
**Why it's wrong:** Whitespace or quoting in supplied values can change the external client's argument boundaries (`Exchange/ExchangeProcess.cs`).
**Do this instead:** When changing launch behavior, use a verified argument-boundary strategy while preserving login/password/input/output order; the process test demonstrates `ArgumentList` usage for its own dotnet launch (`Tests/ProgramEndToEndTests.cs`, `AGENTS.md`).

## Error Handling

**Strategy:** Exceptions propagate through the concrete coordinator into a process-level logging/exit-code boundary (`Exchange/ExchangeProcess.cs`, `Exchange/Program.cs`).

**Patterns:**
- Missing executable, startup timeout, missing completion event, and exchange timeout have numeric message prefixes (`Exchange/ExchangeProcess.cs`).
- Unknown configuration/runtime errors fall back to exit `99` (`Exchange/Program.cs`).
- Logger initialization is outside the guarded block; errors there do not use the main stderr/exit-code handler (`Exchange/Program.cs:16`).
- Cleanup has no independent exception guard and may replace an earlier error (`Exchange/ExchangeProcess.cs:58`).

## Cross-Cutting Concerns

**Logging:** NLog is injected as a concrete logger, with Russian messages at lifecycle transitions and errors; configuration is external XML (`Exchange/Program.cs`, `Exchange/ExchangeProcess.cs`, `Exchange/exchangeNlog.config`).
**Validation:** Binding is direct; explicit launch validation checks executable existence, but no dedicated options validator checks missing sections, paths, or timeout ranges (`Exchange/Program.cs`, `Exchange/ExchangeProcess.cs`, `Exchange/Options/AgentOptions.cs`).
**Authentication:** Client login/password are option properties forwarded to the external process; use environment overrides for local credentials and never reproduce settings values in docs or tests (`Exchange/Options/AgentOptions.cs`, `Exchange/ExchangeProcess.cs`, `AGENTS.md`).

---

*Architecture analysis: 2026-10-27*
