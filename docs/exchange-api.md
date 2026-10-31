# Exchange Admin API

Publish the utility and API from the same revision into separate folders, from the repository root:

```powershell
dotnet publish Exchange/Exchange.csproj -c Release -o .artifacts/publish/utility
dotnet publish Exchange.Api/Exchange.Api.csproj -c Release -o .artifacts/publish/api
```

The utility folder must contain `Exchange.exe`, its DLL/deps/runtimeconfig and dependencies, `embeddedsettings.json`, `appsettings.json`, and `exchangeNlog.config`. Configure the external client and exchange directories there. Publishing uses the repository's existing utility configuration assets; the operator maintains deployment credentials. Do not commit deployment secrets or print their contents.

The API runs manually as a Windows console application. Use `Exchange.Api/appsettings.example.json` as the shape for deployment-managed API configuration. Supply a real key manually through deployment configuration or `AdminApi__ApiKey` in the launching process environment. Set `AdminApi__UtilityPath` to the full separately published `Exchange.exe` path. Environment variables override JSON. No agent edits to protected environment files are needed; the application does not load dotenv files.

For a local console deployment, run from the published API folder:

```powershell
.\Exchange.Api.exe --urls http://127.0.0.1:5080
```

Stop the API with Ctrl+C. Configure HTTPS before sending API keys across a remote network. The process identity needs read/execute access to the utility/client, access to exchange folders, Windows events and the existing log location. Startup refuses a missing/blank server key, invalid utility target, or missing/invalid required utility configuration. Startup checks inspect configuration; they do not run exchange.

Open `http://127.0.0.1:5080/` for Swagger UI. The launch profile opens this root URL automatically. Swagger is available in both Development and Production; its OpenAPI document is at `/swagger/v1/swagger.json`. Use **Authorize** to supply `X-Api-Key`; the configured key is never embedded in the UI or document. Opening documentation does not launch the utility. Executing the authenticated POST does initiate the configured utility.

## Default launch

```http
POST /admin/exchanges HTTP/1.1
Host: 127.0.0.1:5080
X-Api-Key: <redacted>
Content-Type: application/json

{}
```

Success is HTTP 200 with this shape (paths and PID below are synthetic):

```json
{
  "processId": 12345,
  "defaults": {
    "clientFileName": "ExchangeClient.exe",
    "clientExecutablePath": "C:\\ExchangeUtility\\client\\ExchangeClient.exe",
    "clientWorkingDirectory": "C:\\ExchangeUtility\\client",
    "inputDirectory": "C:\\ExchangeUtility\\input",
    "outputDirectory": "C:\\ExchangeUtility\\output",
    "timeoutMilliseconds": 30000
  }
}
```

No body, JSON `null`, `{}`, and a null, empty or whitespace-only `exchangeProcessFileName` select the same defaults. Nonblank executable input returns HTTP 400 `unsupported_client_override` in this phase; explicit client selection belongs to Phase 2. The request never changes the configured utility target.

The PID acknowledges OS process creation immediately. It does not confirm readiness, completion or exchange success; later utility failure cannot change the launch response. Each accepted request creates a process. Overlapping launches are admitted, but existing fixed named events and broad client-process cleanup can interfere with concurrent real exchanges. Independent exchange isolation is not guaranteed. There is no queue, status/history endpoint or completion wait.

## Effective configuration and errors

Default definitions remain in `ExchangeProcess`. Settings load from the utility folder's required `embeddedsettings.json`, then `appsettings.json`, then inherited environment overrides. The utility process runs with that folder as its working directory. The client working directory is resolved separately from `Exchange:AgentCatalog`. Relative client and input/output paths resolve against the utility folder.

Input and output directories may remain blank. Empty, whitespace-only or null configured input/output values resolve to empty strings, are reported as blank, and are passed as two empty client arguments. The API does not replace them with a folder or create directories. The external client determines how it handles those empty arguments.

Each request captures settings once and pins the consumed values into that child's environment. The six response fields describe that snapshot. Files must remain present and parseable during child startup; pinning values does not repair missing or malformed files. Credentials are never response fields or API child command arguments.

Errors use sanitized ProblemDetails with a `code` field:

| HTTP | Code | Meaning |
|---|---|---|
| 401 | unauthorized | Missing or invalid caller key; identical generic response |
| 400 | invalid_request | Invalid JSON or request shape |
| 400 | unsupported_client_override | Nonblank client selection is unavailable in Phase 1 |
| 503 | utility_configuration_unavailable | Required deployment/configuration became unavailable |
| 500 | utility_start_failed | Immediate OS process creation failed |

Errors do not echo keys, credentials, request values, raw parser errors or process-start exceptions. Correct deployment issues and retry explicitly. A returned PID remains a successful creation acknowledgement even if the child exits quickly.

## Verification

Run `dotnet test AIPractice1.slnx` from the repository root. New tests use synthetic configuration and harmless test-owned executables with bounded readiness/release and owned-PID cleanup. Never run a successful real exchange as a routine smoke test: startup and cleanup can terminate installed clients. Real exchange requires a separately authorized environment.
