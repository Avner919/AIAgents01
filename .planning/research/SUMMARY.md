# Project Research Summary

**Project:** AIAgents01 — Exchange Admin API
**Domain:** Authenticated Windows process launch API for an existing exchange coordinator
**Researched:** 2026-10-27
**Confidence:** MEDIUM

## Executive Summary

Build a separate, controller-based ASP.NET Core application that authenticates an API key and starts the existing `Exchange.exe` utility. The request optionally selects a full path for the external client used by that utility. Return the utility PID immediately after process creation; this acknowledges launch, not successful exchange or client readiness. When input is blank, report the effective nonsecret defaults used, with default definitions remaining in `ExchangeProcess`.

Use the existing .NET stack, framework authentication/authorization, a small injectable process launcher, and a pure defaults/resolution surface owned by `ExchangeProcess`. Share that surface with the web application without invoking exchange orchestration in the web process. Keep the utility executable location operator-controlled, set its deployment working directory explicitly, and transport the selected external-client path through child-specific settings. A database, job queue, scheduler, and completion dashboard are unnecessary for the confirmed milestone.

The main risks are default/reporting drift, cleanup that still kills processes by the original client name, and overlapping coordinators sharing fixed Windows event names. Resolve those integration contracts before exposing a real launch endpoint. Use isolated resolver, launcher, and HTTP tests; ordinary tests must never execute real exchange or terminate installed clients. The current `net8.0` target minimizes feature scope, but .NET 8 support ends November 10, 2026: production planning needs an explicit supported-runtime decision.

## Key Findings

### Recommended Stack

Detailed evidence: [STACK.md](STACK.md). Retain the current target for a scoped implementation unless an upgrade is separately approved. Microsoft framework/BCL facilities cover this feature without a process-runner or API-key package.

**Core technologies:**

- **ASP.NET Core controllers, `net8.0`:** separate HTTP host matching the utility target; select serviced runtime/package patches during implementation.
- **Framework authentication and authorization:** custom API-key scheme plus explicit controller authorization; fail startup for absent/blank configured credentials.
- **`ProcessStartInfo` and `Process.Start`:** direct launch of the fixed utility executable, explicit working directory, immediate PID capture, no shell or completion wait.
- **Existing `ExchangeProcess`:** class-owned defaults and pure effective-settings resolution shared with the API; no copied default constants.
- **xUnit and `Microsoft.AspNetCore.Mvc.Testing` 8.0.x:** retain established testing conventions and substitute the launcher in HTTP tests.
- **Windows:** required by the current external-client/named-event integration.

**Version requirement:** keep ASP.NET testing package major aligned with its application target. The installed .NET 10 SDK supports the solution format but does not establish production runtime support. Research reports .NET 8 support ending November 10, 2026 and .NET 10 LTS ending November 14, 2028; decide deployment target explicitly before production. [Microsoft support policy](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core)

### Expected Features

Detailed scope and proposed contract refinements: [FEATURES.md](FEATURES.md).

**Must have — user-confirmed:**

- Separate ASP.NET Core admin controller secured by an API key.
- Launch `Exchange.exe` as a separate process; return its PID immediately.
- Accept an optional full external-client executable path through `ExchangeProcessFileName`.
- Use existing defaults for blank input; preserve default ownership in `ExchangeProcess`.
- Inform the caller of effective defaults when blank input selects them, through an explicitly nonsecret response.
- Preserve existing console invocation and external protocol behavior where compatible with the approved override/lifecycle contract.

**Should have — recommended contract details, pending requirements approval:**

- Normalize omitted, null, empty, and whitespace-only field values consistently.
- Validate explicit path forms and return useful sanitized validation/launch errors.
- Prefer HTTP 202 with PID, default-use indicator, and effective settings; clearly describe launch-only semantics.
- Document default and explicit-path requests, authentication failures, and direct launch failures.
- Agree overlap behavior and include focused, isolated automated verification.

**Defer (v2+ or demonstrated need):** completion tracking, status/history endpoints, live logs, kill/restart operations, queues/retries, key-management UI, and a separate defaults-preview endpoint. None is required to satisfy the confirmed milestone.

### Architecture Approach

Detailed boundaries: [ARCHITECTURE.md](ARCHITECTURE.md). Keep the web application a launcher: authentication and authorization precede resolution and every launch side effect. Resolve a coherent launch plan, start the utility once, capture PID, and return. The independent console process owns the external-client protocol and lifecycle.

**Major components:**

1. **Authentication handler and authorized admin controller:** validate API credentials, bind the request, and return an explicit response DTO.
2. **Pure resolver on `ExchangeProcess`:** own default definitions, normalize selection, resolve effective client path/working directory, and expose only safe settings.
3. **Shared console configuration loader:** preserve the utility's configuration provider order and deployment base directory without logging/process side effects.
4. **Injectable utility launcher:** fixed absolute utility path, explicit working directory, child-only override, immediate PID/error contract.
5. **Console entry point and `ExchangeProcess` orchestration:** bind the override, use the shared resolver, preserve named-event/argument contracts, and manage the selected client's lifetime.

