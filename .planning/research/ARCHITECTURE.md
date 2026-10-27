# Architecture Research

**Domain:** API-key-protected Windows process launch API for an existing exchange coordinator
**Researched:** 2026-10-27
**Confidence:** MEDIUM — official .NET/ASP.NET documentation cross-checked against repository source; deployment and external-client behavior require phase validation. Confidence classification obtained with `classify-confidence --provider websearch --verified`.

## Standard Architecture

### System Overview

```text
Authorized caller
    |
    v
ASP.NET Core: API-key authentication -> authorized AdminController
    |                                      |
    |                              validate + resolve launch plan
    |                                      |
    |                              IExchangeLauncher
    |                                      |
    +<-- launch acknowledgement + PID <--- Process.Start
                                           |
                                           v
                                 Exchange.exe, separate process
                                           |
                                 ExchangeProcess orchestration
                                           |
                                 selected external client .exe
```

Keep the web app a launcher. Never instantiate `ExchangeProcess` or call `StartExchangeProcess` in a request. Exchange startup/cleanup can kill client processes and uses blocking named-event waits. PID acknowledgement means the utility was created, not that client startup or exchange succeeded.

### Component Responsibilities

| Component | Responsibility | Typical implementation |
|-----------|----------------|------------------------|
| API authentication handler | Authenticate API-key header, create principal, issue challenge | Custom `AuthenticationHandler<TOptions>` registered through `AddScheme`; controller authorization policy |
| AdminController | Bind optional path, request a launch, return PID and effective settings | `[ApiController]`, `[Authorize]`, POST action |
| Effective launch resolver | Own default selection, full-path normalization, safe settings projection | Public pure static method on `ExchangeProcess`, returning immutable launch settings |
| Console configuration loader | Load the existing providers in existing order from a specified console deployment directory | Shared helper inside the existing Exchange assembly; do not initialize NLog or run orchestration here |
| IExchangeLauncher | Start only the configured Exchange utility and return its PID | Injectable adapter around `Process.Start` |
| Console entry point | Read optional override, resolve settings, run exchange | Existing `Exchange/Program.cs` with narrowly scoped input changes |
| ExchangeProcess | Own default constants and external-client lifecycle | Existing class; add override support without duplicating constants |

These boundaries are recommendations, not existing implementation. ASP.NET authentication alone does not secure endpoints: apply authorization explicitly or use an authenticated fallback policy. [S1]

## Recommended Project Structure

```text
Exchange/                         existing executable; keep existing file paths
    ExchangeProcess.cs            default constants + pure resolver + orchestration
    ExchangeConfiguration.cs      new reusable configuration loader
    Options/                      existing models; optional client path property
Exchange.AdminApi/                new independent ASP.NET Core host
    Program.cs                    composition, configuration validation, middleware
    Controllers/AdminController.cs
    Authentication/ApiKeyAuthenticationHandler.cs
    Launching/IExchangeLauncher.cs
    Launching/ExchangeLauncher.cs
    Contracts/                    request and nonsecret response types
Tests/                            existing tests plus focused console/resolver checks
Exchange.AdminApi.Tests/          HTTP authentication/launch contract tests
```

Use a project reference from the API to `Exchange/Exchange.csproj` for its pure settings/resolver surface. Loading an assembly is separate from invoking its entry point; keep helper calls free of process/logging side effects. This small milestone does not require moving existing files to a shared library, which would also conflict with the repository's rename prohibition. Validate publish output: project references can affect copied content, and API publication must not accidentally embed deployment credentials or substitute its own configuration for the console's. A separate shared library is only worthwhile if ownership can remain in `ExchangeProcess` without relocation or duplicate compilation. Project references are the supported SDK dependency mechanism. [S5]

## Architectural Patterns

### Pattern 1: One resolver for behavior and reported defaults

Normalize omitted, null, empty, and whitespace-only request values into the default branch. Keep the existing client name/file constants inside `ExchangeProcess`; expose behavior through a pure resolver rather than copying constants into the controller. Default resolution combines configured `AgentCatalog` with the class-owned filename. An explicit value must be a fully qualified executable path: check `Path.IsPathFullyQualified`, normalize with `Path.GetFullPath`, validate existence and executable extension, and define UNC support explicitly.

