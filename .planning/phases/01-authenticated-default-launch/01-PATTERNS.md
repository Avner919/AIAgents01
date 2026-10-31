# Phase 01: Authenticated Default Launch - Pattern Map

**Mapped:** 2026-10-31
**Files classified:** 24 proposed created/modified files
**Analogs found:** 17 / 24 (including explicitly limited partial matches)

## Scope and Evidence

Read the phase context and research, repository guidance, unit-test rules, both project skill indexes, and canonical project/requirements/roadmap documents. The file names below expand the research responsibility table into a concrete proposed inventory. `ApiApplication.cs`, `IUtilityProcessLauncher.cs`, `LaunchResponse.cs`, and `appsettings.example.json` are provisional names for responsibilities the research leaves unnamed. They are planning choices, not approved user contract names.

All existing analog paths below were verified with `git ls-files -- <paths>` from the repository root. No installed mirrors, settings values, protected environment files, or deployment credentials were read. Source observations take precedence over older codebase maps. No executable, exchange, or tests were run for this mapping.

## File Classification

| New/Modified File | Role | Data Flow | Closest Analog | Match Quality |
|---|---|---|---|---|
| `Exchange.Api/Exchange.Api.csproj` | config | transform | `Exchange/Exchange.csproj` | role-match; Web SDK is new |
| `Exchange.Api/Program.cs` | config | request-response | `Exchange/Program.cs` | partial; entry-point layout only |
| `Exchange.Api/ApiApplication.cs` | provider | request-response | None | none; injectable web composition is new |
| `Exchange.Api/Controllers/ExchangeController.cs` | controller | request-response | None | none |
| `Exchange.Api/Authentication/ApiKeyAuthenticationHandler.cs` | middleware | request-response | None | none |
| `Exchange.Api/Options/ApiOptions.cs` | model | transform | `Exchange/Options/ExchangeOptions.cs` | role-match; no startup validation analog |
| `Exchange.Api/Services/UtilityProcessLauncher.cs` | service | event-driven | `Tests/ProgramEndToEndTests.cs` | partial; direct child creation only |
| `Exchange.Api/Services/IUtilityProcessLauncher.cs` | service | event-driven | None | none; no service interface exists |
| `Exchange.Api/Contracts/LaunchRequest.cs` | model | request-response | None | none; configuration models are not HTTP contracts |
| `Exchange.Api/Contracts/LaunchResponse.cs` | model | request-response | None | none; no public safe projection exists |
| `Exchange/Configuration/ExchangeConfigurationLoader.cs` | utility | file-I/O, transform | `Exchange/Program.cs` | exact provider sequence; explicit base/env injection is new |
| `Exchange/ResolvedExchangeSettings.cs` | model | transform | `Exchange/Options/ExchangeOptions.cs` | partial; immutable snapshot is new |
| `Exchange/ExchangeProcess.cs` | service | event-driven | `Exchange/ExchangeProcess.cs` | exact existing integration site |
| `Exchange/Program.cs` | config | file-I/O, event-driven | `Exchange/Program.cs` | exact existing integration site |
| `AIPractice1.slnx` | config | transform | `AIPractice1.slnx` | exact existing project registry |
| `Tests/Exchange.Tests.csproj` | config | file-I/O, transform | `Tests/Exchange.Tests.csproj` | exact staging/reference site |
| `Tests/ExchangeApiTests.cs` | test | request-response | `Tests/ProgramEndToEndTests.cs` | partial; test conventions only, HTTP fixture new |
| `Tests/ExchangeConfigurationTests.cs` | test | file-I/O, transform | `Tests/ProgramEndToEndTests.cs` | role-match; synthetic files and cleanup |
| `Tests/UtilityProcessLauncherTests.cs` | test | event-driven | `Tests/ProgramEndToEndTests.cs` | role-match; owned child and bounded waits |
| `Tests/Fixtures/HarmlessUtility/HarmlessUtility.csproj` | config | transform | `Exchange/Exchange.csproj` | role-match; executable project only |
| `Tests/Fixtures/HarmlessUtility/Program.cs` | utility | event-driven, file-I/O | `Tests/ProgramEndToEndTests.cs` | partial; ownership discipline, helper behavior new |
| `Exchange.Api/appsettings.example.json` | config | file-I/O | None | none; do not copy secret-bearing utility JSON |
| `docs/exchange-api.md` | config (documentation) | transform | `Tests/README.md` | role-match; concise commands and execution limits |
| `Tests/README.md` | config (documentation) | transform | `Tests/README.md` | exact existing documentation site |

