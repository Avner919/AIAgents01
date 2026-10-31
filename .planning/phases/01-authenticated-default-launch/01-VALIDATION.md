---
phase: "01"
slug: "authenticated-default-launch"
status: draft
nyquist_compliant: false
wave_0_complete: false
created: "2026-10-31"
---

# Phase 01 — Validation Strategy

## Test Infrastructure

| Property | Value |
|----------|-------|
| Framework | Existing xUnit 2.9.3 and Microsoft.NET.Test.Sdk 17.14.1 |
| Config file | Tests/Exchange.Tests.csproj; new project references planned |
| Quick run command | `dotnet test Tests/Exchange.Tests.csproj --filter "FullyQualifiedName~ExchangeApiTests|FullyQualifiedName~ExchangeConfigurationTests|FullyQualifiedName~UtilityProcessLauncherTests"` |
| Full suite command | `dotnet test AIPractice1.slnx` |
| Estimated runtime | Not measured; target focused warm run under 30 seconds |

## Sampling Rate

- After each behavior-changing task commit: run the applicable focused tests below, requiring nonzero expected test discovery.
- After each plan wave and before verification: run `dotnet test AIPractice1.slnx`.
- Measure actual feedback latency during execution; target 30 seconds for focused warm runs, not a claimed timing result.
- Never run the actual exchange utility as a success smoke test. Use only harmless test-owned child processes.

## Per-Task Verification Map

Plan/task IDs and threat references are provisional until the planner finalizes them; the planner must replace these placeholders with exact plan/task and phase-unique threat IDs.

| Task ID | Plan | Wave | Requirement | Threat Ref | Secure Behavior | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|------------|-----------------|-----------|-------------------|-------------|--------|
| tracer | TBD | 1 | API-01, EXEC-01, EXEC-04 | TBD | Only configured test utility launches; PID response precedes child release | HTTP/process integration | `dotnet test Tests/Exchange.Tests.csproj --filter "FullyQualifiedName~ExchangeApiTests|FullyQualifiedName~UtilityProcessLauncherTests"` | No; first tracer must create | pending |
| auth | TBD | TBD | AUTH-01, AUTH-02 | TBD | Generic 401 and zero launches; missing server key prevents startup | HTTP/host integration | `dotnet test Tests/Exchange.Tests.csproj --filter FullyQualifiedName~ExchangeApiTests` | No; planned | pending |
| defaults | TBD | TBD | EXEC-03, DEF-01, DEF-02 | TBD | Blank variants agree; allowlisted settings match child snapshot without secrets | Configuration/HTTP integration | `dotnet test Tests/Exchange.Tests.csproj --filter "FullyQualifiedName~ExchangeConfigurationTests|FullyQualifiedName~ExchangeApiTests"` | No; planned | pending |
| overlap | TBD | TBD | EXEC-05 | TBD | Second request acknowledged while first child remains unreleased | Concurrent HTTP/process integration | `dotnet test Tests/Exchange.Tests.csproj --filter FullyQualifiedName~ExchangeApiTests` | No; planned | pending |
| failures | TBD | TBD | ERR-01 | TBD | Deployment failures prevent startup; immediate errors contain no success PID or secrets | HTTP/host/process integration | `dotnet test Tests/Exchange.Tests.csproj --filter "FullyQualifiedName~ExchangeApiTests|FullyQualifiedName~UtilityProcessLauncherTests"` | No; planned | pending |

All automated commands fail on nonzero exit; test commands also fail validation when zero expected tests are discovered. The executor must check both signals.

## Wave 0 Requirements

- [ ] Add harmless executable fixture with bounded readiness/release and cleanup by owned PID only.
- [ ] Add test-owned HTTP host, synthetic utility configuration, and process lifetime fixtures.
- [ ] Add ExchangeApiTests, ExchangeConfigurationTests, and UtilityProcessLauncherTests under Tests/ following repository test rules.
- [ ] Add web/helper project references and solution entries without renaming existing paths.
- [ ] Finalize task and threat mappings when plans are generated.

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| Successful real exchange and independent concurrency | Outside launch-only contract | Requires separately authorized external Windows client environment | Do not execute during routine verification. Automated admission tests do not establish real exchange isolation. |

All scoped API launch behaviors should have isolated automated verification. Documentation review checks that examples match the final route/configuration and distinguish PID acknowledgement from exchange success.

## Validation Sign-Off

- [ ] All tasks have automated verification or explicit test-infrastructure dependencies.
- [ ] No three consecutive tasks lack automated verification.
- [ ] First tracer creates missing test infrastructure before invoking focused tests.
- [ ] No watch-mode flags; waits and owned child cleanup are bounded.
- [ ] Actual feedback latency measured during execution.
- [ ] `nyquist_compliant: true` only after audit confirms it.

**Approval:** Draft validation strategy; not evidence of passing implementation tests.
