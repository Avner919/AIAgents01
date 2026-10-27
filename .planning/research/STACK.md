# Stack Research

**Domain:** Windows exchange utility with a separate authenticated process-launch API
**Researched:** 2026-10-27
**Confidence:** MEDIUM — official Microsoft documentation opened and cross-checked against repository sources; confidence returned by `classify-confidence --provider websearch --verified`.

## Recommended Stack

### Core Technologies

| Technology | Version | Purpose | Why Recommended |
|------------|---------|---------|-----------------|
| C# / ASP.NET Core controller application | `net8.0`, serviced 8.0.x runtime | Separate admin API using `Microsoft.NET.Sdk.Web`, controllers, DI and JSON request/response DTOs | Matches the current console application's target and the requested controller architecture; avoids unrelated migration during implementation. See support deadline below. |
| ASP.NET Core authentication and authorization | Shared framework 8.0.x | Custom API-key authentication scheme and authorization on the launch controller | Uses framework challenge/authorization behavior and supports pipeline integration tests; no additional authentication framework is required for this single administrative credential. |
| `System.Diagnostics.Process` / `ProcessStartInfo` | .NET 8 BCL | Launch fixed server-configured `Exchange.exe`, capture its PID | Direct OS process launch meets immediate acknowledgement semantics without introducing a queue or scheduler. |
| Existing `ExchangeProcess` class | Repository source | Own default process values and effective client-path resolution | User explicitly requires this ownership. Expose a side-effect-free defaults/resolution API rather than copying constants into the web app. |
| Windows | Supported host OS for selected runtime and client | Host both processes and existing named-event integration | The existing client protocol is Windows-specific; Linux hosting is outside this milestone. |

**Support decision required before production:** Microsoft's support policy, updated October 25, 2026, lists .NET 8 end of support as **November 10, 2026**, and .NET 10 LTS end of support as **November 14, 2028**. Keeping `net8.0` is an implementation-compatibility choice, not a durable deployment recommendation. Schedule an explicit .NET 10 upgrade decision; do not silently retarget the existing utility in this feature. Use the current serviced patch of the chosen runtime rather than assuming installed SDK version establishes production support. [Microsoft support policy](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core)

### Supporting Libraries

| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| `Microsoft.Extensions.Options` and configuration | ASP.NET Core shared framework 8.0.x | Typed API-key and utility launch configuration; validate required settings at startup | Keep server-selected utility executable location distinct from caller-selected external-client path. Obtain keys through deployment configuration; never return them in defaults. |
| `System.Text.Json` | Shared framework 8.0.x | Request and response serialization | Standard controller DTOs; no Newtonsoft.Json requirement identified. |
| `Microsoft.AspNetCore.Mvc.Testing` | Latest serviced 8.0.x compatible with selected target | `WebApplicationFactory` HTTP pipeline tests | Authenticate requests and replace a launcher abstraction with a fake; never initiate real exchange in ordinary tests. |
| Existing xUnit | 2.9.3, runner 3.1.5 | Unit and integration assertions | Retain current test infrastructure instead of introducing a second test framework. |
| Existing NLog | 6.0.4 in utility | Preserve existing utility logging | New API can use built-in `ILogger<T>` initially. Expanding NLog hosting integration is optional; do not alter current logging deployment casually. |

### Development Tools

| Tool | Purpose | Notes |
|------|---------|-------|
| Installed .NET SDK supporting `.slnx` | Solution build/test | Repository guidance identifies .NET 10 SDK. SDK and application target/runtime are different choices. |
| `dotnet test AIPractice1.slnx` | Required solution validation after implementation | Follow repository test skill; use isolated substitutes and retain existing invalid-configuration process test. |
| `dotnet format` | Established formatting | Match existing block namespaces, Allman braces, UTF-8 and CRLF conventions. |

## Installation

Implementation commands below are illustrative, not executed during research. Select an unused API project name; no existing files need renaming.

```powershell
dotnet new webapi --use-controllers --framework net8.0 --name Exchange.AdminApi
dotnet sln AIPractice1.slnx add Exchange.AdminApi/Exchange.AdminApi.csproj
# Add Microsoft.AspNetCore.Mvc.Testing to the chosen API test project,
# pinning an exact serviced 8.0.x version at implementation time.
dotnet test AIPractice1.slnx
```

Authentication, controllers, configuration, JSON and process launch use the shared framework/BCL. Do not add API-key, process-runner, database or job-system packages without a requirement. Template-added OpenAPI tooling is optional; documenting an API-key header must not embed a real key.

## Alternatives Considered

