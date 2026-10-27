---
last_mapped_commit: 6a4a0e6426a45c1bc286ebdcf3bb27e0193469a3
last_mapped_at: 2026-10-27
---
# External Integrations

**Analysis Date:** 2026-10-27

## APIs & External Services

**External exchange client:**
- `ExchangeClient.exe` performs the gateway exchange; this repository orchestrates its lifecycle through `Exchange/ExchangeProcess.cs`.
  - SDK/Client: direct `System.Diagnostics.Process` invocation; no gateway HTTP SDK is declared in `Exchange/Exchange.csproj`.
  - Executable location: resolve `Exchange:AgentCatalog` plus the executable filename through `Exchange/ExchangeProcess.cs` and `Exchange/Options/ExchangeOptions.cs`.
  - Auth: `Exchange:Agent:Login` and `Exchange:Agent:Password` are model properties in `Exchange/Options/AgentOptions.cs`; use environment overrides `Exchange__Agent__Login` and `Exchange__Agent__Password` rather than recording credentials in documentation (`AGENTS.md`).
  - Argument contract: login, password, incoming directory, outgoing directory, in that order (`Exchange/ExchangeProcess.cs`). Preserve the order when modifying this integration.
  - Endpoint, remote protocol, remote authentication implementation, and gateway transport are owned by the external executable; they are not established by source in `Exchange/ExchangeProcess.cs`.

**Windows interprocess coordination:**
- Named events coordinate launch and exchange completion in `Exchange/ExchangeProcess.cs`.
  - `CloseExchng`: create/open a close-event handle; the current implementation closes its local handle during cleanup.
  - `OneTimeExchng`: create/open a manual-reset event and signal it to request one exchange.
  - `LoadEnd`: create/open an event, wait for client startup, then reset it.
  - `ExchngEnd`: open the client's existing event, reset before requesting exchange, wait for completion, then reset again.
  - Preserve the exact names in `Exchange/ExchangeProcess.cs`; they are external protocol identifiers.
  - Both startup and exchange waits use the model timeout in milliseconds (`Exchange/Options/AgentOptions.cs`, `Exchange/ExchangeProcess.cs`).

## Data Storage

**Databases:**
- None detected: `Exchange/Exchange.csproj` declares logging packages and `Exchange/ExchangeProcess.cs` delegates exchange to the external executable without a database client.

**File Storage:**
- Local filesystem directories are passed to the external client using `Exchange:InFilePath` and `Exchange:OutFilePath`, defined by `Exchange/Options/ExchangeOptions.cs` and consumed in `Exchange/ExchangeProcess.cs`.
- Local operational logs use NLog file targets declared by `Exchange/exchangeNlog.config`.
- Test-owned temporary directories isolate executable staging and synthetic configuration in `Tests/ProgramEndToEndTests.cs`.
- No object-storage SDK or file-transfer implementation is declared in `Exchange/Exchange.csproj`; the application itself does not enumerate or transform exchange payloads in `Exchange/ExchangeProcess.cs`.

**Caching:**
- No application cache or external cache provider is detected in `Exchange/ExchangeProcess.cs` or `Exchange/Exchange.csproj`.

## Authentication & Identity

**Auth Provider:**
- External client credentials, represented by the plain string model properties in `Exchange/Options/AgentOptions.cs`.
  - Implementation: `Exchange/Program.cs` binds configuration, then `Exchange/ExchangeProcess.cs` interpolates credentials into the external process arguments.
  - No application session, OAuth flow, API authentication middleware, or identity provider SDK is configured by `Exchange/Program.cs` or `Exchange/Exchange.csproj`.
  - Credentials are process arguments, so preserve the contract carefully and avoid logging arguments; `Exchange/ExchangeProcess.cs` does not emit the assembled argument string in its log calls.

## Monitoring & Observability

**Error Tracking:**
- No remote error-tracking SDK is declared in `Exchange/Exchange.csproj`.
- Exceptions are logged through NLog and printed to stderr by `Exchange/Program.cs`.
- Exit codes derive from numeric exception-message prefixes, with fallback `99`, in `Exchange/Program.cs`; integration exceptions use prefixed messages in `Exchange/ExchangeProcess.cs`.

