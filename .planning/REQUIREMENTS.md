# Requirements: AIAgents01 — Exchange Admin API

**Defined:** 2026-10-27
**Core Value:** An authorized caller can start the exchange utility with an explicit client executable or clearly reported defaults and immediately receive the launched process ID.
**Approval:** The user approved the complete v1 list in this chat before this document was created.

## v1 Requirements

### Admin API

- [ ] **API-01**: An operator can call an admin POST endpoint hosted in a separate ASP.NET Core application.

### Authentication

- [ ] **AUTH-01**: An operator must provide a valid API key; missing or invalid keys are rejected before any process is launched.
- [ ] **AUTH-02**: Missing server API-key configuration prevents anonymous access to the launch endpoint.

### Execution

- [ ] **EXEC-01**: An authorized operator can launch the configured `Exchange.exe` as a separate operating-system process.
- [ ] **EXEC-02**: An operator can provide an optional full external-client executable path through `ExchangeProcessFileName`.
- [ ] **EXEC-03**: Omitted, null, empty, or whitespace-only executable input selects the existing defaults.
- [ ] **EXEC-04**: An operator receives the launched utility process ID immediately after process start, without waiting for exchange completion.
- [ ] **EXEC-05**: An operator can submit overlapping launch requests; an existing exchange process does not by itself cause a request to be rejected or queued.

### Defaults

- [ ] **DEF-01**: Default definitions remain in `ExchangeProcess` and are shared with API resolution rather than copied into the API.
- [ ] **DEF-02**: For blank executable input, the operator receives the effective client filename, full executable path, client working directory, input/output directories, and timeout in milliseconds, excluding credentials.

### Error Handling

- [ ] **ERR-01**: An operator receives useful errors for invalid executable paths and immediate process-start failures without disclosure of secrets.

### Compatibility

- [ ] **COMP-01**: Existing default console invocation remains supported, and client launch and cleanup consistently support the selected executable.

### Verification

- [ ] **TEST-01**: Automated tests cover authentication, defaults, overrides, immediate responses, and launch failures using isolated substitutes.

### Documentation

- [ ] **DOC-01**: Operators have configuration instructions and request/response examples for default and explicit-path launches.

## User Stories

- As an authorized operator, I can select an external client executable and start the existing console utility through an API.
- As an operator using blank input, I can see which non-secret effective defaults were selected.
- As an operator, I receive the utility PID promptly and can submit another launch without waiting for the first exchange to finish.

## Acceptance Criteria

- Authentication failure causes zero launches; absent server key configuration never makes the endpoint anonymous (`AUTH-01`, `AUTH-02`).
- A full client executable path reaches the console utility without changing the fixed utility launch target (`EXEC-01`, `EXEC-02`).
- All blank-input variants use the same class-owned defaults, and reported values agree with the effective execution configuration (`EXEC-03`, `DEF-01`, `DEF-02`).
- A response containing the utility PID can complete while that utility is still running (`EXEC-04`). A PID acknowledges process creation, not successful exchange.
- Overlapping valid requests are admitted without an automatic single-flight rejection or queue (`EXEC-05`). Development-plan parallelism is a separate preference.
- An immediate launch failure returns an error rather than a successful PID response (`ERR-01`). Later child failure is outside the immediate response contract.
- Default and override scenarios retain coherent launch/cleanup behavior and pass isolated regression checks (`COMP-01`, `TEST-01`).

## Definition of Done

- Every v1 requirement is mapped to exactly one roadmap phase and verified against observable behavior.
- Code changes follow `AGENTS.md`, `.codex/rules/unit-tests.md`, and `.agents/skills/run-and-fix-tests/SKILL.md`.
- `dotnet test AIPractice1.slnx` passes before code changes are marked complete; any blocker is reported accurately.
- Automated launch tests use substitutes and never initiate real exchange or terminate installed clients.
- Documentation includes authentication, default disclosure, override examples, launch-only PID semantics, and the approved overlapping-launch behavior.
- No credentials or API keys are committed, logged, or returned; protected environment files and existing file/directory paths remain untouched.

## v2 Requirements

No additional requirements are committed for a later release. Completion tracking, scheduling, and process-management endpoints would require separate approval.

## Out of Scope

| Feature | Reason |
|---------|--------|
| Waiting for exchange completion in the POST response | User selected immediate utility PID acknowledgement |
| Completion/status/history endpoints | Excluded from the approved v1 scope |
| Scheduling, queues, and automatic retries | Excluded from v1; overlapping requests are allowed |
| Process-management endpoints such as stop/restart | Excluded from the approved v1 scope |
| Running exchange orchestration inside the web process | User selected a separate API that launches `Exchange.exe` |
| API-owned copies of client defaults | Defaults must remain in `ExchangeProcess` |
| Exposing credentials in a defaults response | Repository guidance and approved scope require non-secret fields only |
| Unrelated cleanup or framework migration | Requires a separate scope decision |

## Planning Notes

- Executable selection is for the external client, while the API's utility target is configured by the operator.
- Existing named events and process-name cleanup can cause interference between overlapping exchanges; the implementation plan must address this against `EXEC-05` and `COMP-01` without silently replacing the approved policy with serialization.
- Route, API-key header/configuration names, override transport, path-validation details, and response status/body names are implementation decisions for phase planning.
- Client working directory is distinct from the console utility's deployment/configuration working directory.
- Existing configuration-derived values remain governed by the console configuration sources; sharing resolution does not authorize moving settings values or secrets into class constants.

## Traceability

All 14 v1 requirements are assigned to exactly one roadmap phase. All requirements currently have status Pending.

| Requirement | Phase | Status |
|-------------|-------|--------|
| API-01 | Phase 1 | Pending |
| AUTH-01 | Phase 1 | Pending |
| AUTH-02 | Phase 1 | Pending |
| EXEC-01 | Phase 1 | Pending |
| EXEC-02 | Phase 2 | Pending |
| EXEC-03 | Phase 1 | Pending |
| EXEC-04 | Phase 1 | Pending |
| EXEC-05 | Phase 1 | Pending |
| DEF-01 | Phase 1 | Pending |
| DEF-02 | Phase 1 | Pending |
| ERR-01 | Phase 1 | Pending |
| COMP-01 | Phase 2 | Pending |
| TEST-01 | Phase 2 | Pending |
| DOC-01 | Phase 2 | Pending |

**Coverage:**
- v1 requirements: 14 total
- Mapped to phases: 14
- Unmapped: 0
- Duplicate phase assignments: 0

---
*Requirements defined: 2026-10-27*
*Last updated: 2026-10-29 after approval of revised roadmap mappings; v1 requirements unchanged*
