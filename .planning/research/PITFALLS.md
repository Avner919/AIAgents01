# Pitfalls Research

**Domain:** API-key-protected ASP.NET Core launcher for the Windows Exchange utility
**Researched:** 2026-10-27
**Confidence:** MEDIUM — official documentation cross-checked against checked repository source; obtained from `classify-confidence --provider websearch --verified`. Recommendations below are design judgments; external-client and deployment behavior remains untested.

## Critical Pitfalls

### Pitfall 1: Treating a PID as exchange success

**What goes wrong:** The API reports success even though the child immediately exits because configuration, logging, permissions, or client startup failed. Alternatively, the controller waits for exchange completion and violates the immediate-response requirement.

**Why it happens:** Two different processes start: the API creates `Exchange.exe`, then that utility starts the selected external client and waits for Windows events. A successful first process creation proves neither later operation.

**How to avoid:** Return a launch acknowledgement, preferably HTTP 202 with the utility PID and effective nonsecret settings. Catch direct OS launch failures separately and return sanitized problem details. Do not wait for exit or readiness. Document that later failures use the utility's existing diagnostics/exit-code behavior and are outside this response. Capture PID before releasing the process wrapper; handle a null result as failure. Do not add a fictitious status URL. [S1]

**Warning signs:** Response says “exchange completed”; controller calls `WaitForExit`; tests only cover a child that exits instantly; missing utility and missing external client are treated as the same HTTP failure.

**Phase to address:** API contract and process integration. Verify a fake still-running child produces an immediate PID response and a thrown start exception produces no success response.

### Pitfall 2: Inheriting the web application's working directory

**What goes wrong:** `Exchange.exe` exists and starts, but loads missing or incorrect JSON/NLog files. A client at an alternate path starts without its supporting files.

**Why it happens:** The console entry point uses relative configuration/logging paths. Developers confuse the utility deployment directory, external-client directory, and API content root. `WorkingDirectory` does not locate an executable when `UseShellExecute` is false. [S2]

**How to avoid:** Configure the utility's absolute executable path and its own deployment working directory; launch directly with `UseShellExecute = false`. Resolve the external client's working directory deliberately, normally its containing directory, preserving `AgentCatalog` for the default case. Test the start-info builder from an unrelated current directory. Validate deployment artifacts and access using the service identity before any authorized real exchange.

**Warning signs:** Works from an IDE only; `FileName = "Exchange.exe"`; response defaults are derived from the API's JSON; alternate client still receives the old catalog as working directory.

**Phase to address:** Defaults/override contract and deployment integration.

### Pitfall 3: Reporting defaults from a second source

**What goes wrong:** Blank requests return defaults copied into the API while the child executes different settings. Serializing all options accidentally reveals client credentials.

**Why it happens:** Class constants, configured settings, environment overrides, and per-request values are mistaken for the same thing. Current `ExchangeProcessFileName` is a private class constant, while catalog, directories and timeout come from configuration.

**How to avoid:** Keep defaults in `ExchangeProcess` and introduce a pure shared resolver/projection. Normalize omitted, null, empty and whitespace-only values identically. Define the inventory of “all defaults” explicitly: effective client path/name, working directory, relevant nonsecret directories and timeout with units; decide whether fixed event identifiers belong in the inventory. Use an allowlist response DTO. Ensure parent and child use the same configuration location and precedence. Put the selected value only in this child's start configuration, never a process-wide environment mutation.

**Warning signs:** API duplicates `ExchangeClient.exe`; options objects are returned directly; concurrent requests mutate environment state; defaults tests verify only `usedDefaults = true`.

**Phase to address:** First phase: shared default resolution and console override contract, before API response implementation.

### Pitfall 4: Changing the executable path while cleanup still targets the default name

**What goes wrong:** An alternate executable survives cleanup while unrelated `ExchangeClient` instances are killed. Merely deriving a different process name can kill unrelated programs with that name.

**Why it happens:** `StartClient()` and the `finally` block both call `StopClients()`, which enumerates all processes and matches the hardcoded default process name. Current startup performs that destructive step before checking the executable exists.

**How to avoid:** Treat client lifecycle ownership as part of override support. Retain the actual launched process and design cleanup around that owned instance. Decide explicitly how legacy pre-existing clients are handled; do not silently extend process-name killing to arbitrary names. Validate input before side effects. Make cleanup tolerate partial initialization and preserve the original failure rather than replace it with a cleanup exception.

**Warning signs:** Only `FileName` changes; name-based kill remains unquestioned; process handle is a local variable discarded after start; failure tests accidentally exercise `StopClients()`.

