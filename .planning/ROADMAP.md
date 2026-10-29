# Roadmap: AIAgents01

## Overview

The Exchange Admin API milestone delivers two usable vertical slices: an API-key-protected default launch with an immediate utility PID, effective defaults, launch errors, and overlapping request admission, followed by explicit external-client selection and lifecycle compatibility. Each slice includes console integration, HTTP behavior, isolated verification, and usable configuration examples. The API launches the configured `Exchange.exe`; the caller-selected executable is the external client used by that utility.

## Phases

- [ ] **Phase 1: Authenticated Default Launch** - Authorized operators launch with class-owned defaults, receive an immediate PID or launch error, and can submit overlapping requests.
- [ ] **Phase 2: Explicit Client Selection and Compatibility** - Operators select a full client path with consistent cleanup, preserved console behavior, and complete operator documentation.

## Phase Details

### Phase 1: Authenticated Default Launch
**Goal:** An authorized operator can start the separate exchange utility using existing defaults, receive its PID and non-secret effective settings or a useful launch error, and submit another request while the first utility is running.
**Mode:** mvp
**Depends on:** Nothing (first phase)
**Requirements:** API-01, AUTH-01, AUTH-02, EXEC-01, EXEC-03, EXEC-04, EXEC-05, DEF-01, DEF-02, ERR-01
**Success Criteria** (what must be TRUE):
  1. An operator with a valid API key can call an admin POST endpoint in a separate ASP.NET Core application and start the operator-configured `Exchange.exe` as a separate process; usable configuration instructions and a default-launch example accompany this slice.
  2. Missing or invalid API keys cause zero launches; missing server API-key configuration never permits anonymous launch.
  3. Omitted, null, empty, and whitespace-only executable input all use the same defaults retained in `ExchangeProcess`, with API resolution and console execution agreeing on the effective values.
  4. Successful default-launch responses contain the utility PID and effective client filename, full executable path, client working directory, input/output directories, and timeout in milliseconds, without credentials or API keys. The response can complete while the utility is still running; the PID acknowledges process creation only.
  5. Two valid overlapping default-launch requests can each receive an immediate utility PID without rejection, queueing, or serialization solely because another exchange is running. Invalid executable paths applicable to this slice and immediate process-start failures return useful errors without secrets or successful PID responses. Admission does not promise independent concurrent external exchanges.
**Plans:** TBD

**Verification:** Use isolated HTTP tests with substitute launchers to prove authentication precedes side effects, all blank variants resolve consistently, safe response fields match effective configuration, and response completion does not depend on child completion. Keep the first test launch active while submitting a second and verify both return PIDs. Cover invalid applicable paths and immediate start failures, with no success PID or secrets in errors. Verify the console/default resolution contract without invoking real exchange or installed-client cleanup. Check configuration and default-launch examples against the implemented contract. Run `dotnet test AIPractice1.slnx` after code changes, following the repository test skill.

**Discussion before planning:** Settle endpoint/header/configuration names, response and error shapes, utility deployment/configuration working directory, and shared resolution/configuration boundaries so reported defaults match execution. Review broad cleanup and fixed named events before wiring launches; record concurrency limits with this slice. Overlapping admission must be verified in Phase 1. Do not introduce single-flight rejection or queueing or claim concurrent exchange isolation.

### Phase 2: Explicit Client Selection and Compatibility
**Goal:** An authorized operator can select a full external-client executable path with consistent startup and cleanup, while existing console invocation and the Phase 1 launch contract remain supported and fully documented.
**Mode:** mvp
**Depends on:** Phase 1
**Requirements:** EXEC-02, COMP-01, TEST-01, DOC-01
**Success Criteria** (what must be TRUE):
  1. An operator can supply a full executable path, including a path containing spaces, through `ExchangeProcessFileName`; the launched `Exchange.exe` receives that selection and uses it for the external client, without changing the API's configured utility target.
  2. Explicit-path launches preserve the Phase 1 authentication, immediate-response, overlapping-admission, and safe error contracts; invalid selected-client paths produce useful errors before a success response.
  3. Existing default console invocation still works, and default and selected-client startup/cleanup follow a consistent lifecycle rather than continuing to assume the original hardcoded client name for overrides.
  4. Invalid executable paths and immediate process-start failures produce useful errors without secrets; launch failures never masquerade as successful PID responses.
  5. Operators can follow configuration instructions and default/explicit-path request and response examples; isolated automated checks demonstrate authentication, defaults, overrides, immediate responses, overlapping admission, cleanup compatibility, and launch failures.
**Plans:** TBD

**Verification:** Exercise the real HTTP/authentication pipeline with test-owned substitutes; assert full paths survive the chosen child transport, working directories/configuration stay coherent, overlap has no admission lock, and controlled lifecycle failures clean up the selected test client consistently. Use test-owned processes only where needed for OS launch/PID behavior; never run real exchange or terminate installed clients. Complete `dotnet test AIPractice1.slnx` under the repository test skill. Documentation must distinguish process creation from exchange success and state verified concurrency limits.

**Discussion before planning:** Determine client ownership and treatment of pre-existing clients, path validation including UNC policy, override transport, host/service identity, and deployment artifacts. Fixed named events may cause interference between admitted utilities; confirm external-client behavior before choosing a mitigation. Preserve `CloseExchng`, `OneTimeExchng`, `ExchngEnd`, `LoadEnd`, and client argument order unless separately approved. If independent concurrent exchange requires an unsupported protocol change, record the limit and request a scope decision rather than silently adding serialization or claiming isolation. Select the supported deployment runtime without expanding into an unapproved framework migration.

## Coverage

All 14 v1 requirements map to exactly one phase: 10 in Phase 1 and 4 in Phase 2. No orphans or duplicates. Tests and usable configuration examples accompany both phases; `TEST-01` belongs to Phase 2 where the full approved behavior matrix is complete. `ERR-01` is established for default launches in Phase 1 and extended to selected-client validation in Phase 2. Operator documentation is finalized with `DOC-01` in Phase 2.

## Progress

Execution order: Phase 1 → Phase 2. Plans within a phase may run in parallel only where their dependencies permit.

| Phase | Plans Complete | Status | Completed |
|-------|----------------|--------|-----------|
| 1. Authenticated Default Launch | 0/TBD | Not started | - |
| 2. Explicit Client Selection and Compatibility | 0/TBD | Not started | - |

---
*Created: 2026-10-27; revised two-phase split approved and applied: 2026-10-29.*
