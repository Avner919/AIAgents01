---
name: run-and-fix-tests
description: Run all AIAgents01 tests and repair failures after code changes, when tests fail, or when asked to validate this repository. Use automatically before completing code changes in this project.
---

# Run all tests and fix failures

Apply only to `C:\Projects\GitHub\AIAgents01` and its subdirectories. Work from the repository root, identified by `AIPractice1.slnx`, rather than assuming the shell starts there.

Read the root `AGENTS.md` and `.codex/rules/unit-tests.md`. Tests live in `Tests/Exchange.Tests.csproj`; application code lives in `Exchange/`.

## Procedure

1. Inspect the working tree and current changes. Preserve unrelated user edits. Establish the intended behavior from the task and existing contracts before changing assertions or application code.
2. Run the entire solution, allowing restore and build:

   ```powershell
   dotnet test AIPractice1.slnx --logger "console;verbosity=normal"
   ```

3. If a command fails, distinguish restore, build, SDK, permission, and external-service problems from an actual failing test. Follow the environment's permission flow when needed; do not change security settings or repository-wide dependencies merely to force a green run. If an external blocker remains, report it and stop the repair loop.
4. For actual failures, inspect the failing test, production path, exception, and expected behavior. Reproduce the smallest relevant case, for example:

   ```powershell
   dotnet test Tests/Exchange.Tests.csproj --filter "FullyQualifiedName~Exchange.Tests.ProgramEndToEndTests"
   ```

5. Fix the demonstrated cause with a focused change. Repair production code when behavior is wrong; change a test only when its expectation conflicts with an established requirement or its setup is defective. Add a regression assertion when it protects the intended behavior. Do not delete, skip, exclude, or weaken a failing test just to make the suite pass.
6. Re-run the affected tests after each repair. Continue while each attempt has a new evidence-backed hypothesis. If the same failure repeats without a new hypothesis, or repair requires a product decision or unrelated refactor, report the unresolved cause rather than making speculative changes.
7. After the last code change, run `dotnet test AIPractice1.slnx` again. Completion requires a successful full run with expected test discovery; zero discovered tests is not a successful validation.

## Project constraints

- The existing process-level test checks malformed startup configuration, exit code `99`, and stderr. It does not prove that a real exchange succeeds.
- Keep tests isolated from real `m_exchng.exe` processes. Application startup and cleanup can kill all processes with that name; testing real exchange requires separate authorization.
- Use temporary directories and test-owned configuration. Do not expose credentials from deployment settings in output or fixtures.

## Result

Report the commands actually run, passed/failed/skipped counts, the cause and repair if applicable, and remaining blockers. Do not claim a passing suite from an earlier run before subsequent changes. This skill authorizes local validation and focused repairs within the current task, not commits, pushes, or pull-request publication.
