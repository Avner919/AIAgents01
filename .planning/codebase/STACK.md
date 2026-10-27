---
last_mapped_commit: 6a4a0e6426a45c1bc286ebdcf3bb27e0193469a3
last_mapped_at: 2026-10-27
---
# Technology Stack

**Analysis Date:** 2026-10-27

## Languages

**Primary:**
- C# targeting .NET 8: console entry point, configuration models, and Windows client orchestration in `Exchange/Program.cs`, `Exchange/Options/`, and `Exchange/ExchangeProcess.cs`.
- C# 12 syntax is present through the primary constructor in `Exchange/ExchangeProcess.cs`; no explicit `LangVersion` override is declared in `Exchange/Exchange.csproj`.

**Secondary:**
- Python: Codex hook implementations and standard-library unit tests in `.codex/hooks/` and `.codex/hooks/tests/`; the formatter launcher explicitly selects Python 3.12 in `.codex/config.toml`.
- XML: MSBuild projects, solution membership, and NLog configuration in `Exchange/Exchange.csproj`, `Tests/Exchange.Tests.csproj`, `AIPractice1.slnx`, and `Exchange/exchangeNlog.config`.
- JSON/TOML: application configuration filenames referenced by `Exchange/Program.cs` and development-agent configuration in `.codex/config.toml`. Deployment settings values are intentionally excluded from this map.

## Runtime

**Environment:**
- .NET 8 application and test target (`net8.0`) in `Exchange/Exchange.csproj` and `Tests/Exchange.Tests.csproj`.
- Windows is required for the actual exchange integration: named operating-system events and an external `.exe` are used by `Exchange/ExchangeProcess.cs`.
- The isolated startup-failure test invokes the managed executable through the .NET host in `Tests/ProgramEndToEndTests.cs`.

**Package Manager:**
- NuGet through SDK/MSBuild `PackageReference` declarations in `Exchange/Exchange.csproj` and `Tests/Exchange.Tests.csproj`; package-manager version follows the selected .NET SDK.
- Lockfile: no tracked `packages.lock.json` or other dependency lockfile detected; package versions are explicitly declared in the two project files.
- SDK pin: no tracked `global.json` detected; `AGENTS.md` identifies an installed .NET 10 SDK for `.slnx` support.

## Frameworks

**Core:**
- .NET SDK console application (`Microsoft.NET.Sdk`, `OutputType` executable), configured in `Exchange/Exchange.csproj`.
- Microsoft.Extensions.Configuration JSON/environment providers and configuration binding are used in `Exchange/Program.cs`; these libraries are not separately version-pinned as direct references in `Exchange/Exchange.csproj`.
- No ASP.NET application host, web routing, background-service host, or dependency-injection container is created by `Exchange/Program.cs`.

**Testing:**
- xUnit 2.9.3 with `xunit.runner.visualstudio` 3.1.5 in `Tests/Exchange.Tests.csproj`.
- Microsoft.NET.Test.Sdk 17.14.1 in `Tests/Exchange.Tests.csproj`.
- Python `unittest` for development hooks in `.codex/hooks/tests/test_format_after_edit.py` and `.codex/hooks/tests/test_protect_files.py`.
- No coverage collector dependency or coverage threshold is declared in `Tests/Exchange.Tests.csproj`.

**Build/Dev:**
- SDK/MSBuild builds the two projects referenced by `AIPractice1.slnx`.
- `dotnet format` consumes `.editorconfig`; the formatter hook invokes whitespace formatting for edited C# files in `.codex/hooks/format_after_edit.py`.
- Codex project configuration and command rules live in `.codex/config.toml` and `.codex/rules/permissions.rules`.

## Key Dependencies

**Critical:**
- NLog 6.0.4: operational and exception logging in `Exchange/Program.cs` and `Exchange/ExchangeProcess.cs`; declared in `Exchange/Exchange.csproj`.
- NLog.Web.AspNetCore 6.0.4: `NLogBuilder.ConfigureNLog` setup and additional layout renderers in `Exchange/Program.cs` and `Exchange/exchangeNlog.config`; declared in `Exchange/Exchange.csproj`. Its presence does not make this a web application.
- External `ExchangeClient.exe`: process and named-event protocol required by `Exchange/ExchangeProcess.cs`; the client is not a NuGet dependency or source project in `AIPractice1.slnx`.