**Phase to address:** Console override support. Deeper phase research must establish the external client's single-instance behavior and required legacy startup handling.

### Pitfall 5: Exposing shared named-event coordination to concurrent HTTP calls

**What goes wrong:** A second utility can stop the first client's process, reset its events, or consume completion state intended for another exchange. Two requests can interfere even on an otherwise idle machine.

**Why it happens:** The current protocol uses fixed names `CloseExchng`, `OneTimeExchng`, `ExchngEnd`, and `LoadEnd`; HTTP creates an easy concurrent entry point. An in-memory API semaphore does not protect multiple web instances or direct console invocations.

**How to avoid:** Define a single-active-exchange policy before exposing the endpoint. Prefer a machine-visible gate in the coordinator before event/client management, with deterministic busy diagnostics. Preserve protocol identifiers unless the external client contract explicitly permits changes. Resolve the immediate-PID/busy-response tradeoff in phase design: a child may start and then exit busy after the API has acknowledged its PID. Do not promise a synchronous busy result without an admission protocol.

**Warning signs:** Concurrency is deferred as a scaling concern; only one-controller-instance locking is used; manual and API launches are assumed mutually exclusive.

**Phase to address:** Process integration and concurrency contract; requires deeper research before implementation.

### Pitfall 6: Authentication that silently permits launch

**What goes wrong:** Registering an API-key handler without endpoint authorization leaves the action callable anonymously. An absent configured key equals an absent supplied key and authorizes a request.

**Why it happens:** ASP.NET Core authentication registration does not itself restrict endpoints. [S3] Custom string comparisons often omit missing/blank configuration and duplicate-header cases.

**How to avoid:** Validate a nonblank configured secret at startup and fail closed. Apply explicit authorization to the admin controller or an authenticated fallback policy. Define a single API-key header, reject ambiguous header values, and verify failure never reaches the launcher. Use HTTPS and operator-provided secret configuration; keep actual keys out of repository files, defaults, URLs, diagnostics and child arguments. Full-path execution means authorized callers are trusted to select code executed under the service identity: use a deliberately limited Windows identity and document this trust boundary.

**Warning signs:** Only a valid-key happy-path test exists; production uses a sample key; launch logic runs before authentication; responses expose configuration objects.

**Phase to address:** Authenticated API foundation, before wiring the real launcher.

## Technical Debt Patterns

| Shortcut | Immediate Benefit | Long-term Cost | When Acceptable |
|----------|-------------------|----------------|-----------------|
| Controller calls `Process.Start` directly | Fewer types | Cannot isolate launch failures or prove unauthorized requests have no side effects | Avoid here; use one narrow injectable launcher |
| Clone class defaults into API settings | Quick response DTO | Default drift and child/parent mismatch | Never for class-owned defaults |
| Treat PID as durable job identity | No storage | PID reuse and absent completion history | Acceptable as a clearly documented launch acknowledgement only |
| Add full job queue/status subsystem | Familiar job architecture | Scope expansion and lifecycle burden | Defer until durable status is requested |

## Integration Gotchas

| Integration | Common Mistake | Correct Approach |
|-------------|----------------|------------------|
| API to utility | Caller chooses directly launched executable | Keep utility path operator-controlled; caller selects only external-client override |
| Utility to client | Concatenate quoted executable and arguments into a shell command | Direct executable launch; preserve existing positional argument order; use structured argument tokens if CLI transport is chosen |
| Service filesystem access | Desktop user's permissions or mapped drives assumed | Validate local/UNC policy and Windows service identity permissions; file existence is not execution/readiness proof |
| Console configuration | ASP.NET's standard environment-specific JSON behavior assumed | Preserve the console's actual JSON, JSON, environment order and explicit deployment base directory |
| Error handling | Raw exception returned to caller | Sanitized problem response for direct launch failures; protected operational diagnostics for details |

## Performance Traps

| Trap | Symptoms | Prevention | When It Breaks |
|------|----------|------------|----------------|
| Waiting for client readiness in controller | Requests approach configured exchange timeout | Return on utility process creation | First slow client startup |
| Unlimited concurrent utility creation | Interference, excess processes, event races | Explicit admission/concurrency policy | Two overlapping launches |
| Redirected child output without lifetime owner | Child stalls after response | Avoid redirection or give draining streams an independent lifetime owner | When redirected buffers fill |

## Security Mistakes

