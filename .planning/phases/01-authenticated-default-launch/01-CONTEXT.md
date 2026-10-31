# Phase 1: Authenticated Default Launch - Context

**Gathered:** 2026-10-31
**Status:** Ready for planning

<domain>
## Phase Boundary

Deliver a separate ASP.NET Core admin API that launches the configured `Exchange.exe` with existing client defaults and immediately returns its PID and agreed non-secret effective settings. Include API-key authentication, overlapping request admission, useful immediate launch errors, isolated verification, and usable configuration examples.

Phase 1 requirements: API-01, AUTH-01, AUTH-02, EXEC-01, EXEC-03, EXEC-04, EXEC-05, DEF-01, DEF-02, ERR-01. Explicit external-client selection and its lifecycle compatibility belong to Phase 2.
</domain>

<decisions>
## Implementation Decisions

### API-key operation
- **D-01:** Callers send the API key in the `X-Api-Key` header.
- **D-02:** Refuse API startup if the configured server key is missing or blank.
- **D-03:** Resolve the server key through standard ASP.NET Core configuration sources, including deployment-managed JSON and environment overrides. Do not restrict deployment to environment variables alone. Never commit key values; protected environment files remain user-maintained.
- **D-04:** Missing and invalid caller keys receive the same generic `401 Unauthorized` response. Never echo the supplied key. Authentication failure causes zero process launches.

### Deployment and configuration
- **D-05:** Initially run the API manually as a console application on Windows. Windows Service and IIS hosting are not requested for this phase.
- **D-06:** Configure the full path to a separately published `Exchange.exe`. The API does not assume the utility is deployed alongside it. This utility target is distinct from the optional external-client path planned for Phase 2.
- **D-07:** Read effective exchange settings from the utility deployment folder, following its existing configuration order: `embeddedsettings.json`, `appsettings.json`, then inherited environment overrides. Use the utility folder as the child working directory. API resolution and child execution must agree; do not maintain separate API exchange settings or default definitions.
- **D-08:** Refuse API startup if the configured utility executable or required configuration files are missing. Report the deployment problem without exposing secrets. This validation must not execute exchange or invoke client cleanup.

### Carried-forward requirements
- **D-09:** Default definitions remain in `ExchangeProcess`. Omitted, null, empty, and whitespace-only executable input all select existing defaults.
- **D-10:** Blank-input responses report effective client filename, full executable path, client working directory, input/output directories, and timeout in milliseconds, excluding credentials and API keys. Client working directory is distinct from the utility working directory.
- **D-11:** Return the utility PID after process creation without waiting for exchange completion. A PID is a launch acknowledgement, not evidence of successful exchange.
- **D-12:** Allow overlapping requests without rejecting, queueing, or serializing solely because another exchange is running. Shared named events and broad process cleanup remain concurrency concerns; independent successful external exchanges are not guaranteed.
- **D-13:** Use isolated substitutes and test-owned processes for verification. Do not run real exchange or terminate installed clients. Preserve named-event identifiers and external client argument order unless separately approved.

### Open choices for planning
The user selected API-key operation and deployment/configuration for discussion. Endpoint naming, request/response shape, configuration key names, and detailed launch-error formatting were not selected or decided. Resolve these during planning consistently with the approved requirements; do not describe them as user-approved choices. No explicit "you decide" answer was selected.
</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Scope and constraints
- `.planning/ROADMAP.md` — approved Phase 1 boundary, success criteria, verification, and Phase 2 dependency.
- `.planning/REQUIREMENTS.md` — approved requirements, acceptance criteria, and exclusions.
- `.planning/PROJECT.md` — project-level decisions and compatibility constraints.
- `AGENTS.md` — repository operation, secret protection, execution, and validation rules.
- `.codex/rules/unit-tests.md` — test placement and isolation conventions.
- `.agents/skills/run-and-fix-tests/SKILL.md` — required solution validation procedure after code changes.

### Existing integration
- `Exchange/Program.cs` — actual configuration precedence, relative paths, and console entry point.
- `Exchange/ExchangeProcess.cs` — class-owned defaults, client lifecycle, and Windows protocol.
- `Exchange/Options/ExchangeOptions.cs` — exchange configuration contract.
- `Exchange/Options/AgentOptions.cs` — agent configuration contract and millisecond timeout.
- `Exchange/Exchange.csproj` — target framework and copied deployment assets.
- `Tests/ProgramEndToEndTests.cs` — existing isolated child-process test pattern.

No external specifications or ADRs were referenced during this discussion.
</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `ExchangeProcess` owns executable defaults; shared resolution must retain their ownership and avoid executing orchestration merely to inspect settings.
- `ExchangeOptions` and `AgentOptions` provide existing configuration models.
- The existing process test demonstrates isolated temporary deployment and controlled executable startup.

### Established Patterns
- The console loads configuration relative to its working directory and applies environment overrides last. Do not assume it loads development JSON or dotenv files.
- Existing C# style uses four-space indentation, Allman braces, block-scoped namespaces, and CRLF.
- Launch and cleanup currently match client processes by name; fixed named events are shared across utilities. Overlap admission does not establish protocol isolation.

### Integration Points
- A separate web application adds HTTP/authentication behavior and starts the console utility as a separate OS process.
- Default resolution and safe disclosure connect to the existing exchange configuration and class-owned defaults.
- The configured utility working directory connects API launch behavior to console JSON and NLog assets. Validate deployment without triggering real exchange.
</code_context>

<specifics>
## Specific Ideas

- Concrete caller authentication header: `X-Api-Key`.
- Initially start and stop the API manually; point it at an independently published utility folder.
- Operator-facing settings must describe what that child will use, including inherited environment overrides.
</specifics>

<deferred>
## Deferred Ideas

None introduced during discussion. Explicit client selection remains in Phase 2 as already planned. Endpoint naming, response shape, and error formatting are open planning choices within Phase 1, not deferred capabilities.
</deferred>

---

*Phase: 01-authenticated-default-launch*
*Context gathered: 2026-10-31*
