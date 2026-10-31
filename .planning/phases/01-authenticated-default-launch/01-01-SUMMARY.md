---
phase: 01-authenticated-default-launch
plan: "01"
subsystem: api
tags: [aspnetcore, api-key, process, configuration, xunit]
requires: []
provides:
  - Authenticated HTTP-to-process default launch with immediate PID
  - Shared class-owned default resolution and pinned child environment
  - Isolated harmless-child tracer and operator instructions
affects: [01-02, 01-03, explicit-client-selection]
tech-stack:
  added: []
  patterns: [pure-configuration-resolution, child-environment-snapshot, owned-process-fixtures]
key-files:
  created:
    - Exchange.Api/Exchange.Api.csproj
    - Exchange/Configuration/ExchangeConfigurationLoader.cs
    - Tests/Fixtures/HarmlessUtility/Program.cs
    - docs/exchange-api.md
  modified:
    - Exchange/Program.cs
    - Exchange/ExchangeProcess.cs
    - Tests/Exchange.Tests.csproj
requirements-completed: [API-01, AUTH-01, AUTH-02, EXEC-01, EXEC-03, EXEC-04, DEF-01, DEF-02, ERR-01]
coverage:
  - id: default-launch-tracer
    description: Real authenticated HTTP request launches a harmless owned child and returns its PID before release.
    requirement: EXEC-04
    verification:
      - kind: integration
        ref: Tests/ExchangeApiTests.cs#DefaultLaunch_ReturnsOwnedProcessIdBeforeReleaseAsync
        status: pass
    human_judgment: false
  - id: ordered-client-arguments
    description: Four pure cases preserve argument order and boundaries, including spaces.
    verification:
      - kind: unit
        ref: Tests/ExchangeClientStartInfoTests.cs#BuildClientStartInfo_PreservesOrderedArguments
        status: pass
    human_judgment: false
  - id: blank-exchange-directories
    description: User amendment D-14 preserves blank directories through shared resolution, child environment and arguments.
    verification:
      - kind: unit
        ref: Tests/ExchangeConfigurationTests.cs#BlankExchangeDirectories_RemainEmptyInChildAndArguments
        status: pass
    human_judgment: false
completed: 2026-10-31
status: complete
---

# Phase 01 Plan 01: Authenticated Default Launch

The separate API launches only its configured utility and returns a PID with six safe effective settings, using shared defaults and an immutable per-launch environment.

## Accomplishments

- Added `POST /admin/exchanges`, `X-Api-Key` authentication, fail-closed startup validation, sanitized errors and an explicit response allowlist.
- Retained executable defaults in `ExchangeProcess`; console and API share loading/resolution. Four ordered `ArgumentList` entries preserve client arguments containing spaces.
- Added the harmless executable, real loopback HTTP host and owned-child readiness/release fixtures. Production launching was exercised without real exchange or installed-client cleanup.
- Added operator deployment instructions and a blank-key example. The user-requested generated key is in ignored local API settings; no key value was printed or committed.

## Task Commits

1. RED boundary tests: `2f13289` — four expected missing-method assertion failures.
2. Task 1 production tracer: `6621f7d` — API, shared resolution, process launcher and isolated tests.
3. User execution amendment D-14: `6a31ddf` — permit blank configured input/output directories and three regression cases.
4. Task 2 instructions/local configuration protection: `3a42875`.

## Verification

- Final `dotnet test AIPractice1.slnx --logger "console;verbosity=normal"`: 13 passed, 0 failed, 0 skipped, nonzero discovery; reported test time 9.1391 seconds. Run outside the sandbox after the sandbox denied access to user NuGet configuration.
- Earlier first tracer run: 10 passed. Four original RED cases subsequently pass using direct calls rather than reflection.
- Whitespace formatter applied to changed C# files. `git -c core.whitespace=cr-at-eol diff --check` passes; the flag respects the repository's required CRLF endings.
- API started outside the sandbox at `http://127.0.0.1:5080`. A GET-only request received HTTP 405, proving reachability without invoking the POST launch action. No real exchange was started.
- Existing NLog deprecation warnings remain; no logging package migration was performed.

## Decisions and Deviations

- The executor was stopped at the user's explicit request after a prolonged handoff stall. Reconciliation found its committed RED tests but no implementation/SUMMARY. Implementation continued inline from that work.
- The user explicitly requested leaving actual input/output settings blank. D-14 records that amendment: empty/null/whitespace directory settings become empty strings, not invented absolute defaults, and remain separate empty client arguments. Existing utility settings files were not changed.
- The first API start succeeded inside the sandbox but logged Windows DataProtection/EventLog access errors. The verified owned API process was stopped and restarted outside the sandbox; normal startup then succeeded.
- The first tracer's documented larger file-count exception was retained. No new package dependencies, renames or protected environment-file edits were introduced.

## Remaining Phase Work

This closes Plan 01-01 only. Plans 01-02 and 01-03 still provide adversarial authentication/startup/error matrices, full blank-request/snapshot mutation coverage, overlap admission and publish-output verification. Plan-level requirement labels do not mark the whole phase verified. Phase code review/security review and independent goal verification remain pending.

## Self-Check: PASSED

The first two created key files exist, task commits are reachable on the current branch, required scoped tests pass and no actual exchange was run. Local key configuration is ignored. The API is intentionally left running for the user's startup request.