| Mistake | Risk | Prevention |
|---------|------|------------|
| Arbitrary full path treated as harmless data | Service identity executes caller-selected code | Require API key and least privilege; keep deployment writable only by trusted operators; decide UNC policy |
| Global environment mutation per request | Cross-request override leakage | Set only per-child `ProcessStartInfo` environment |
| API key inherited needlessly by utility | Larger secret exposure boundary | Review inherited environment and remove the API's secret variable from child settings where practical |
| Logging client command lines/default objects | Existing client credentials exposed | Safe response projection and credential-redacted diagnostics; never log full arguments |

## UX Pitfalls

| Pitfall | User Impact | Better Approach |
|---------|-------------|-----------------|
| “Successful exchange” for PID response | Caller assumes work completed | State utility started and exchange outcome is not returned |
| Defaults shown only in documentation | Caller cannot see effective selection | Return applied defaults/effective settings when blank |
| Timeout without units | Seconds mistaken for milliseconds | Name/document the millisecond unit |
| Relative or quoted path silently accepted | Wrong executable or confusing failure | Define fully qualified unquoted path input and useful validation errors |

## "Looks Done But Isn't" Checklist

- [ ] **Authentication:** Missing, incorrect, blank and ambiguous keys invoke the launcher zero times; missing configured secret cannot enable access.
- [ ] **Defaults:** Omitted/null/empty/whitespace values use the same `ExchangeProcess` resolver and expose only nonsecret settings.
- [ ] **Overrides:** Full paths with spaces remain intact; relative/malformed paths fail before process side effects; alternate client cleanup is defined.
- [ ] **Immediate result:** A still-running fake child returns its PID without waiting; start exceptions produce sanitized failure responses.
- [ ] **Isolation:** HTTP tests replace the launcher and test real authentication; console tests stop before external exchange and process-name cleanup. [S4]
- [ ] **Deployment:** Utility launch location, console configuration and child working directory are independently verified.
- [ ] **Concurrency:** Overlapping API/direct invocations have an explicit coordinator policy; API-local locking limitations are documented.
- [ ] **Validation:** Implementation follows the repository test skill and runs `dotnet test AIPractice1.slnx`; no successful startup test is described as real exchange validation.

## Recovery Strategies

| Pitfall | Recovery Cost | Recovery Steps |
|---------|---------------|----------------|
| Wrong working directory | LOW | Correct deployment working directory; verify expected artifact layout using an isolated substitute |
| Reported defaults differ | MEDIUM | Compare resolver inputs/configuration origins; consolidate projection; add parent/child contract regression coverage |
| Cleanup kills unrelated clients | HIGH | Disable further launches, review owned-process lifecycle and restore affected exchanges through authorized operations |
| Concurrent coordinators interfere | HIGH | Stop admitting overlapping work; establish machine-visible coordinator ownership before resuming |
| Secret leaked through response/logging | HIGH | Remove the exposure and coordinate operator key rotation; agents must not modify protected secret files |

## Pitfall-to-Phase Mapping

| Pitfall | Prevention Phase | Verification |
|---------|------------------|--------------|
| Default drift and disclosure | Shared defaults/console override | Pure resolver tests and allowlisted response assertions |
| Renamed-client lifecycle mismatch | Shared defaults/console override | Owned-process substitute tests; partial-start cleanup checks |
| Authentication bypass | Separate authenticated API | HTTP integration tests with real handler and fake launcher |
| PID confused with completion | Process integration | Still-running fake test plus direct launch-error contract test |
| Working directory/configuration mismatch | Process integration/deployment | Start-info assertions from unrelated current directory; publish artifact review |
| Fixed-event concurrency | Process integration/deployment | Defined cross-process admission policy; safe substitute-based overlap tests |

## Sources

- [S1: Microsoft Process.Start](https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.process.start?view=net-8.0) — process creation API and launch error behavior; later exchange outcomes are inferred from the repository's two-stage execution.
- [S2: Microsoft ProcessStartInfo.UseShellExecute](https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.processstartinfo.useshellexecute?view=net-8.0) — direct execution and working-directory behavior.
- [S3: ASP.NET Core authentication overview](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/?view=aspnetcore-8.0) — authentication versus endpoint access restrictions.
- [S4: ASP.NET Core integration tests](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-8.0) — test host and dependency substitution.
- Checked source: `Exchange/ExchangeProcess.cs` (`StartClient`, `StopClients`, event constants and cleanup), `Exchange/Program.cs` (relative configuration, diagnostics and exit codes), `.planning/PROJECT.md`, `.planning/research/ARCHITECTURE.md`; read 2026-10-27. No real exchange or secret configuration files accessed.

---
*Pitfalls research for: Exchange Admin API*
*Researched: 2026-10-27*
