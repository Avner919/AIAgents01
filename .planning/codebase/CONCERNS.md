---
last_mapped_commit: 6a4a0e6426a45c1bc286ebdcf3bb27e0193469a3
last_mapped_at: 2026-10-27
---
# Codebase Concerns

**Analysis Date:** 2026-10-27

## Tech Debt

**Configuration validation:**
- Evidence: `Exchange/Program.cs` binds options directly and constructs the coordinator without checking the section, nested agent, paths, or timeout range.
- Files: `Exchange/Options/ExchangeOptions.cs`, `Exchange/Options/AgentOptions.cs`.
- Impact: Missing or invalid values can fail after named events are acquired or existing clients are stopped.
- Fix approach: Validate configuration before entering orchestration; add isolated tests for missing sections and invalid timeout values.

## Known Bugs

**Cleanup after partial initialization:**
- Evidence: Dictionary assignment occurs only after its initializer finishes, while `CloseEventHandlers` unconditionally dereferences the field in `Exchange/ExchangeProcess.cs`.
- Trigger: A named-event acquisition throws during registration.
- Impact: Cleanup can mask the original exception; its failure also skips the subsequent `StopClients` call.
- Fix approach: Track handles as they are acquired, make disposal null-safe, and preserve the original error independently of cleanup errors.

**Incomplete handle disposal:**
- Evidence: `CloseEventHandlers` closes only `CloseExchng` and `OneTimeExchng` in `Exchange/ExchangeProcess.cs`.
- Impact: `LoadEnd` and acquired `ExchngEnd` handles have no matching explicit disposal; process exit eventually reclaims them.
- Fix approach: Dispose every acquired handle exactly once, including partial-registration cases.

**Client argument boundaries:**
- Evidence: `StartClient` interpolates four values into one unquoted argument string in `Exchange/ExchangeProcess.cs`.
- Trigger: A credential or input/output path contains whitespace or quoting characters.
- Impact: The client can receive incorrect arguments.
- Fix approach: Verify `ProcessStartInfo.ArgumentList` compatibility with the external client while preserving argument order.

## Security Considerations

**Credentials on the child command line:**
- Evidence: `Exchange/ExchangeProcess.cs` passes authentication fields as process arguments.
- Risk: Tools with appropriate OS access may inspect the command line.
- Recommendation: Coordinate a safer credential transport with the external client; this changes the integration contract and requires separate planning.

**Shared process termination:**
- Evidence: `StopClients` kills every matching process by name in `Exchange/ExchangeProcess.cs`, both before launch and during cleanup.
- Risk: Other exchanges in the same environment can be interrupted.
- Current mitigation: `AGENTS.md` requires explicit authorization for real execution.
- Recommendation: Establish process ownership or an exclusive execution boundary before supporting concurrent runs.

**Hook coverage limits:**
- Evidence: `.codex/hooks/protect_files.py` inspects direct tool input; `.codex/config.toml` registers the hook.
- Risk: Indirect scripts and computed paths are outside this input guard's coverage; trust must also be established by the host.
- Recommendation: Preserve repository restrictions and treat hook checks as one layer, not filesystem enforcement (`AGENTS.md`, `README.md`).

## Performance Bottlenecks

- `Exchange/ExchangeProcess.cs` performs blocking readiness and completion waits, each with the configured timeout. This is a lifecycle constraint rather than a measured performance defect.
- No benchmark or production timing evidence was collected; avoid assigning throughput figures without measurements (`Tests/ProgramEndToEndTests.cs`).

## Fragile Areas

**Named-event protocol:**
- Files: `Exchange/ExchangeProcess.cs`.
- Risk: Fixed event names and manual resets share external state; simultaneous runs or stale signals can interfere.
- Safe modification: Preserve protocol names and sequencing; validate with an authorized Windows client environment.
- Coverage: `Tests/ProgramEndToEndTests.cs` exits during configuration loading and does not cover this lifecycle.

**Logging startup boundary:**
- Evidence: NLog initialization occurs before the main try/finally in `Exchange/Program.cs`.
- Risk: Initialization errors bypass the normal stderr, exit-code mapping, and shutdown boundary.
- Safe modification: Test malformed or missing logging configuration in an isolated process before changing the boundary.

## Scaling Limits

- `Exchange/ExchangeProcess.cs` has shared event identifiers and broad process-name cleanup; independent concurrent exchanges are not isolated.
- No measured capacity is available. Establish ownership and event isolation with client compatibility tests before expanding concurrency.

## Dependencies at Risk

- `Exchange/Exchange.csproj` targets `net8.0` and references NLog packages. No dependency vulnerability or support-lifecycle audit was performed.
- Changes to logging packages require validation of `Exchange/exchangeNlog.config` under `AGENTS.md`; do not infer compatibility from package restore alone.

## Missing Critical Features

- No dedicated options validation boundary exists in `Exchange/Program.cs` and the options models.
- No process/event adapter interfaces exist in `Exchange/ExchangeProcess.cs`, limiting safe deterministic lifecycle tests.
- `AGENTS.md` reports no CI workflow; local checks remain the documented validation path.

## Test Coverage Gaps

- High: Client startup, completion, timeout, event disposal, and cleanup error paths in `Exchange/ExchangeProcess.cs` are outside the configuration-failure test.
- High: Argument handling and shared-client termination require isolated substitutes before adding automated coverage (`Tests/ProgramEndToEndTests.cs`, `.codex/rules/unit-tests.md`).
- Medium: Logging initialization failure and exit-code prefix edge cases lack dedicated coverage in `Exchange/Program.cs`.
- Python hook tests run separately from `AIPractice1.slnx`; see `.codex/hooks/tests/` and `.planning/codebase/TESTING.md`.
- This map records source evidence only. Tests and real exchange were not run during mapping.

---

*Concerns audit: 2026-10-27*