Return an allowlisted DTO with `usedDefaults`, effective client executable path/name, client working directory, configured catalog, input/output directories, and timeout milliseconds. Include class-owned protocol defaults if these are part of the promised default inventory. Never serialize `ExchangeOptions` directly: it contains client authentication settings. Distinguish class defaults from configuration-derived effective values so the caller sees what will actually apply. Scope of “all defaults” must be finalized in requirements; the source contains configuration values as well as class constants.

API and console must use the same configuration source and override precedence. Load console configuration from its deployment directory, not ASP.NET content root; preserve JSON, JSON, environment ordering. Resolve once for the launch plan and pass the selected client value explicitly to the child, including the resolved default selection, to prevent inherited environment values from silently selecting another client. Configuration reload between parent resolution and child startup can still produce mismatched other settings; document immutable deployment configuration during launch or pass a coherent configuration snapshot through child environment overrides.

### Pattern 2: Direct child-process launch with per-child override

Recommend a narrowly named environment override such as `Exchange__ExchangeProcessFileName`, added to **this child's** `ProcessStartInfo.Environment` only. Add the corresponding nullable option read by the console. This reuses its existing environment provider and avoids shell quoting or changing machine-wide state. Do not call `Environment.SetEnvironmentVariable` per request. Remove the API-key variable from the child's inherited environment where practical; it is unnecessary for exchange.

The executable in API `ProcessStartInfo.FileName` is an administrator-configured absolute path to `Exchange.exe`; request input selects the external client, never the API's directly launched executable. Set `UseShellExecute = false`, `CreateNoWindow = true`, and console working directory explicitly to its deployment folder. The console currently reads configuration/NLog relative to working directory. For the external client, use the selected executable's directory as its working directory; default selection preserves `AgentCatalog`. Confirm the external protocol's assumptions about supporting files. [S2]

If CLI transport is preferred later, use a named option and `ArgumentList.Add` per token, never concatenated `Arguments`, `cmd.exe`, or PowerShell. The same argument-list approach should protect the existing external-client positional arguments while preserving their order. Strings added to .NET `ArgumentList` are escaped by the framework. [S3]

### Pattern 3: Start acknowledgement with explicit resource ownership

`IExchangeLauncher.Start` returns an integer PID after successful `Process.Start`; no `WaitForExit`, client readiness wait, or exchange timeout occurs in the HTTP request. Return HTTP 202 with PID and the effective settings; label it started/accepted rather than completed. Do not invent a status URL when no status endpoint exists.

Read PID before disposing the local `Process` wrapper. Disposing releases process-associated handles through `Close`; it is not a request to kill the child. Do not redirect stdout/stderr unless a separately owned consumer drains both streams for the entire child lifetime; an immediate response with abandoned pipes can stall the child. Surface OS launch failures as sanitized problem responses, without raw exception/configuration dumps. PID reuse makes PID unsuitable as a durable job identifier. [S4]

### Pattern 4: Isolated launch tests

Use `WebApplicationFactory` and replace `IExchangeLauncher` in test dependency injection. Authenticate through the real API-key handler with test-only settings. Verify missing/incorrect keys never invoke the launcher, full paths containing spaces remain intact, blank variants report the resolver's defaults, and a fake still-running launch returns immediately with the fake PID. Separately test start-info construction and console option resolution without running real exchange. An OS smoke test may use a harmless dedicated test executable, never the production utility with normal configuration. [S6]

## Data Flow

1. Authenticate API key before any launch side effect or disclosure of effective paths.
2. Bind request; classify blank versus explicit executable selection; reject malformed/relative explicit paths.
3. Load console configuration from its configured deployment directory and resolve the nonsecret launch plan using `ExchangeProcess`.
4. Build the child start info using fixed utility path, explicit working directory and per-child selected client override.
5. Start once, capture PID, release wrapper handles, return immediate acknowledgement and effective settings.
6. In the independent utility, bind the override, run the same resolver, and execute the existing Windows protocol.

No database, message broker, background queue, or durable job record is required for this milestone. If a durable task lifecycle becomes a requirement, add it explicitly rather than treating the PID response as one.

## Scaling Considerations

| Scale | Architecture adjustments |
|-------|--------------------------|
| One deployment, one active exchange | Appropriate initial boundary; explicitly prevent overlapping coordinators |
| Multiple API instances on one Windows host | API-local locks are insufficient; use a machine-visible coordinator gate |
| Multiple exchange hosts | Route to an explicit worker host; global event names and external-client behavior prevent naive horizontal scaling |

