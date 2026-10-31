---
phase: "01"
slug: "authenticated-default-launch"
status: planned
nyquist_compliant: true
wave_0_complete: false
created: "2026-10-31"
---

# Phase 01 — Validation Strategy

## Test Infrastructure

| Property | Value |
|----------|-------|
| Framework | Existing xUnit 2.9.3 and Microsoft.NET.Test.Sdk 17.14.1 |
| Config file | Tests/Exchange.Tests.csproj; new project references planned |
| Quick run command | Per-task selectors below; initial tracer uses `dotnet test Tests/Exchange.Tests.csproj --filter "FullyQualifiedName~ExchangeApiTests|FullyQualifiedName~ExchangeConfigurationTests|FullyQualifiedName~UtilityProcessLauncherTests" --logger "console;verbosity=normal"` |
| Full suite command | `dotnet test AIPractice1.slnx` |
| Estimated runtime | Not measured; target focused warm run under 30 seconds |

## Sampling Rate

- After each behavior-changing task commit: run the applicable focused tests below, requiring nonzero expected test discovery.
- After each plan wave and before verification: run `dotnet test AIPractice1.slnx`.
- Measure actual feedback latency during execution; target 30 seconds for focused warm runs, not a claimed timing result.
- Never run the actual exchange utility as a success smoke test. Use only harmless test-owned child processes.

## Per-Task Verification Map

Task IDs, waves and threat references are final planning mappings. New test files do not exist at planning HEAD; each named task creates its tests before invoking the command. This is a Nyquist planning audit, not evidence that tests exist or passed. Every test invocation fails validation on nonzero exit, zero matching discovery, absent required cases or skipped required cases.

| Task ID | Plan | Wave | Requirement | Threat Ref | Secure Behavior | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|------------|-----------------|-----------|-------------------|-------------|--------|
| 01-01-T1 | 01-01 | 1 | API-01, AUTH-01, AUTH-02, EXEC-01, EXEC-03, EXEC-04, DEF-01, DEF-02, ERR-01 | T-01-01 through T-01-05 | Fixed target/cwd; real auth; safe settings/PID before owned child release | HTTP/config/process tracer | `dotnet test Tests/Exchange.Tests.csproj --filter "FullyQualifiedName~ExchangeApiTests|FullyQualifiedName~ExchangeConfigurationTests|FullyQualifiedName~UtilityProcessLauncherTests" --logger "console;verbosity=normal"` | No; created by 01-01-T1 | pending execution |
| 01-01-T2 | 01-01 | 1 | API-01, AUTH-02, EXEC-01, DEF-02 | T-01-03 | Accurate blank-key documentation; full regression gate | Solution + documentation review | `dotnet test AIPractice1.slnx` | Existing suite plus T1 tests | pending execution |
| 01-01-T1-arguments | 01-01 | 1 | DEF-01, DEF-02, EXEC-01; D-13 compatibility | T-01-02 | Actual StartClient consumes pure ArgumentList builder; four ordered values survive spaces without secret diagnostics | Pure start-info theory, exactly four cases | `dotnet test Tests/Exchange.Tests.csproj --filter FullyQualifiedName~ExchangeClientStartInfoTests --logger "console;verbosity=normal"` | No; created by 01-01-T1 before invocation | pending execution |
| 01-02-T1 | 01-02 | 2 | AUTH-01, AUTH-02, API-01 | T-01-06, T-01-07 | Generic 401 before malformed-body binding; startup refusal; JSON/environment precedence | HTTP/owned-host tracer | `dotnet test Tests/Exchange.Tests.csproj --filter FullyQualifiedName~ExchangeApiAuthenticationTests --logger "console;verbosity=normal"` | No; created by 01-02-T1 | pending execution |
| 01-02-T2 | 01-02 | 2 | EXEC-01, ERR-01 | T-01-08, T-01-09, T-01-10 | Missing/invalid deployment, immediate failure, no PID/secrets in errors | HTTP/host/process integration | `dotnet test Tests/Exchange.Tests.csproj --filter "FullyQualifiedName~ExchangeApiFailureTests|FullyQualifiedName~UtilityProcessLauncherTests" --logger "console;verbosity=normal"` | New failure tests created by T2; launcher tests from 01-01 | pending execution |
| 01-03-T1 | 01-03 | 3 | EXEC-03, DEF-01, DEF-02, ERR-01 | T-01-11, T-01-12 | Blank matrix, immutable child agreement, internal credential pinning | HTTP/config/process tracer | `dotnet test Tests/Exchange.Tests.csproj --filter "FullyQualifiedName~ExchangeApiDefaultsTests|FullyQualifiedName~ExchangeConfigurationTests" --logger "console;verbosity=normal"` | New default tests created by T1; config tests from 01-01 | pending execution |
| 01-03-T2 | 01-03 | 3 | API-01, EXEC-04, EXEC-05 | T-01-13, T-01-14, T-01-15 | Two live owned children before either release; separate PID/snapshot ownership | Concurrent HTTP/process integration | `dotnet test Tests/Exchange.Tests.csproj --filter FullyQualifiedName~ExchangeApiOverlapTests --logger "console;verbosity=normal"` | No; created by 01-03-T2 | pending execution |
| 01-03-T2-publish | 01-03 | 3 | EXEC-01, ERR-01 | T-01-16 | Inspect publish filenames only, never run utility or read settings | Bounded publish smoke | `dotnet test Tests/Exchange.Tests.csproj --filter FullyQualifiedName~DeploymentContractTests --logger "console;verbosity=normal"` | No; created by 01-03-T2 | pending execution |