Coverage: 6 exact, 6 role-match, 5 partial, 7 without analog. Documentation uses the nearest available role classification; it is not runtime configuration.

## Pattern Assignments

### `Exchange/Configuration/ExchangeConfigurationLoader.cs` and `Exchange/Program.cs`

**Analog:** `Exchange/Program.cs` (61 lines).

Imports/layout, lines 1-10:

```csharp
using System;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using NLog;
using NLog.Web;
using Exchange.Options;

namespace Exchange
{
    public static class Program
```

Core configuration composition, lines 21-27:

```csharp
var config = new ConfigurationBuilder().AddJsonFile("embeddedsettings.json")
                                       .AddJsonFile("appsettings.json")
                                       .AddEnvironmentVariables()
                                       .Build();

var exchangeOptions = config.GetSection(ExchangeOptions.SectionName)
                            .Get<ExchangeOptions>();
```

Copy provider order and section binding into a pure shared loader. Add an explicit utility base directory and injected environment snapshot without changing process-global cwd/environment. The API host's configuration remains distinct. The console and API must consume this same loader and the same class-owned resolver; no API-side duplicated defaults.

Existing console lifecycle, lines 29-33:

```csharp
var exchangeProcess = new ExchangeProcess(logger, exchangeOptions);

exchangeProcess.StartExchangeProcess();

return 0;
```

Existing console error/cleanup convention, lines 35-46:

```csharp
catch (Exception exception)
{
    logger.Error(exception, LogMessage);

    Console.Error.WriteLine(LogMessage + exception);

    return GetExitCode(exception.Message);
}
finally
{
    LogManager.Shutdown();
}
```

`GetExitCode` at lines 49-58 extracts a numeric prefix and otherwise returns 99. Preserve console failure semantics and the existing malformed-configuration regression. Do not reuse raw exception output as API responses or logs. Utility logging is initialized before the console try block at lines 16-17; a shared loader must not initialize logging or execute orchestration.

### `Exchange/ExchangeProcess.cs`

**Analog:** itself (223 lines); default ownership and integration sites.

Class-owned constants, lines 12-17:

```csharp
public class ExchangeProcess(Logger logger, ExchangeOptions exchangeOptions)
{
    // Имя процесса
    private const string ExchangeProcessName = "ExchangeClient";
    // Имя файла 
    private const string ExchangeProcessFileName = ExchangeProcessName + ".exe";
```

Path selection and existing validation, lines 89-94:

```csharp
//Получаем полный путь до утилиты
var processPath = Path.Combine(exchangeOptions.AgentCatalog, ExchangeProcessFileName);
if (!File.Exists(processPath))
{
    throw new FileNotFoundException($"1: Утилита обмена {ExchangeProcessFileName} не найдена в каталоге {exchangeOptions.AgentCatalog}.");
}
```

Extract pure resolution here, preserving constant ownership. Normalize default input with one blank rule covering omitted/null/empty/whitespace. Resolve configured relative paths against utility cwd consistently; existing `Path.Combine` alone is not an absolute-path canonicalizer. The same resolved filename/path/client cwd/directories/timeout must feed client start and response projection.

Existing process construction, lines 98-108:

