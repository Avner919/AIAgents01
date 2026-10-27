# AIAgents01 — Exchange Admin API

## What This Is

AIAgents01 contains a Windows console utility that coordinates file exchange through an external client executable. This milestone adds a separate ASP.NET Core application with an API-key-protected admin controller that launches `Exchange.exe` and immediately returns its process ID. The caller can supply a full external-client executable path or leave the field blank to use defaults retained in `ExchangeProcess`.

## Core Value

An authorized caller can start the exchange utility with an explicit client executable or clearly reported defaults and immediately receive the launched process ID.

## Requirements

### Validated

Existing capabilities inferred from checked source and `.planning/codebase/`; this does not establish successful operation against the real external client.

- ✓ Coordinate client startup and one-time exchange through Windows named events — existing `Exchange/ExchangeProcess.cs` implementation.
- ✓ Load JSON configuration followed by environment overrides — existing `Exchange/Program.cs` implementation.
- ✓ Provide process exit codes and error diagnostics for guarded startup/runtime failures — existing `Exchange/Program.cs` implementation and isolated malformed-configuration process test.
- ✓ Keep the client executable defaults in `ExchangeProcess` — existing class constants.

### Active

- [ ] Add a separate ASP.NET Core application containing an admin API controller.
- [ ] Require an API key for the admin launch endpoint.
- [ ] Allow the caller to provide an optional full path for the external client executable, corresponding to `ExchangeProcessFileName`.
- [ ] Launch `Exchange.exe` as a separate operating-system process and pass the selected client executable override to it.
- [ ] Return immediately after launch with the `Exchange.exe` process ID, without waiting for exchange completion.
- [ ] Use the existing client executable defaults when the override is omitted or blank.
- [ ] Inform the caller of all defaults applied when the executable field is blank, including the effective client executable selection.
- [ ] Retain default values in class `ExchangeProcess`; avoid separately maintained copies in the API.
- [ ] Add focused automated coverage for authentication, override/default selection, and immediate launch responses using isolated substitutes.

### Out of Scope

- Waiting for exchange completion in the launch response — the user selected an immediate process-ID response.
- Moving exchange orchestration into the API process — the user selected a separate ASP.NET Core application that launches `Exchange.exe`.
- Replacing API-key authentication with another mechanism — API key is the selected access method.
- Returning credentials or API keys as defaults — repository guidance prohibits exposing secrets; default reporting concerns non-secret behavior and executable selection.
- Unrelated fixes from `.planning/codebase/CONCERNS.md` — address only changes necessary for this milestone.

## Context

- Current application: `Exchange/Exchange.csproj`, targeting `net8.0`; entry point `Exchange/Program.cs`.
- The external client executable is distinct from the console utility `Exchange.exe` launched by the new API.
- `Exchange/ExchangeProcess.cs` currently defines the client process name and derived executable filename; the default client is resolved under the configured agent catalog.
- The coordinator uses the protocol identifiers `CloseExchng`, `OneTimeExchng`, `ExchngEnd`, and `LoadEnd`.
- Configuration precedence is `embeddedsettings.json`, `appsettings.json`, then environment variables. The entry point does not automatically load dotenv files or development JSON.
- Configuration and NLog paths are relative to the working directory; launching the console utility must preserve its expected configuration location.
- Existing application test: `Tests/ProgramEndToEndTests.cs`, an isolated malformed-configuration startup test that stops before external process management.
- The complete codebase map is in `.planning/codebase/`, committed as `40920ce`.
- Source-level risks include broad client-process termination, shared named events, cleanup assumptions, and raw client-argument construction. Relevant changes require focused regression checks.
- API route, request/response names, API-key header and configuration key, child launch transport, and detailed error responses remain implementation decisions to settle during requirements and phase planning.
- A returned process ID confirms that the utility was started; it does not establish that exchange succeeded.

## Constraints

- **Architecture:** Run the API in a separate ASP.NET Core app and launch `Exchange.exe` as a child process — explicit user decision.
- **Defaults:** Keep defaults in `ExchangeProcess` and expose effective non-secret defaults to the caller when the executable field is blank — explicit user decision.
- **Platform:** Actual exchange requires Windows and the external client's supported protocol.
- **Compatibility:** Preserve existing default invocation, client argument order, named-event identifiers, and millisecond timeout semantics unless separately agreed.
- **Security:** Require an API key; never hardcode, log, return, or commit API keys or client credentials. Secret files are maintained manually by the user.
- **Repository operations:** Do not modify protected environment files or rename/move files and directories. Each Git push requires user approval. See `AGENTS.md`.
- **Validation:** Run `dotnet test AIPractice1.slnx` before completing code changes, following `.agents/skills/run-and-fix-tests/SKILL.md` and `.codex/rules/unit-tests.md`.
- **Execution:** Do not exercise real exchange without an explicitly authorized environment; startup and cleanup can terminate existing client processes. Use isolated substitutes for automated launch tests.

## Key Decisions

| Decision | Rationale | Outcome |
|----------|-----------|---------|
| Separate ASP.NET Core admin API | User confirmed a separate web application launching the console utility | — Pending implementation |
| Accept a full external-client executable path | User needs to select the client executable per launch | — Pending implementation |
| Blank or omitted path uses class-owned defaults | Preserve existing behavior and keep defaults in `ExchangeProcess` | — Pending implementation |
| Report applied defaults to the caller | Caller must know what will be used when leaving the field blank | — Pending implementation |
| Return immediately with the utility process ID | User selected launch acknowledgement rather than waiting for completion | — Pending implementation |
| Protect the endpoint with an API key | User selected API-key authentication | — Pending implementation |

## Evolution

This document evolves at phase transitions and milestone boundaries.

After each phase transition:
1. Move invalidated requirements to Out of Scope with a reason.
2. Move verified requirements to Validated with a phase reference.
3. Add newly discovered requirements to Active.
4. Record decisions and their observed outcomes.
5. Update What This Is when behavior changes.

After each milestone:
1. Review all sections against the delivered system.
2. Confirm the Core Value still drives prioritization.
3. Revisit Out of Scope decisions and their reasons.
4. Update Context with current implementation and verified behavior.

---
*Last updated: 2026-10-27 after project initialization and user scope confirmation*