**Logs:**
- NLog 6.0.4 and NLog.Web.AspNetCore 6.0.4 are declared in `Exchange/Exchange.csproj`.
- `Exchange/Program.cs` loads relative `exchangeNlog.config` before application configuration and shuts down NLog in `finally`.
- `Exchange/exchangeNlog.config` configures Windows ProgramData file logging, UTF-8 encoding, and daily archive behavior; deployment path values are deliberately not repeated here.
- Logging configuration contains rules referring to target names absent from its target declarations in `Exchange/exchangeNlog.config`; validate logging behavior when changing deployment or NLog versions (`AGENTS.md`).
- The isolated startup test supplies a null NLog target and therefore does not verify production file logging (`Tests/ProgramEndToEndTests.cs`).

## CI/CD & Deployment

**Hosting:**
- Windows console process with externally installed exchange client; the integration requires process-management permissions and access to named events, exchange directories, and log output (`Exchange/ExchangeProcess.cs`, `Exchange/exchangeNlog.config`).
- `Exchange/Exchange.csproj` copies runtime configuration files alongside build output; run with that output directory as the working directory (`Exchange/Program.cs`, `AGENTS.md`).
- No repository deployment automation or cloud-hosting configuration is detected alongside `AIPractice1.slnx`.

**CI Pipeline:**
- No tracked CI workflow detected; local validation commands are defined by `AGENTS.md` and `.agents/skills/run-and-fix-tests/SKILL.md`.
- Codex's post-edit hook invokes `dotnet format whitespace` for edited C# files in `.codex/hooks/format_after_edit.py`; its registration and the protected-file hook are in `.codex/config.toml`.
- Development hooks require active-session trust; configuration presence does not prove execution (`README.md`, `AGENTS.md`).

## Environment Configuration

**Required env vars:**
- No environment variable is unconditionally required by `Exchange/Program.cs`: environment values override the two JSON configuration sources rather than forming a separately validated mandatory-variable list.
- Deployment must supply usable values for `Exchange:AgentCatalog`, `Exchange:InFilePath`, `Exchange:OutFilePath`, and nested `Exchange:Agent` fields used by `Exchange/ExchangeProcess.cs`.
- Supported override names follow the provider convention: `Exchange__AgentCatalog`, `Exchange__InFilePath`, `Exchange__OutFilePath`, `Exchange__Agent__Timeout`, `Exchange__Agent__Login`, and `Exchange__Agent__Password` (`Exchange/Program.cs`, `Exchange/Options/ExchangeOptions.cs`, `Exchange/Options/AgentOptions.cs`, `AGENTS.md`).
- Configuration precedence: `embeddedsettings.json` → `appsettings.json` → environment variables (`Exchange/Program.cs`).
- No environment-specific JSON file or dotenv loader is added by `Exchange/Program.cs`.
- The .NET executable test optionally honors `DOTNET_HOST_PATH` to choose its host process (`Tests/ProgramEndToEndTests.cs`).

**Secrets location:**
- Application credentials enter through configuration binding and environment overrides (`Exchange/Program.cs`, `Exchange/Options/AgentOptions.cs`). Do not copy deployment settings values into codebase documents (`AGENTS.md`).
- A local root `.env` exists and is ignored by `.gitignore`; its contents are not inspected, and `Exchange/Program.cs` does not load it.
- Repository rules prohibit changing secret environment files; `.codex/config.toml` registers `.codex/hooks/protect_files.py` to check direct tool arguments (`AGENTS.md`).
- No dedicated secrets-manager SDK is declared in `Exchange/Exchange.csproj`.

## Webhooks & Callbacks

**Incoming:**
- No HTTP listeners or webhook endpoints are configured by `Exchange/Program.cs`.
- Windows named-event signals `LoadEnd` and `ExchngEnd` are the client-to-orchestrator completion notifications in `Exchange/ExchangeProcess.cs`.

**Outgoing:**
- No direct HTTP callback code is present in `Exchange/ExchangeProcess.cs`.
- The orchestrator launches the external executable and signals `OneTimeExchng`; remote network interactions are outside the source-level integration boundary (`Exchange/ExchangeProcess.cs`).
- Startup and cleanup call `StopClients()`, which enumerates processes and kills every matching `ExchangeClient` process (`Exchange/ExchangeProcess.cs`). Isolated tests must fail before that path; real exchange requires an explicitly authorized environment (`Tests/ProgramEndToEndTests.cs`, `AGENTS.md`).

---

*Integration audit: 2026-10-27*