Recommend a project reference to the existing Exchange assembly for its pure surface; no existing file moves are needed. Verify publish content and configuration boundaries. Prefer a per-child environment override because the console already uses environment configuration; CLI transport remains an alternative using `ArgumentList`. Exact option names and transport are proposals. Never change the API process environment per request or run exchange inside the controller.

### Critical Pitfalls

Detailed prevention guidance: [PITFALLS.md](PITFALLS.md).

1. **Defaults disagree with execution or reveal credentials.** Share one class-owned resolver and the same console configuration origin/precedence; return an allowlisted nonsecret inventory rather than an options dump. Distinguish class constants from configuration-derived effective settings.
2. **Alternate executable breaks process cleanup.** Current cleanup targets the original client process name globally. Prefer tracking the owned client instance; explicitly decide legacy handling of pre-existing clients. Changing the name used for global killing is insufficient.
3. **Concurrent launches interfere through fixed named events.** Agree admission policy before implementation. A console-level machine-visible gate is recommended; an API-only lock cannot cover manual launches or multiple hosts on one machine. Immediate PID acknowledgement cannot also guarantee synchronous busy rejection without an admission protocol.
4. **Working directory and PID semantics are misunderstood.** Keep utility deployment directory distinct from external-client directory and API content root. Return on OS start, never exchange completion; avoid abandoned redirected output streams and sanitize start failures.
5. **API-key registration leaves the endpoint anonymous.** Require authorization, validate server key configuration, reject missing/invalid/ambiguous credentials before launching, and avoid key disclosure or unnecessary inheritance by the child. Full-path selection grants trusted callers code execution under the Windows service identity.

## Implications for Roadmap

Suggested phases below are recommendations for requirements/roadmap discussion, not approved expansion of scope.

### Phase 1: Shared defaults and safe console override

**Rationale:** Both API reporting and actual client selection depend on a stable console contract. Filename override support must account for existing name-based cleanup before it is usable.

**Delivers:** pure `ExchangeProcess` resolver/descriptor, defined nonsecret default inventory, optional console override, explicit client working-directory behavior, and agreed owned-client cleanup with compatibility coverage.

**Addresses:** optional full path, blank/default behavior, defaults retained in the existing class, unchanged default invocation.

**Avoids:** duplicated defaults, credential disclosure, alternate-client cleanup mismatch, and validation occurring after destructive startup effects.

**Focused verification:** blank variants, default/configuration resolution, full paths with spaces/Unicode, rejected path forms, override transport/binding, and partial-start cleanup using substitutes.

### Phase 2: Separate authenticated admin API

**Rationale:** Build and verify the HTTP trust boundary against a fake launcher before enabling OS process side effects. A settled resolver allows the response to reflect utility behavior.

**Delivers:** separate web project, typed validated operator settings, API-key authentication plus controller authorization, request/response DTOs, recommended launch acknowledgement/error contract, and injectable launcher interface.

**Addresses:** separate admin controller, API key, immediate PID response shape, effective-default disclosure, and useful errors/examples.

**Uses:** ASP.NET Core shared framework, project reference to the pure Exchange surface, xUnit and `WebApplicationFactory`.

**Avoids:** anonymous launch, missing-key fail-open behavior, raw configuration responses, and exchange execution inside HTTP handlers.

**Focused verification:** real authentication pipeline with fake launcher; invalid credentials cause zero launches; blank and explicit paths produce expected effective settings; simulated still-running launch returns immediately; direct start failures do not report success.

### Phase 3: Process integration, overlap policy, and deployment validation

**Rationale:** Wire the real launcher only after defaults and authentication are verified. Deployment and cross-process ownership determine whether a correct HTTP contract works safely on Windows.

**Delivers:** direct fixed-target utility launch, child-only override, explicit utility working directory, coherent parent/child settings, agreed cross-process concurrency behavior, publish/deployment guidance, and operational request examples.

**Addresses:** end-to-end launch integration, actual PID capture, default/override fidelity, and explicit launch-only semantics.

**Avoids:** shared-event interference, wrong configuration roots, inherited API secrets, shell quoting errors, and unowned redirected streams.

**Focused verification:** start-info assertions independent of current directory, parent/child contract checks, substitute-based overlap tests, publish artifact review, and `dotnet test AIPractice1.slnx`. A real exchange smoke test requires a separately authorized environment and is not implied by passing these tests.

### Phase Ordering Rationale

- Define behavior/default ownership before implementing the response that promises those defaults.
- Resolve client lifecycle alongside the override because cleanup currently assumes a fixed client name.
- Verify authorization with substitutes before connecting process creation.
- Treat concurrency as coordinator correctness, not a later scaling feature. Agree policy during requirements; implement the selected protection before exposing real launch.
- After the shared contract stabilizes, authentication and launcher construction can be developed in parallel without changing the dependency order for integration.

