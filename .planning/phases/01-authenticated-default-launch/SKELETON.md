# Walking Skeleton — Exchange Admin API

**Phase:** 01
**Generated:** 2026-10-31
**Status:** Planned architecture; no implementation or passing tests claimed.

## Capability Proven End-to-End

An authorized operator sends a default POST to a separate ASP.NET Core API; shared utility configuration resolves class-owned defaults; the production launcher starts an owned harmless process and returns its actual PID and safe settings before the child is released.

## Architectural Decisions

| Decision | Choice | Rationale |
|---|---|---|
| Framework | Separate net8.0 Microsoft.NET.Sdk.Web application | Retains the existing target and user-selected process boundary |
| Data layer | Utility-folder JSON and inherited environment; no database | The domain persists no application records and requires existing utility configuration |
| Interaction | POST /admin/exchanges with JSON/default input; no UI | Operator HTTP-to-process interaction is the scoped vertical path |
| Authentication | Real X-Api-Key handler/policy; missing server key refuses startup | D-01 through D-04 |
| Deployment | Manual Windows console API plus separately published full utility executable path | D-05/D-06; no service/IIS registration |
| Shared resolution | ExchangeConfigurationLoader.Load and ExchangeProcess.ResolveDefaults | D-07/D-09; pure inspection never invokes orchestration |
| Client argument construction | Pure ExchangeProcess.BuildClientStartInfo consumed by StartClient; four ordered ArgumentList entries | D-13 preserves login/password/input/output boundaries when resolved paths or credentials contain spaces; pure synthetic tests avoid lifecycle execution |
| Child handoff | Immutable per-launch settings/environment, pinned consumed leaves, utility cwd | Response and execution share the same snapshot |
| Public projection | PID plus six named non-secret fields | D-10/D-11; no options or internal snapshot serialization |
| Concurrency identity | Each launch's own PID; no singleton running identity | D-12; admission preserves the shared external protocol |
| Layout | Exchange.Api/, existing Exchange/, Tests/ with nested harmless executable fixture | No file/directory moves; explicit nested compile exclusions |
| Dependencies | Existing packages plus shared-framework/project references | No new third-party package required |

Route, DTO, configuration names and error codes above are planning choices for an initially unpublished contract, not user-approved names. They are coordinated/costly to change but no deployed consumer/migration is established; no new one-way decision gate is introduced. Locked process/auth/protocol choices remain settled.

## Stack Touched in Phase 1

- [ ] Separate web project, existing utility integration, solution/test/helper build references.
- [ ] Real routing and authentication through the default POST.
- [ ] Configuration read from utility JSON plus captured environment; immutable child environment write.
- [ ] Real Windows process creation and immediate PID acknowledgement with a harmless test substitute.
- [ ] Manual Windows local-run/configuration instructions and output-asset publish smoke checks.

The template's database/UI examples are replaced by the actual API-to-process boundary. No database, UI or corresponding tasks are invented.

## Execution Slices

| Plan | Wave | User-visible result |
|---|---|---|
| 01-01 | 1 | Authenticated default launch, safe settings/PID, usable manual configuration |
| 01-02 | 2 | Fail-closed authentication/configuration and sanitized deployment/start errors |
| 01-03 | 3 | Complete blank/snapshot matrix and independently acknowledged overlapping requests |

The first tracer is intentionally one bounded multi-file task because project registration and safe test-process scaffolding must be wired into the end-to-end path together. Its verification must succeed before expansion.

## Subsequent Slice

Phase 2 adds explicit external-client executable selection and lifecycle compatibility, complete milestone verification and operator examples. Existing console invocation, named-event identifiers and client argument order remain supported. API admission does not establish independent successful external exchanges.

## Scope and Evidence Limits

No status/history/stop/restart endpoints, queue, scheduler, database, UI, protocol redesign or actual exchange verification. Required files must remain present and parseable during child startup. Environment pinning handles valid setting changes, not atomic replacement of deployment assets. Production host identity/ACLs/runtime and real deployment remain uninspected. Routine tests use only synthetic settings and owned harmless processes.
