# Tests

Run from the repository root:

```powershell
dotnet test AIPractice1.slnx
```

Alternatively, use `dotnet test Tests/Exchange.Tests.csproj` with an SDK that does not support `.slnx`.

The xUnit end-to-end test launches the actual Exchange executable with malformed configuration in a unique temporary directory. It checks exit code 99 and configuration diagnostics on stderr. It supplies its own settings and logging configuration, so no external exchange client is started or stopped. The child process has a 30-second deadline and temporary files are cleaned up.

Successful exchange with the external Windows client requires a separately authorized integration environment.

The admin API tests use a real loopback HTTP server and synthetic configuration. Authentication tests substitute a counting launcher and assert that rejected requests launch nothing. The default-launch tracer and launcher test start only `HarmlessUtility.exe`, staged under its original name with no deployment settings. A named pipe reports safe settings and the owned PID, and the test verifies the HTTP response arrives before releasing the child. All waits and owned-process cleanup are bounded. Tests never use installed-client names for cleanup or change the host's global environment/working directory.

Four pure `ExchangeClientStartInfoTests` cases inspect ordered `ArgumentList` values, including paths/credentials containing spaces. They do not start a process or construct exchange orchestration. See [the operator guide](../docs/exchange-api.md) for configuration and request/response examples.
