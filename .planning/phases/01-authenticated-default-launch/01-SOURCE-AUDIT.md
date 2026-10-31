# Phase 01 Source Coverage Audit

Planning only, 2026-10-31. Every scoped source item below is covered. Existing source paths were checked using git ls-files; proposed paths are explicitly created by their task. Calibration: factor 1, zero samples, confidence low. Current research and tracked source satisfy discovery; history digest has no completed phases and graphify is disabled. Root owns roadmap/state updates and commits.

## Multi-Source Coverage

| Source | ID | Feature or constraint | Plan/task | Status |
|---|---|---|---|---|
| GOAL | Phase 1 | Authorized separate utility launch, immediate PID/safe defaults, errors and another request while first runs | 01-01-T1; 01-02-T1/T2; 01-03-T1/T2 | COVERED |
| REQ | API-01 | Separate ASP.NET Core admin POST | 01-01-T1 | COVERED |
| REQ | AUTH-01 | Valid key before launch | 01-01-T1; 01-02-T1 | COVERED |
| REQ | AUTH-02 | Absent server key fails closed | 01-01-T1; 01-02-T1 | COVERED |
| REQ | EXEC-01 | Configured utility starts as separate process | 01-01-T1 | COVERED |
| REQ | EXEC-03 | Every blank executable representation selects defaults | 01-01-T1; 01-03-T1 | COVERED |
| REQ | EXEC-04 | Actual PID before child completion | 01-01-T1; 01-03-T2 | COVERED |
| REQ | EXEC-05 | Overlap admission without automatic running-process rejection/queue | 01-03-T2 | COVERED |
| REQ | DEF-01 | Defaults remain in ExchangeProcess/shared resolution | 01-01-T1; 01-03-T1 | COVERED |
| REQ | DEF-02 | Six safe effective default fields agree with execution | 01-01-T1; 01-03-T1 | COVERED |
| REQ | ERR-01 | Applicable path and immediate launch errors exclude secrets | 01-02-T2 | COVERED |
| CONTEXT | D-01 | X-Api-Key | 01-01-T1; 01-02-T1 | COVERED |
| CONTEXT | D-02 | Missing/blank key refuses startup | 01-01-T1; 01-02-T1 | COVERED |
| CONTEXT | D-03 | Standard API JSON/environment configuration | 01-01-T1/T2; 01-02-T1 | COVERED |
| CONTEXT | D-04 | Same generic 401, no echo, zero launches | 01-01-T1; 01-02-T1 | COVERED |
| CONTEXT | D-05 | Manual Windows console hosting | 01-01-T2 | COVERED |
| CONTEXT | D-06 | Separately published full utility target | 01-01-T1/T2; 01-02-T2 | COVERED |
| CONTEXT | D-07 | Utility-folder precedence/cwd and child agreement | 01-01-T1; 01-03-T1 | COVERED |
| CONTEXT | D-08 | Missing utility/assets prevent startup without execution | 01-01-T1; 01-02-T2 | COVERED |
| CONTEXT | D-09 | Class-owned defaults for blank variants | 01-01-T1; 01-03-T1 | COVERED |
| CONTEXT | D-10 | Six safe fields; distinct client cwd | 01-01-T1; 01-03-T1 | COVERED |
| CONTEXT | D-11 | PID acknowledges creation only | 01-01-T1; 01-02-T2; 01-03-T2 | COVERED |
| CONTEXT | D-12 | Approved overlapping admission, shared-protocol limits | 01-01-T2; 01-03-T2 | COVERED |
| CONTEXT | D-13 | Owned harmless tests; preserve protocol/argument order | All code tasks | COVERED |
| RESEARCH | Architecture | Thin Web SDK app, framework auth, pure shared loader/resolver, injected launcher | 01-01-T1 | COVERED |
| RESEARCH | Configuration | API config separate from utility config; no API-owned defaults | 01-01-T1; 01-03-T1 | COVERED |
| RESEARCH | Snapshot | Pin all six consumed leaves including internal credentials; normalize aliases | 01-01-T1; 01-03-T1 | COVERED |
| RESEARCH | Process | Full fixed target, utility cwd, no shell/pipes/wait; dispose local wrapper | 01-01-T1; 01-02-T2 | COVERED |
| RESEARCH | Tests | Real Kestrel loopback port zero; package-free helper; actual production-launcher tracer | 01-01-T1 | COVERED |
| RESEARCH | Isolation | Deterministic readiness/release, bounded waits, child-only env, owned-PID cleanup | 01-01-T1; 01-03-T2 | COVERED |
| RESEARCH | Security | Auth-before-binding, malformed bodies, key startup validation, safe DTO/problems/logs | 01-01-T1; 01-02-T1/T2 | COVERED |
| RESEARCH | Defaults | Blank matrix, millisecond timeout, utility/client cwd separation, relative/UNC paths | 01-03-T1 | COVERED |
| RESEARCH | Concurrency | Second child before first release, response-to-own-PID invariant | 01-03-T2 | COVERED |
| RESEARCH | Compatibility | Argument-free console, original event identifiers/order, existing malformed-config test | 01-01-T1 | COVERED |
| RESEARCH | Client argument boundaries | Pure ExchangeProcess.BuildClientStartInfo consumed by StartClient; four ordered ArgumentList entries preserve credentials/canonical input/output paths containing spaces; four synthetic pure theory cases, no lifecycle/process execution or credential dumps | 01-01-T1, D-13 | COVERED |
| RESEARCH | Deployment | Same-revision utility/API publish, required output existence, manual-run examples | 01-01-T2; 01-03-T2 | COVERED |
| RESEARCH | A1 | Files remain present/parseable during child startup; PID is not exchange success | 01-01-T2; 01-03-T1/T2 | COVERED, explicit limit |
| RESEARCH | Package/style | net8.0, no new packages, existing logging unchanged, CRLF/C# conventions | 01-01-T1 and inherited task rules | COVERED |
| PATTERNS | Build integration | Nested helper compile exclusion, apphost/dependency staging, no production config staging | 01-01-T1 | COVERED |