| Recommended | Alternative | When to Use Alternative |
|-------------|-------------|-------------------------|
| Scoped `net8.0` implementation | .NET 10 LTS | Prefer a separately approved upgrade for supported production operation after the imminent .NET 8 deadline; validate utility dependencies and deployment runtime together. |
| Custom framework authentication handler plus authorization | Third-party API-key package | Reconsider when multiple keys, rotation administration, identities or per-client policies become requirements. |
| Controller endpoint and direct launch service | Minimal API | Valid for small APIs generally, but the user explicitly requested an admin controller. |
| `ProcessStartInfo` | Background job framework / message queue | Appropriate only if durable scheduling, retries, cross-host execution or completion tracking becomes required. |
| Read-only `ExchangeProcess` defaults/resolver accessed via project reference | Child process metadata mode | Consider metadata mode only if project referencing the console assembly proves unsuitable. It adds startup/protocol complexity and must never run exchange while describing defaults. |

## What NOT to Use

| Avoid | Why | Use Instead |
|-------|-----|-------------|
| Shell (`cmd.exe`, PowerShell) wrapping the launch | Adds command parsing around a user-selected executable path | Fixed `Exchange.exe` launch with `UseShellExecute=false`; one argument per `ArgumentList` entry if using a CLI override. |
| String-concatenated `Arguments` for the new override transport | Spaces and quoting can change argument boundaries | `ArgumentList`, or a per-child environment override explicitly read by the console. Never mutate host-wide environment per request. |
| `Task.Run` around the entire exchange / `WaitForExit` in HTTP handler | Changes immediate launch acknowledgement into work/completion management | Return PID after successful `Process.Start`; report launch failure separately from later exchange failure. |
| Defaults duplicated in API configuration or DTO initializers | Drifts from class-owned behavior | Resolve effective non-secret values through `ExchangeProcess`; preserve current config precedence. |
| API-key query-string parameters or returned configuration dumps | Spreads credential material through URLs or responses | Header-based credential input and explicitly selected non-secret response fields. |

## Stack Patterns by Variant

**If the optional external-client path is blank:**
- Normalize null, empty and whitespace consistently.
- Obtain the default executable filename and resolution behavior from `ExchangeProcess`; combine with effective agent catalog there.
- Return the effective non-secret defaults actually used. Class constants are not all configuration defaults: timeout and directories originate from configuration and need consistent resolution, not copies of deployment settings in the API.

**If a full external-client executable path is supplied:**
- Validate an absolute executable path and pass it as a value to `Exchange.exe`; do not make the caller select the API's own launch target.
- Keep the utility's working directory at its deployment output/configuration directory. The client's working directory is a separate integration decision.
- Read the returned PID as an OS launch acknowledgement, not a success verdict for exchange or client startup.

**If the API is hosted under IIS or a Windows service:**
- Verify account permissions, Windows session behavior, named-event accessibility and child lifecycle in the intended environment.
- Host recycling and immediate child startup failure remain deployment concerns; research does not establish a detached-process durability guarantee.

## Version Compatibility

| Package A | Compatible With | Notes |
|-----------|-----------------|-------|
| New API targeting `net8.0` | Existing `Exchange` targeting `net8.0` | Keep defaults/resolution methods pure; referencing an executable assembly does not require invoking its entry point. Verify project-reference build and publish output during implementation. |
| `Microsoft.AspNetCore.Mvc.Testing` 8.0.x | ASP.NET Core API 8.0.x | Match major framework version; pin exact package version when implementing. |
| .NET 10 SDK | Repository `.slnx` and existing `net8.0` target | Does not by itself install/prove the correct production ASP.NET Core runtime. |

## Sources

All external evidence below was opened on 2026-10-27. Recommendations are engineering synthesis, not claims that Microsoft prescribes this exact application design. Confidence is MEDIUM under the workflow's provider classifier.

- [Microsoft .NET support policy](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core) — support deadlines; policy updated October 25, 2026.
- [ASP.NET Core authentication overview, 8.0](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/?view=aspnetcore-8.0) — registered authentication schemes, handlers, authorization distinction and middleware ordering.
- [ASP.NET Core integration tests, 8.0](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-8.0) — test host, `WebApplicationFactory`, dependency replacement and test isolation.
- [ProcessStartInfo.ArgumentList](https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.processstartinfo.argumentlist?view=net-10.0) — argument escaping and exclusivity with `Arguments`. The 8.0 URL failed extraction; confirm target compilation during implementation.
- [Options pattern](https://learn.microsoft.com/en-us/dotnet/core/extensions/options) — typed configuration and validation.
- Repository evidence: `.planning/PROJECT.md`, `.planning/codebase/STACK.md`, `Exchange/ExchangeProcess.cs`, `Exchange/Exchange.csproj` — existing target, dependency versions, private default constants, relative paths and current launch behavior; no deployment settings values read.

**Research gaps:** exact serviced NuGet patch selection, final API host, API-key header/configuration names, launcher transport, and the complete set of user-facing non-secret defaults must be settled in phase planning. No real process was launched and no package compatibility build was performed during research.

---
*Stack research for: Exchange Admin API*
*Researched: 2026-10-27*
