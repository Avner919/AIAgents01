---
last_mapped_commit: 6a4a0e6426a45c1bc286ebdcf3bb27e0193469a3
last_mapped_at: 2026-10-27
---
# Codebase Structure

**Analysis Date:** 2026-10-27

## Directory Layout

```text
AIAgents01/
├── Exchange/                    # Console executable and configuration assets
│   ├── Options/                 # Configuration binding models
│   │   ├── ExchangeOptions.cs
│   │   └── AgentOptions.cs
│   ├── Program.cs               # Composition and exit-code boundary
│   ├── ExchangeProcess.cs       # Native process/event coordination
│   ├── Exchange.csproj          # .NET 8 executable and output-copy rules
│   ├── embeddedsettings.json    # Base configuration asset; values excluded
│   ├── appsettings.json         # Override configuration asset; values excluded
│   └── exchangeNlog.config      # NLog XML configuration
├── Tests/                       # xUnit project and process-level startup check
│   ├── Exchange.Tests.csproj
│   ├── ProgramEndToEndTests.cs
│   └── README.md
├── .codex/                      # Agent configuration, rules, Python hooks/tests
│   ├── config.toml
│   ├── rules/
│   └── hooks/
│       ├── protect_files.py
│       ├── format_after_edit.py
│       └── tests/
├── .agents/skills/               # Local validation and explicit PR-review skills
├── .planning/codebase/           # GSD codebase analysis documents
├── docs/                        # Agent/tooling documentation and images
├── .idea/                       # Local IDE metadata
├── .artifacts/                  # Ignored local artifacts
├── AIPractice1.slnx             # Solution containing application and tests
├── AGENTS.md                    # Repository operation/runtime guidance
├── README.md                    # Repository overview
├── .editorconfig                # C# style conventions
└── .gitignore                   # Generated output and local-file exclusions
```

## Directory Purposes

**`Exchange/`:**
- Purpose: Own the complete production application.
- Contains: Console entry point, exchange coordinator, options, deployment configuration assets, and generated `bin/`/`obj/` output.
- Key files: `Exchange/Program.cs`, `Exchange/ExchangeProcess.cs`, `Exchange/Exchange.csproj`.

**`Exchange/Options/`:**
- Purpose: Represent the bound `Exchange` configuration section.
- Contains: Plain mutable C# options classes, with nested agent settings.
- Key files: `Exchange/Options/ExchangeOptions.cs`, `Exchange/Options/AgentOptions.cs`.

**`Tests/`:**
- Purpose: Keep .NET tests outside the executable project.
- Contains: xUnit source, project configuration, and usage instructions.
- Key files: `Tests/Exchange.Tests.csproj`, `Tests/ProgramEndToEndTests.cs`, `Tests/README.md`.

**`.codex/`:**
- Purpose: Configure local agent operations and automated edit checks.
- Contains: Hook registrations, command rules, test guidance, Python hook implementations, and Python tests.
- Key files: `.codex/config.toml`, `.codex/rules/permissions.rules`, `.codex/rules/unit-tests.md`, `.codex/hooks/protect_files.py`, `.codex/hooks/format_after_edit.py`.

**`.agents/skills/`:**
- Purpose: Supply repository-specific agent procedures.
- Contains: Skill documents and `agents/openai.yaml` metadata for validation and requested PR reviews.
- Key files: `.agents/skills/run-and-fix-tests/SKILL.md`, `.agents/skills/review-pull-request/SKILL.md`.

**`docs/`:**
- Purpose: Provide supporting repository/tooling documentation.
- Contains: Markdown and image assets.
- Key files: `docs/about.md`, `docs/img1.png`, `docs/img2.png`.

## Key File Locations

**Entry Points:**
- `Exchange/Program.cs`: Production `Main` entry point and exception-to-exit-code mapping.
- `Tests/ProgramEndToEndTests.cs`: xUnit process-based startup failure scenario.
- `.codex/hooks/protect_files.py`: Pre-tool hook executable.
- `.codex/hooks/format_after_edit.py`: Post-edit hook executable.

**Configuration:**
- `AIPractice1.slnx`: Solution membership and test project grouping.
- `Exchange/Exchange.csproj`: Target framework, logging packages, output-copy rules.
- `Tests/Exchange.Tests.csproj`: Test dependencies and application assembly staging.
- `Exchange/embeddedsettings.json`, `Exchange/appsettings.json`: Runtime configuration assets referenced by the entry point; settings values are outside this map.
- `Exchange/exchangeNlog.config`: Runtime log routing configuration.
- `.editorconfig`: Four-space C# indentation, Allman braces, system-first imports, UTF-8 and CRLF conventions.
- `.codex/config.toml`: Approval/sandbox configuration and hook registrations.
- `.codex/rules/permissions.rules`: Command-prefix operation policies.
- `AGENTS.md`: Applicable operating constraints and runtime precautions.

**Core Logic:**
- `Exchange/ExchangeProcess.cs`: Client executable lookup, argument order, named native events, timeout waits, cleanup.
- `Exchange/Options/ExchangeOptions.cs`: Top-level section name and file/client paths.
- `Exchange/Options/AgentOptions.cs`: Agent timeout and authentication property definitions.