```csharp
var clientProcess = new Process
{
    StartInfo = new ProcessStartInfo
    {
        FileName = processPath,
        WorkingDirectory = exchangeOptions.AgentCatalog,
        Arguments = $"{exchangeOptions.Agent.Login} {exchangeOptions.Agent.Password} {exchangeOptions.InFilePath} {exchangeOptions.OutFilePath}",
    }
};

clientProcess.Start();
```

Preserve external client argument order and named events `CloseExchng`, `OneTimeExchng`, `ExchngEnd`, `LoadEnd` (lines 19-27). This client launcher is distinct from the API's utility launcher. Keep credentials out of utility command arguments, API DTOs and diagnostics; this existing external-client argument contract remains unchanged in Phase 1.

Side-effect boundary: `StartClient` calls `StopClients` at line 87; orchestration finally calls it again at line 62. `StopClients` at lines 170-179 enumerates all processes, filters `ExchangeClient`, and kills matches. Event registration at lines 73-79 uses shared names; waits at lines 115 and 153 use the configured timeout. Never call these paths for defaults inspection, startup preflight or routine API tests. Do not copy waits/cleanup into the HTTP request, or infer independent exchanges from overlapping request admission.

### `Exchange.Api/Options/ApiOptions.cs` and `Exchange/ResolvedExchangeSettings.cs`

**Analog:** `Exchange/Options/ExchangeOptions.cs` (33 lines), supported by `AgentOptions.cs` (23 lines).

Model convention, lines 1-11:

```csharp
namespace Exchange.Options
{
    /// <summary>
    ///     Модель настроек из файла конфигурации.
    /// </summary>
    public class ExchangeOptions
    {
        /// <summary>
        ///     Имя секции файла конфигурации.
        /// </summary>
        public const string SectionName = "Exchange";
```

Existing option members, individual declaration lines 16, 21, 26 and 31:

```csharp
public string OutFilePath { get; set; }
public string InFilePath { get; set; }
public string AgentCatalog { get; set; }
public AgentOptions Agent { get; set; }
```

`AgentOptions.cs:11,16,21` declares `int Timeout`, `string Login`, `string Password`. Copy naming, namespace and section-constant conventions for API options; add API-owned key and fixed utility path only. No existing validator, eager startup-validation registration or immutable type exists. Design those using research recommendations. A resolved internal model may need sensitive fields in memory; never serialize it. A response requires an explicit six-field allowlist. Do not reuse mutable options as that response or as a shared cross-request snapshot.

### `Exchange.Api/Services/UtilityProcessLauncher.cs` and `IUtilityProcessLauncher.cs`

**Closest partial analog:** `Tests/ProgramEndToEndTests.cs:35-48` for direct controlled process construction (full excerpt below). `ExchangeProcess` launches the external client and has incompatible waits/process-name cleanup.

Use an absolute operator-configured utility target, `UseShellExecute=false`, utility deployment cwd, child-specific environment, and a fresh start-info/snapshot per request. Capture PID after successful creation, dispose the local wrapper without terminating the child, return immediately, and map immediate failures to safe categories. Do not copy redirected pipes or `WaitForExitAsync` into the production launcher. No interface/DI, sanitized process-error type, PID acknowledgement, or snapshot pinning implementation exists to copy; use RESEARCH.md for these new pieces.

### `Tests/ExchangeApiTests.cs`, `Tests/ExchangeConfigurationTests.cs`, `Tests/UtilityProcessLauncherTests.cs`

**Analog:** `Tests/ProgramEndToEndTests.cs` (77 lines).

Imports/test layout, lines 1-14:

```csharp
using System.Diagnostics;

using Xunit;

namespace Exchange.Tests
{
    public class ProgramEndToEndTests
    {
        [Fact]
        public async Task InvalidConfiguration_ReportsErrorAndReturnsFailureAsync()
        {
            var applicationDirectory = Path.Combine(AppContext.BaseDirectory, "ExchangeApp");
            var testDirectory = Path.Combine(Path.GetTempPath(), "Exchange.Tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(testDirectory);
```

