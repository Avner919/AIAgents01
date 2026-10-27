# Feature Research

**Domain:** API-key-protected Windows exchange process launcher
**Researched:** 2026-10-27
**Confidence:** HIGH for confirmed scope and checked source; MEDIUM for proposed contract details pending requirements approval.

## Feature Landscape

This is a focused administrative integration, not a general job scheduler. Requirements labeled **Confirmed** come from `PROJECT.md` and the user's decisions. Items labeled **Proposed** are recommendations for requirements discussion rather than approved product expansion.

### Table Stakes (Users Expect These)

| Feature | Why Expected | Complexity | Notes |
|---------|--------------|------------|-------|
| Separate ASP.NET Core admin controller | Keeps HTTP hosting separate from existing exchange orchestration | MEDIUM | **Confirmed.** API launches `Exchange.exe`; it does not directly run the external client. |
| API-key protection | Launches execute under the server identity | MEDIUM | **Confirmed.** **Proposed:** dedicated request header, authenticated endpoint authorization, reject missing/invalid keys before launching; no configured key must never mean anonymous access. Header/configuration names remain undecided. |
| Optional full client executable path | Operator selects the external client per request | MEDIUM | **Confirmed.** The supplied path is for the client selected by `ExchangeProcessFileName`, not a replacement path for `Exchange.exe`. Transport must preserve spaces and Unicode. |
| Omitted/blank field uses existing defaults | Preserves existing default invocation | MEDIUM | **Confirmed.** **Proposed:** consistently treat missing, null, empty, and whitespace-only values as default selection; explicitly test each. |
| One source of default values | Response and executed behavior must agree | MEDIUM | **Confirmed.** Defaults stay in `ExchangeProcess`; expose a reusable resolver/descriptor without copying constants into the API. |
| Applied defaults in the launch response | Caller must know what blank input selected | MEDIUM | **Confirmed.** Return effective non-secret executable/default behavior and whether defaults were applied. Define the exact non-secret field list during requirements; do not return entire configuration. |
| Immediate utility process ID | Caller does not wait for exchange completion | LOW | **Confirmed.** Return after OS process start, not after exchange or client readiness. PID is for `Exchange.exe`, not its eventual external client. |
| Useful validation and launch errors | Bad requests should not look like successful starts | MEDIUM | **Proposed:** validate a supplied absolute executable path; malformed input yields 400; launch failure yields an appropriate server error. Use consistent ProblemDetails without secrets or raw exception dumps. File existence cannot guarantee later launch success. |
| Preserve console compatibility | Existing operators still need the current invocation | MEDIUM | **Confirmed constraint.** Preserve argument order, named-event protocol, timeout units, and default client selection. |
| Isolated automated verification | Tests must not kill or launch real exchange clients | MEDIUM | **Confirmed.** Cover authentication, default/override resolution, launch failure, immediate PID response and cross-process override transport with substitutes. |

### Differentiators (Value Within This Scope)

| Feature | Value Proposition | Complexity | Notes |
|---------|-------------------|------------|-------|
| Transparent effective settings | Operators see exactly which non-secret defaults were selected | MEDIUM | Already **Confirmed**, so include in v1 rather than treating transparency as optional polish. Prefer reporting effective selection for explicit paths too (**Proposed**). |
| Explicit launch acknowledgement | Distinguishes process start from successful exchange | LOW | **Proposed:** HTTP 202 with PID and applied defaults. Microsoft supplies `Accepted(object)`; final route/status/body remain implementation decisions. Do not invent a status URL without a status endpoint. |
| Clear request/response examples | Helps operators distinguish utility and external-client executable paths | LOW | **Proposed:** document default request, explicit-path request, authentication failure and launch failure alongside the API contract. No UI required. |

### Anti-Features (Avoid in This Milestone)

| Feature | Why Requested | Why Problematic | Alternative |
|---------|---------------|-----------------|-------------|
| Wait for exit or exchange success | Appears to provide stronger acknowledgement | Conflicts with immediate-response decision; ties HTTP lifetime to exchange timeout | Return PID and explicitly define launch-only semantics. |
| Arbitrary executable/argument execution endpoint | Makes launcher reusable | Broadens capability beyond the requested external-client override | Fix the utility target in trusted server configuration; accept only the requested client path field. |
| Duplicated default constants or full settings echo | Seems easiest for populating response | Defaults drift; configuration may contain credentials | Read class-owned descriptors and return a deliberate non-secret projection. |
| Process kill, restart, status dashboard or live logs | Natural operations-console extensions | Adds ownership, PID reuse, retention and lifecycle contracts the user did not request | Document launch-only behavior; revisit only on explicit demand. |
| Queue/retries/distributed scheduler | Could absorb repeat requests | Repeated runs can interfere with shared named events and broad process cleanup | Define overlap behavior before allowing concurrent launch; do not silently retry side effects. |
| OAuth/users/roles/key management UI | Could support many operators | Replaces or greatly expands selected API-key scope | A configured secret and protected admin endpoint for v1. |