Other-phase exclusions: EXEC-02, COMP-01, TEST-01 and DOC-01 retain Phase 2 ownership. Focused tests and usable default instructions still accompany Phase 1. No deferred ideas were introduced by context. No source item is missing.

## Dependency and File Ownership

| Plan | Needs | Creates/expands | Wave |
|---|---|---|---|
| 01-01 | Tracked console/options/test/solution patterns | Authenticated default path, pure shared settings, real launcher/helper fixtures, instructions | 1 |
| 01-02 | 01-01 verified tracer and host/launcher/fixture contracts | Authentication/startup/error matrices and safe failures | 2 |
| 01-03 | 01-02 validated host/error behavior | Blank/snapshot/overlap matrices and publish-output checks | 3 |

All plans share API/fixture files, so sequential waves are intentional. Same-wave file overlap is zero because each wave has one plan. The first task in each plan is an end-to-end tracer verified before expansion. First-tracer file-count exception is documented in 01-01; other tasks touch at most five files. No required external setup/credential is needed for isolated implementation; actual production deployment remains user-managed.

## Deterministic Edge Probe Accounting

Source: unchanged .planning/tmp/01-edge-coverage.json, 14 unresolved raw items, five unclassified. Nine classified known edges have defensible explicit truths; raw machine classifications are not rewritten.

| Requirement/category | Authored handling | Location |
|---|---|---|
| API-01/concurrency | Two independent HTTP responses while children alive | 01-03 truths/T2 |
| AUTH-01/concurrency | Each unauthorized request causes zero additional launch, including overlap | 01-02 truths/T1 |
| AUTH-02/concurrency | Server key checked before listening/launch | 01-02 truths/T1 |
| EXEC-01/unclassified | FLAGGED unresolved taxonomy assumption; known configured-target tracer still planned | 01-01 flagged_assumptions |
| EXEC-03/unclassified | FLAGGED unresolved taxonomy assumption; explicit blank matrix still planned | 01-03 flagged_assumptions |
| EXEC-04/unclassified | FLAGGED unresolved taxonomy assumption; immediate PID tracer still planned | 01-01 flagged_assumptions |
| EXEC-05/adjacency | Identical requests do not merge | 01-03 truths/T2 |
| EXEC-05/empty | First launch requires no running child | 01-03 truths/T2 |
| EXEC-05/ordering | Each response belongs to its own PID; arrival order unspecified | 01-03 truths/T2 |
| EXEC-05/idempotency | Repeated input creates distinct children; no deduplication | 01-03 truths/T2 |
| EXEC-05/concurrency | Second acknowledgement before first release | 01-03 truths/T2 |
| DEF-01/concurrency | Independent immutable snapshots and environment | 01-03 truths/T1/T2 |
| DEF-02/unclassified | FLAGGED unresolved taxonomy assumption; safe six-field agreement planned | 01-01 flagged_assumptions |
| ERR-01/unclassified | FLAGGED unresolved taxonomy assumption; applicable errors matrix planned | 01-02 flagged_assumptions |

No-silent-drop equality: 14 = 9 explicit truth mappings + 5 flagged unresolved assumptions. No unclassified row is auto-resolved/dismissed; no fabricated backstop verification is used. These are classification uncertainties, not omissions of their known requirements.

## Prohibition Recall and Projection

Adversarial candidates considered once across the ten requirements: moving orchestration into HTTP; anonymous/fail-open launch; logging caller key; running real exchange as a test; duplicating API defaults; waiting for completion; replacing overlap with a queue; global mutable default settings; exposing credentials in default responses; raw exception disclosure. Precision keeps the two product/safety prohibitions below. Architecture/default/wait/global-state items are already explicit requirement/edge behavior; routine auth/logging/privacy/error hygiene goes to STRIDE rather than duplicating canon prohibitions.

The installed probe-core.cjs projectProhibitions serializer was invoked with unresolved descriptor-less items. Its exact projected statement/status/reason fields are stored under 01-01 and 01-03 must_haves.prohibitions. Neither has any check descriptor. They remain flagged-unverified downstream, never silently green:

- Routine verification must not initiate real exchange or terminate installed client processes (01-01).
- The API must not substitute a queue, single-flight rejection, or serialization for approved overlapping launch admission (01-03).

Secret non-disclosure is an existing approved constraint enforced by T-01-03/T-01-09/T-01-12 and sentinel regressions. It is not minted as a duplicate generic canon prohibition. No new prohibited product behavior or unapproved external API capability is invented.

## Pre-Planning Contributions

plan:pre hooks were rendered and consumed. API coverage uses the reasoned local-process declaration in COVERAGE.md; assumption-delta detected second/another and records no-change per-launch PID identity in 01-03; security registers are present in all plans; no schema-relevant paths were detected and no schema push is needed. No AI/frontend work is in scope. No new one-way contract choice with established consumers exists; initially unpublished contract names remain planning choices. User-approved overlap is not reopened.