### Research Flags

**Needs targeted phase research:**

- **Phase 1:** external-client single-instance behavior, pre-existing client cleanup obligations, alternate client supporting-file assumptions, and the exact default disclosure inventory.
- **Phase 3:** named-event scope and machine-visible admission, immediate-PID versus busy-result semantics, selected Windows host/service identity, UNC policy, and publish/configuration fidelity.

**Standard patterns — skip a separate research phase:**

- **Phase 2:** controller binding, API-key scheme/authorization, DI, ProblemDetails, and fake-launcher integration testing have sufficient framework guidance.
- Pure resolution, direct `ProcessStartInfo` construction, and unit assertions need implementation checks rather than additional broad ecosystem research.

## Confidence Assessment

| Area | Confidence | Notes |
|------|------------|-------|
| Stack | MEDIUM | Official Microsoft evidence and repository target/dependencies; exact serviced versions, project-reference publish behavior, and host remain unvalidated. |
| Features | HIGH for confirmed scope; MEDIUM for proposals | User/project decisions are direct evidence. Status/body, path rules, default inventory, and overlap policy still require approval. |
| Architecture | MEDIUM | Source-backed process boundaries and framework patterns; external-client lifecycle and deployment assumptions need validation. |
| Pitfalls | MEDIUM | Cleanup/event risks are grounded in checked code; prevention designs have not been exercised in the intended Windows environment. |

**Overall confidence:** MEDIUM. Research supports a small implementation with clear boundaries; it does not establish external-client or production-host compatibility.

### Gaps to Address

- **Default inventory:** agree effective path/name, client working directory, relevant catalog/directories, timeout with millisecond units, and whether protocol constants are user-facing. Exclude all credentials.
- **HTTP contract:** settle route, header and configuration names, absent-body semantics, response fields/status, and sanitized failure categories during requirements/phase planning.
- **Path policy:** settle fully qualified local/UNC forms, quotes, extension/existence checks, and service-account permissions; retain the user's full-path capability.
- **Lifecycle/admission:** confirm legacy startup cleanup requirements and approve overlap behavior. Do not silently promise safe parallel exchange because development execution is parallel.
- **Configuration coherence:** decide transport and how parent resolution matches child settings when environment overrides or reloads exist.
- **Deployment/runtime:** choose Windows hosting model, verify process survival expectations and artifact layout, select serviced patches, and explicitly resolve the imminent .NET 8 support deadline before production.
- **Validation limits:** research ran no package compatibility build, production process, or real exchange. Subsequent implementation must follow repository test guidance and report the checks actually performed.

## Sources

### Primary evidence

- [STACK.md](STACK.md), [FEATURES.md](FEATURES.md), [ARCHITECTURE.md](ARCHITECTURE.md), [PITFALLS.md](PITFALLS.md) — synthesized research, source provenance, and unresolved decisions.
- Repository/project evidence cited by the researchers: `.planning/PROJECT.md`, `.planning/codebase/`, `Exchange/ExchangeProcess.cs`, `Exchange/Program.cs`, and `Exchange/Exchange.csproj` — confirmed scope, class defaults, configuration order, cleanup, and named-event protocol.
- [Microsoft .NET support policy](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core) — framework support deadlines.
- [ASP.NET Core authentication](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/?view=aspnetcore-8.0) — schemes, handlers, and authorization distinction.
- [ASP.NET Core web APIs](https://learn.microsoft.com/en-us/aspnet/core/web-api/?view=aspnetcore-8.0) — controller validation and ProblemDetails conventions.
- [ControllerBase.Accepted](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.controllerbase.accepted?view=aspnetcore-10.0) — proposed 202 response helper; research reports applicability to ASP.NET Core 8.
- [Process.Start](https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.process.start?view=net-8.0) and [UseShellExecute](https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.processstartinfo.useshellexecute?view=net-8.0) — process creation, direct execution, and working-directory semantics.
- [ArgumentList](https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.processstartinfo.argumentlist?view=net-10.0) — structured argument transport alternative; target compatibility remains an implementation check.
- [Process.Close](https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.process.close?view=netframework-4.8) — releasing wrapper resources; research reports applicability to .NET 8.
- [Options pattern](https://learn.microsoft.com/en-us/dotnet/core/extensions/options), [project references](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-reference-add), and [ASP.NET Core integration tests](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-8.0) — configuration validation, dependency boundaries, and isolated HTTP verification.

### Evidence limitations

Official sources were consulted by the researchers on 2026-10-27; this synthesis adds no new browsing. Some API references were available through another target-version view. Recommendations about cleanup, concurrency, default fields, transport, and hosting are engineering synthesis requiring phase validation, not externally established client guarantees. No competitor or market-demand assumptions were used.

---
*Research completed: 2026-10-27*
*Ready for roadmap: yes, after requirements resolve the flagged contract decisions.*