**Testing:**
- `Tests/ProgramEndToEndTests.cs`: Unique temporary-directory fixture and actual executable launch with invalid JSON.
- `Tests/Exchange.Tests.csproj`: `CopyExchangeApplication` target stages assemblies and runtime metadata into the test output's `ExchangeApp` subdirectory.
- `.codex/hooks/tests/test_format_after_edit.py`: Formatter hook tests.
- `.codex/hooks/tests/test_protect_files.py`: Protected-file hook tests.
- `.codex/rules/unit-tests.md`: Required .NET test naming, isolation, placement, and validation procedure.

## Naming Conventions

**Files:**
- Use PascalCase C# type names and matching filenames: `Exchange/ExchangeProcess.cs`, `Exchange/Options/AgentOptions.cs`.
- Name .NET test classes/files `<Subject>Tests` and place them under `Tests/`, as in `Tests/ProgramEndToEndTests.cs` (`.codex/rules/unit-tests.md`).
- Use snake_case Python script names and `test_*.py` tests within `.codex/hooks/` and `.codex/hooks/tests/`.
- Use uppercase analysis filenames in `.planning/codebase/`, such as `.planning/codebase/ARCHITECTURE.md`.

**Directories:**
- Match application namespace roles in PascalCase: `Exchange/Options/` corresponds to `Exchange.Options` (`Exchange/Options/ExchangeOptions.cs`).
- Keep tests in `Tests/` using `Exchange.Tests`; group by tested feature in subdirectories as needed (`.codex/rules/unit-tests.md`).
- Keep agent tooling under `.codex/` and local procedures under `.agents/skills/` (`.codex/config.toml`, `AGENTS.md`).

## Where to Add New Code

**New Feature:**
- Primary code: `Exchange/`; wire new orchestration dependencies explicitly from `Exchange/Program.cs`.
- Lifecycle behavior: `Exchange/ExchangeProcess.cs`; preserve the native event identifiers and client argument sequence (`AGENTS.md`).
- Configuration contracts: `Exchange/Options/`; keep section/property names aligned with `Exchange/Program.cs` binding.
- Tests: `Tests/`, with test-owned inputs and no real client process management (`.codex/rules/unit-tests.md`).

**New Component/Module:**
- Implementation: Add a named C# type within `Exchange/` or a focused subdirectory under it; no separate service/repository library is present (`Exchange/Exchange.csproj`).
- Shared options: Add an options class under `Exchange/Options/` and connect it through the existing bound model (`Exchange/Options/ExchangeOptions.cs`).
- Developer hook: `.codex/hooks/`, with regression tests in `.codex/hooks/tests/` and registration in `.codex/config.toml`.

**Utilities:**
- Shared application helpers: Keep them near their production consumer under `Exchange/`; no shared `Utilities/` folder exists (`Exchange/Program.cs`, `Exchange/ExchangeProcess.cs`).
- Test helpers: Keep them in `Tests/`, separate from executable source and deployment settings (`.codex/rules/unit-tests.md`).
- Documentation: Extend `docs/` for supporting material and `.planning/codebase/` for GSD reference documents.

**Validation and repository constraints:**
- Run solution tests from the root after code changes; apply `.agents/skills/run-and-fix-tests/SKILL.md` (`AGENTS.md`).
- PR review procedure applies only to explicit review requests; it is separate from automatic test repair (`.agents/skills/review-pull-request/SKILL.md`).
- Do not rename or move existing files/directories, and preserve unrelated IDE or working-tree edits (`AGENTS.md`).

## Special Directories

**`Exchange/bin/`, `Exchange/obj/`, `Tests/bin/`, `Tests/obj/`:**
- Purpose: Build/restore products and test executable staging (`Exchange/Exchange.csproj`, `Tests/Exchange.Tests.csproj`).
- Generated: Yes.
- Committed: No; excluded by `.gitignore`.

**`.artifacts/`:**
- Purpose: Local task artifacts; contents are not application source (`.gitignore`).
- Generated: Local tooling-dependent.
- Committed: No; excluded by `.gitignore`.

**`.idea/`:**
- Purpose: Present IDE metadata, outside runtime/build logic (`.idea/`).
- Generated: IDE-managed.
- Committed: Some project metadata is tracked, including `.idea/.idea.AIPractice1/.idea/vcs.xml`; preserve unrelated local IDE edits (`AGENTS.md`).

**`.codex/` and `.agents/`:**
- Purpose: Agent configuration, procedures, hooks, and hook tests (`.codex/config.toml`, `.agents/skills/run-and-fix-tests/SKILL.md`).
- Generated: No; maintained repository tooling.
- Committed: Yes; tracked files are present.

**`.planning/codebase/`:**
- Purpose: Codebase reference consumed by GSD planning/execution; `.planning/codebase/ARCHITECTURE.md` and `.planning/codebase/STRUCTURE.md` cover architecture and placement.
- Generated: Yes, through codebase mapping.
- Committed: Mapping outputs are intended for repository tracking by the orchestrator.

---

*Structure analysis: 2026-10-27*