Synthetic configuration, lines 23-25:

```csharp
await File.WriteAllTextAsync(Path.Combine(testDirectory, "embeddedsettings.json"), "{}");
// Fail before ExchangeProcess can start or stop external client processes.
await File.WriteAllTextAsync(Path.Combine(testDirectory, "appsettings.json"), "{ invalid json }");
```

Controlled child construction, lines 35-48:

```csharp
using var process = new Process
{
    StartInfo = new ProcessStartInfo
    {
        FileName = Environment.GetEnvironmentVariable("DOTNET_HOST_PATH") ?? "dotnet",
        WorkingDirectory = testDirectory,
        UseShellExecute = false,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        CreateNoWindow = true
    }
};
process.StartInfo.ArgumentList.Add(Path.Combine(testDirectory, "Exchange.dll"));
Assert.True(process.Start(), "The Exchange executable could not be started.");
```

Bounded test-owned lifetime, lines 49-62:

```csharp
var standardOutput = process.StandardOutput.ReadToEndAsync();
var standardError = process.StandardError.ReadToEndAsync();
using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));

try
{
    await process.WaitForExitAsync(timeout.Token);
}
catch (OperationCanceledException)
{
    process.Kill(entireProcessTree: true);
    await process.WaitForExitAsync();
    throw new TimeoutException("Exchange did not exit within 30 seconds after invalid configuration.");
}
```

Behavior assertions, lines 64-69:

```csharp
var error = await standardError;
var output = await standardOutput;
Assert.True(process.ExitCode == 99,
    $"Expected exit code 99; got {process.ExitCode}. stderr: {error}; stdout: {output}");
Assert.Contains("appsettings.json", error);
Assert.Contains("InvalidDataException", error);
```

`finally` at lines 71-74 deletes the unique test directory. Reuse this ownership/cleanup discipline, not the real Exchange execution, in new API tests. The existing test is deliberately safe because malformed JSON prevents orchestration.

HTTP-specific setup is new: use research's loopback Kestrel/port-zero fixture and real authentication, replace only launcher/config sources for most cases. Include one actual production-launcher-to-harmless-helper tracer. Keep helper alive until test release to prove prompt acknowledgement and overlap without fragile elapsed-time thresholds. Check every blank input/body variant, safe six-field projection, pinned snapshot equality, generic unauthorized responses (including malformed JSON), startup refusal and sanitized launch failures. Never change global cwd/environment or kill processes by name.

### `Tests/Fixtures/HarmlessUtility/Program.cs` and `HarmlessUtility.csproj`

**Partial source analogy:** owned child/files in the preceding test excerpt; executable target in `Exchange/Exchange.csproj:1-7` below. There is no helper protocol implementation to copy.

Implement synthetic safe observations and bounded readiness/release using test-owned IPC/files. No ExchangeProcess invocation, process enumeration, named exchange events, user config, secrets, or arbitrary sleep-based synchronization. The helper requires a Windows apphost output for tracing the production `.exe` launcher; stage dependencies beside it.

### Project registration and output staging

**Applies to:** `Exchange.Api/Exchange.Api.csproj`, helper project, `Tests/Exchange.Tests.csproj`, `AIPractice1.slnx`.

Executable project properties, `Exchange/Exchange.csproj:1-7`:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <AssemblyName>$(MSBuildProjectName)</AssemblyName>
  </PropertyGroup>