All automated commands fail on nonzero exit; test commands also fail validation when zero expected tests are discovered. The executor must check both signals.

## Wave 0 Requirements

- [ ] 01-01-T1 creates harmless executable with bounded named-pipe readiness/release and owned-PID cleanup.
- [ ] 01-01-T1 creates test-owned Kestrel host, synthetic utility configuration and process lifetime fixtures.
- [ ] 01-01-T1 creates the three initial test classes before invoking its selector.
- [ ] 01-01-T1 creates ExchangeClientStartInfoTests and exactly four theory cases before its dedicated selector; zero discovery, fewer/more than four cases, skipped cases, boundary/order mismatch or process/orchestration execution fails validation.
- [ ] 01-01-T1 adds net8.0 web/helper references, nested helper compile exclusions, apphost/dependency staging and solution entries without renaming existing paths.
- [x] Finalize exact task/wave/threat mappings; no TBD entries remain.

Wave 0 is integrated into the first vertical tracer, not a preceding foundation-only plan. `wave_0_complete` remains false until execution creates and verifies infrastructure. First-tracer file-count exception is documented in 01-01; remaining tasks modify at most five files. All plan automated commands have an immediately following fails_when sibling; no command relies on zero-discovery success.

## Threat Coverage

T-01-01–05 are created/checked by 01-01-T1, T-01-06–07 by 01-02-T1, T-01-08–10 by 01-02-T2, T-01-11–12 by 01-03-T1, and T-01-13–16 by 01-03-T2. T-01-15 accepts the explicit D-12 shared external protocol limit; tests prove admission only. Reserved T-01-SC appears in each plan; no new external packages are planned. All other numeric threat IDs are unique within the phase. ASVS L1 uses high blocking threshold; high threats have concrete mitigations/regressions.

## Probe Coverage and Timing

The unchanged deterministic edge report has 14 unresolved raw items. Plans lift nine defensible classified edges to explicit truths and retain all five unclassified rows as flagged unresolved assumptions; see 01-SOURCE-AUDIT.md for one-to-one accounting. Two projectProhibitions-projected descriptor-less prohibitions remain flagged-unverified downstream; this audit does not certify them green or invent check descriptors.

Bound connection/request/readiness waits to 10 seconds, helper release to 30 seconds, cleanup to 5 seconds. No sleeps/timing-only immediacy assertions or process-name cleanup. Warm focused suites target less than 60 seconds and publish smoke has explicit subprocess/test deadlines; execution records actual duration and reports blockers honestly.

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| Successful real exchange and independent concurrency | Outside launch-only contract | Requires separately authorized external Windows client environment | Do not execute during routine verification. Automated admission tests do not establish real exchange isolation. |

All scoped API launch behaviors should have isolated automated verification. Documentation review checks that examples match the final route/configuration and distinguish PID acknowledgement from exchange success.

## Validation Sign-Off

- [x] All tasks have automated verification or explicit test-infrastructure dependencies in their plans.
- [x] No three consecutive tasks lack automated verification.
- [x] First tracer plans creation of missing test infrastructure before invoking focused tests.
- [x] No watch-mode flags; planned waits and owned child cleanup are bounded.
- [ ] Actual feedback latency measured during execution.
- [x] Planning audit confirms complete task/requirement/threat mapping and test-before-command dependencies; `nyquist_compliant: true` records this planning property only.

**Approval:** Final planned validation strategy; all implementation/tests/publish checks remain pending execution. No tests were run while authoring these plans.
