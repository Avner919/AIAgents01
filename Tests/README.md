# Tests

Run from the repository root:

```powershell
dotnet test AIPractice1.slnx
```

Alternatively, use `dotnet test Tests/Exchange.Tests.csproj` with an SDK that does not support `.slnx`.

The xUnit end-to-end test launches the actual Exchange executable with malformed configuration in a unique temporary directory. It checks exit code 99 and configuration diagnostics on stderr. It supplies its own settings and logging configuration, so no external exchange client is started or stopped. The child process has a 30-second deadline and temporary files are cleaned up.

Successful exchange with the external Windows client requires a separately authorized integration environment.