### Scaling Priorities

1. **First bottleneck: correctness under concurrent starts.** Existing fixed named events and broad process termination allow coordinators to interfere. Recommend a named single-instance mutex in the console before event/client management, with defined busy diagnostics; an API gate may improve feedback but must not be the sole safety boundary. Exact immediate-acknowledgement/busy semantics need phase design because a child can exit busy immediately after PID acknowledgement.
2. **Second bottleneck: process ownership.** `StopClients()` currently filters a hardcoded default process name. An alternate filename would survive cleanup while unrelated default clients can still be killed. Retain the launched `Process` and clean up that owned instance; decide startup handling of pre-existing clients deliberately. Merely deriving a new name broadens termination to unrelated same-named programs. This is a required override integration decision, not unrelated cleanup work.

## Anti-Patterns

### Running exchange inside the controller

It blocks request threads on named events and moves process termination into the web host. Start the utility as a separate process.

### Reporting defaults from duplicated web settings

Parent and child can disagree. Keep default definitions in `ExchangeProcess`, use a shared resolver and coherent configuration, and return an explicit nonsecret projection.

### Treating API-key possession as unconstrained host administration

A full-path override intentionally selects executable code executed by the service identity. Use a least-privileged Windows identity and protected deployment/configuration; document that authenticated callers are trusted to select external-client executables. API clients must not select the Exchange utility path itself. Optional directory policies require an explicit product decision, since the user requested full-path support.

## Integration Points

| Boundary | Communication | Notes |
|----------|---------------|-------|
| Caller -> API | HTTPS POST + API-key header | Authenticate and authorize before launch; never put key in URLs |
| API -> shared Exchange surface | Project reference; pure resolver/configuration helper | No orchestration calls; avoid secret DTO serialization |
| API -> utility | OS child process + per-child environment override | Fixed utility executable; console deployment working directory |
| Utility -> external client | OS process + existing ordered arguments | Preserve login/password/input/output protocol order; do not log credentials |
| Utility <-> external client | Existing Windows named events | Preserve protocol names; coordinate concurrent runs |

## Suggested Build Order

1. **Defaults and override contract:** pure resolver/default inventory, console option binding, full-path semantics, dynamic client cleanup decision, isolated regression tests. This resolves the behavior both web response and child execution depend on.
2. **Separate authenticated API:** API project, validated operator configuration, API-key handler and controller with fake launcher. Authentication and launch-plan design can proceed in parallel once the shared contract is stable.
3. **Process integration and deployment:** real launcher, per-child overrides, working-directory/output packaging checks, immediate response and sanitized errors, concurrency policy, focused endpoint tests and documentation.

Research before implementation should settle named-event concurrency, renamed client compatibility, UNC/network identity requirements, and which nonsecret settings count as “all defaults.” Standard controller/authentication/DI and process-start plumbing need no separate architectural spike.

## Sources

- [S1: ASP.NET Core authentication overview](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/?view=aspnetcore-8.0) — authentication handlers, schemes, and authorization requirement.
- [S2: ProcessStartInfo.UseShellExecute](https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.processstartinfo.useshellexecute?view=netframework-4.8) — direct executable launch and working-directory semantics; applicability includes .NET 8.
- [S3: ProcessStartInfo](https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.processstartinfo?view=net-7.0) — `ArgumentList`, environment and start properties; consult .NET 8 reference during implementation, since indexed page includes later-version APIs not recommended here.
- [S4: Process.Close](https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.process.close?view=netframework-4.8) — handle cleanup and Dispose delegation; applicability includes .NET 8.
- [S5: Project references](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-reference-add) — SDK project-reference dependency mechanism.
- [S6: ASP.NET Core integration tests](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-8.0) — `WebApplicationFactory` and replacing dependencies for tests.
- Repository evidence: `.planning/PROJECT.md`, `.planning/codebase/ARCHITECTURE.md`, `Exchange/ExchangeProcess.cs`, `Exchange/Program.cs`, `Exchange/Exchange.csproj`; read 2026-10-27. No application executed and no deployment secret settings read.

---
*Architecture research for: Exchange Admin API*
*Researched: 2026-10-27*