**Infrastructure:**
- `System.Diagnostics.Process`: launches and terminates clients in `Exchange/ExchangeProcess.cs` and hosts the executable under test in `Tests/ProgramEndToEndTests.cs`.
- `System.Threading.EventWaitHandle`: coordinates external client startup and exchange completion in `Exchange/ExchangeProcess.cs`.
- `System.IO`: resolves the external client and test staging paths in `Exchange/ExchangeProcess.cs` and `Tests/ProgramEndToEndTests.cs`.
- Python standard library (`json`, `pathlib`, `subprocess`, `re`) implements the formatter hook without a Python package manifest in `.codex/hooks/format_after_edit.py`.

## Configuration

**Environment:**
- Configuration precedence is `embeddedsettings.json`, then `appsettings.json`, then process environment variables, as explicitly built by `Exchange/Program.cs`.
- Bind the `Exchange` section to `ExchangeOptions` and nested `AgentOptions` in `Exchange/Options/ExchangeOptions.cs` and `Exchange/Options/AgentOptions.cs`.
- Environment overrides use double underscores, for example `Exchange__Agent__Timeout`, per `AGENTS.md`; `Exchange/Program.cs` does not load `.env` files.
- `Exchange/Options/ExchangeOptions.cs` defines incoming/outgoing directories and client location; `Exchange/Options/AgentOptions.cs` defines timeout and client authentication fields. Timeout is consumed as milliseconds by `Exchange/ExchangeProcess.cs`.
- `appsettings.Development.json` is not loaded by `Exchange/Program.cs`; do not rely on environment-specific automatic JSON discovery.
- A root `.env` exists and is excluded by `.gitignore`; its contents are not inspected. Settings and secret values must not be copied into this document (`AGENTS.md`).

**Build:**
- `Exchange/Exchange.csproj` copies `exchangeNlog.config`, `appsettings.json`, and `embeddedsettings.json` to the output using `PreserveNewest`.
- `Tests/Exchange.Tests.csproj` stages built application DLLs and runtime metadata into `ExchangeApp` after build, excluding deployment settings.
- `.editorconfig` sets four-space C# indentation, Allman braces, CRLF, system-first using organization, and async naming rules.
- No tracked container build file, shared `Directory.Build.props`/`Directory.Build.targets`, or CI build definition is detected alongside `AIPractice1.slnx`.

## Platform Requirements

**Development:**
- Use an SDK supporting `net8.0`; solution commands also require `.slnx` support, as explained by `AGENTS.md` and reflected in `AIPractice1.slnx`.
- Restore/build with `dotnet restore Exchange/Exchange.csproj` and `dotnet build Exchange/Exchange.csproj --no-restore` from the repository root (`AGENTS.md`).
- Run `dotnet test AIPractice1.slnx` before completing source-code changes; the automatic repair procedure is `.agents/skills/run-and-fix-tests/SKILL.md`.
- Python is required for the configured development hooks; formatting specifically uses `py -3.12`, while the protection hook uses `python` (`.codex/config.toml`). Hooks require trust in the active Codex session (`README.md`).

**Production:**
- Deploy as a Windows console utility with .NET 8 runtime support, external client installation, configured exchange directories, and logging-directory access (`Exchange/Exchange.csproj`, `Exchange/ExchangeProcess.cs`, `Exchange/exchangeNlog.config`).
- Use the build output as the working directory because logging and application configuration are opened through relative paths in `Exchange/Program.cs`.
- Run actual exchange only in an authorized environment: startup and cleanup terminate all processes matching the client name (`Exchange/ExchangeProcess.cs`, `AGENTS.md`).
- No repository-defined cloud hosting, container platform, or CI/CD deployment target is detected in the tracked files associated with `AIPractice1.slnx`.

---

*Stack analysis: 2026-10-27*