```

Reuse target `net8.0`; the API needs the new `Microsoft.NET.Sdk.Web` SDK, while the package-free helper retains console SDK/output. Utility asset-copy declarations are at `Exchange/Exchange.csproj:13-15,22-29`; they cover `exchangeNlog.config`, `appsettings.json`, `embeddedsettings.json`. Preserve those assets; do not read/copy their production values into examples or fixtures. Verify publish assets explicitly during execution rather than assuming output-copy metadata proves publication.

Reference/staging pattern, `Tests/Exchange.Tests.csproj:16-24`:

```xml
    <ProjectReference Include="../Exchange/Exchange.csproj" />
  </ItemGroup>
  <!-- Stage the real executable without deployment settings or credentials. -->
  <Target Name="CopyExchangeApplication" AfterTargets="Build">
    <ItemGroup>
      <ExchangeApplicationFiles Include="$(MSBuildProjectDirectory)/../Exchange/bin/$(Configuration)/net8.0/*.dll;$(MSBuildProjectDirectory)/../Exchange/bin/$(Configuration)/net8.0/Exchange.deps.json;$(MSBuildProjectDirectory)/../Exchange/bin/$(Configuration)/net8.0/Exchange.runtimeconfig.json" />
    </ItemGroup>
    <Copy SourceFiles="@(ExchangeApplicationFiles)" DestinationFolder="$(OutDir)ExchangeApp" SkipUnchangedFiles="true" />
  </Target>
```

Add API/helper project references and distinct helper staging. The existing target copies DLL/deps/runtimeconfig files, not an `.exe`; production-launcher tracer staging must include helper apphost and required dependencies. **Nested-project compile boundary:** a helper under `Tests/Fixtures/` falls under the parent SDK project's recursive C# glob. Explicitly exclude helper source/generated files from parent test compilation (e.g. `Compile Remove="Fixtures/HarmlessUtility/**/*.cs"`) and reference/build the helper as its own project. No current nested helper exists to supply an exact exclusion analog.

Solution registry, `AIPractice1.slnx:1-6`:

```xml
<Solution>
  <Project Path="Exchange/Exchange.csproj" />
  <Folder Name="/Tests/">
    <Project Path="Tests/Exchange.Tests.csproj" />
  </Folder>