## Feature Dependencies

```text
Default/override launch contract in ExchangeProcess
    -> child-process override transport
    -> API launch service
    -> immediate PID + applied-defaults response

API-key authentication + endpoint authorization
    -> protected launch endpoint

Path normalization + deliberate working directory
    -> predictable client selection and existing configuration loading

Launch abstraction + isolated process substitute
    -> safe HTTP and transport regression tests
```

### Dependency Notes

- Resolve default ownership and override transport before the response DTO; otherwise the API can report values different from those the utility uses.
- Authentication registration alone does not protect the controller: an authorization policy/attribute must require authentication [S2].
- Define malformed input, launch failure and blank-value behavior before contract tests. `[ApiController]` can provide automatic validation responses [S1].
- Existing shared named events and client-process cleanup make overlapping launches a requirements question. A single-flight restriction is a **proposal**, not an approved feature; do not imply safe parallel exchange from parallel development preferences.

## MVP Definition

### Launch With (v1)

- [ ] Separate controller-based API with API-key enforcement.
- [ ] Optional full client path passed to the existing utility with backward-compatible default invocation.
- [ ] Defaults owned by `ExchangeProcess` and effective non-secret defaults reported when input is blank.
- [ ] Immediate launched utility PID with documented launch-only semantics.
- [ ] Agreed path validation, launch failure behavior and overlap policy.
- [ ] Isolated authentication, resolution, response and process-transport coverage; operator examples.

### Add After Validation (v1.x)

No additional product features are required by current scope. Consider only if operators demonstrate a need:

- [ ] Separate defaults-preview endpoint, if knowing defaults before launch becomes necessary.
- [ ] Request deduplication, if clients need reliable retries after an ambiguous network failure.

### Future Consideration (v2+)

Process history, completion tracking and key rotation administration need explicit requirements. They are not prerequisites for this milestone.

## Feature Prioritization Matrix

| Feature | User Value | Implementation Cost | Priority |
|---------|------------|---------------------|----------|
| Protected separate API + child launch | HIGH | MEDIUM | P1 |
| Optional override + class-owned defaults + disclosure | HIGH | MEDIUM | P1 |
| Immediate PID contract | HIGH | LOW | P1 |
| Validation/errors + safe regression coverage | HIGH | MEDIUM | P1 |
| Contract examples | MEDIUM | LOW | P1 |
| Defaults preview endpoint | MEDIUM | LOW | P2, conditional |
| Tracking/dashboard/scheduler | Unestablished | HIGH | Deferred |

## Competitor Feature Analysis

Not applicable: this is an extension of a specific Windows utility. Framework capabilities are the relevant comparison; no market-demand claims or competitor requirements were inferred.

| Concern | Framework Capability | Our Approach |
|---------|----------------------|--------------|
| Request validation | ASP.NET Core controller validation and ProblemDetails [S1] | Use framework conventions and add explicit optional-path rules. |
| Authentication | Extensible authentication schemes and authorization [S2] | API-key handler plus enforcement on launch endpoint. |
| Immediate response | `ControllerBase.Accepted(object)` returns 202 [S3] | Recommended acknowledgement carrying PID/defaults; no completion claim. |

## Confidence and Remaining Decisions

- Confirmed requirements and current default placement: **HIGH**, direct project context and checked `Exchange/ExchangeProcess.cs`.
- Proposed contract decisions: **MEDIUM**, grounded in official framework documentation but pending requirements approval.
- Documentation provenance: Microsoft sources read on 2026-10-27. One .NET 8 web-API URL failed to open; matching Microsoft documentation and current API reference were consulted. No patch-level feature assumptions are required.
- The automated provider-tier classifier returned LOW for `webfetch --verified`; this conservative tool tier does not establish that proposed behavior is approved. Official citations and directly confirmed scope are distinguished above.
- Settle exact applied-defaults fields, API-key header/configuration names, request-body absence semantics, HTTP status/body, valid path forms, and concurrent-launch policy during requirements/phase planning.

## Sources

- **Project evidence:** `.planning/PROJECT.md`; `Exchange/ExchangeProcess.cs` (default filename construction and catalog resolution). No sensitive configuration read or reproduced.
- **[S1]** [Microsoft: Create web APIs with ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/web-api/?view=aspnetcore-8.0) — controller conventions, automatic 400 and validation ProblemDetails; alternate Microsoft search extraction consulted when direct open failed.
- **[S2]** [Microsoft: ASP.NET Core authentication overview](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/?view=aspnetcore-8.0) — custom handlers, challenge/forbid, and need for authorization enforcement.
- **[S3]** [Microsoft: ControllerBase.Accepted](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.controllerbase.accepted?view=aspnetcore-10.0) — 202 response helpers; reference applicability includes ASP.NET Core 8.

---
*Feature research for AIAgents01 — Exchange Admin API.*