</Solution>
```

Add API and helper entries using this project-path structure; no existing paths move. Retain existing xUnit/test SDK references (`Tests/Exchange.Tests.csproj:9-15`); research recommends no new package.

### Web boundary files with no implementation analog

**Applies to:** `Exchange.Api/Program.cs`, `ApiApplication.cs`, `Controllers/ExchangeController.cs`, `Authentication/ApiKeyAuthenticationHandler.cs`, `Contracts/LaunchRequest.cs`, `Contracts/LaunchResponse.cs`.

`Exchange/Program.cs` supplies only entry-point/import style for the web Program. The tracked application has no Web SDK project, DI registration, middleware pipeline, controller, authentication handler, authorization policy, JSON response DTO or HTTP test host. Use RESEARCH.md's official framework references and proposed responsibilities; do not claim console orchestration is a web pattern.

Plan explicit authentication/authorization before controller body binding, fail-closed key validation at host startup, one generic missing/invalid-key 401, and zero resolver/launch calls on failure. Protect the route, not only a post-binding controller branch. Configure null/empty-body handling deliberately. HTTP route/DTO/status/configuration names remain planning choices; `X-Api-Key` is locked by context.

### `Exchange.Api/appsettings.example.json`, `docs/exchange-api.md`, `Tests/README.md`

**Documentation analog:** `Tests/README.md:3-13`, with repository-root command fences and clear execution limits:

````markdown
Run from the repository root:

```powershell
dotnet test AIPractice1.slnx
```
````

Its final paragraph (line 13) is:

> Successful exchange with the external Windows client requires a separately authorized integration environment.

Extend documentation with manual Windows API startup, separately published utility path/cwd, JSON plus environment configuration, blank key placeholder, authenticated default request/response, safe error examples, immediate-PID semantics and admitted-overlap limitations. API example configuration has no safe existing JSON analog: write blank key values and synthetic paths from scratch. No protected environment file updates; no real settings values. Update test README to distinguish HTTP/fake-launcher checks from harmless real-launcher tracer and the existing malformed-config regression.

## Shared Patterns

### Configuration and defaults

Copy provider sequence from `Exchange/Program.cs:21-27`; constants remain in `ExchangeProcess.cs:15-17`. The snapshot must bind once from utility-folder JSON and captured environment, pin all consumed leaves in child-only environment, and project only safe fields. Current option leaves are `OutFilePath`, `InFilePath`, `AgentCatalog`, `Agent.Timeout`, `Agent.Login`, `Agent.Password`; do not omit sensitive consumed leaves from the internal handoff simply because they cannot appear in the response. Environment pinning does not rescue a later missing/malformed required JSON file; PID acknowledgement does not establish child startup success.

### Style and imports

Use block namespaces, four-space indentation, Allman braces, UTF-8/CRLF, System-first using groups and async `Async` suffixes. Sources show uneven legacy using order/tabs; follow `.editorconfig:20-25,30-35,77-83,154-163,177-178` for new code rather than copying incidental formatting defects.

### Authentication and validation

No existing guard or validation wrapper exists. Add framework-based handler/policy and eager options/deployment validation per research. Do not instantiate orchestration or configure utility NLog during API startup checks. Neither option class has existing validation attributes. Treat mutable option types as internal configuration contracts, not safe wire models.

### Errors and logging

Console raw exceptions/exit prefixes are compatibility patterns only. API responses/logs require a new sanitized category/allowlist boundary; never copy raw `exception.Message`/`ToString()` serialization. Preserve existing Russian console messages where touching console behavior. Synthetic sentinel assertions should cover responses and captured logs without real secrets.

### Test isolation and integration

Reuse unique temporary directory, synthetic JSON, owned handles, cancellation and finally cleanup from `Tests/ProgramEndToEndTests.cs:13-14,23-33,35-74`. Use child-specific dictionaries; never process-global mutations. Existing tests do not establish HTTP/authentication, immediate return or overlapping admission. Implementation must run focused behavior checks and the complete `dotnet test AIPractice1.slnx` under the project test skill; this documentation-only mapping ran none.

## No Analog Found

| File | Role | Data Flow | Reason / Planner Reference |
|---|---|---|---|
| `Exchange.Api/ApiApplication.cs` | provider | request-response | No injectable web-host composition; use research host/test strategy |
| `Exchange.Api/Controllers/ExchangeController.cs` | controller | request-response | No HTTP routes/controllers; use research framework references |
| `Exchange.Api/Authentication/ApiKeyAuthenticationHandler.cs` | middleware | request-response | No authentication/authorization implementation |
| `Exchange.Api/Services/IUtilityProcessLauncher.cs` | service | event-driven | No service interface or DI pattern |
| `Exchange.Api/Contracts/LaunchRequest.cs` | model | request-response | No nullable HTTP/body-binding contract |
| `Exchange.Api/Contracts/LaunchResponse.cs` | model | request-response | No safe serialized response; options include sensitive fields |
| `Exchange.Api/appsettings.example.json` | config | file-I/O | No existing API configuration; utility JSON intentionally unread |

Partial matches also require new patterns for immutable snapshots, sanitized launch errors, PID acknowledgement, HTTP tests, helper coordination, environment handoff and nested-project exclusions.

## Metadata

**Analog search scope:** complete tracked-file inventory; application `Exchange/`, tests `Tests/`, root solution/editor configuration, canonical phase/project documents and local skill indexes.
**Implementation/configuration/test/doc files read:** 10 (`Exchange/Program.cs`, `Exchange/ExchangeProcess.cs`, both option classes, utility project, process test, test project, solution, test README, editorconfig). Five strong pattern families were sufficient: configuration entry point, process/default integration, options, isolated process tests, project/staging integration.
**Tracked-source gate:** all 10 paths returned by `git ls-files`; no ignored mirror analogs.
**Pattern extraction date:** 2026-10-31.
**Validation:** documentation-only mapping; no build, tests, actual exchange, external process cleanup, source edits or renames.
