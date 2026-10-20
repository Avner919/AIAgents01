import importlib.util
import io
import json
from pathlib import Path
import subprocess
import tempfile
import unittest
from unittest.mock import patch


SPEC = importlib.util.spec_from_file_location(
    "format_after_edit", Path(__file__).resolve().parents[1] / "format_after_edit.py"
)
hook = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(hook)


class FormatAfterEditTests(unittest.TestCase):
    def setUp(self):
        self.directory = tempfile.TemporaryDirectory()
        self.addCleanup(self.directory.cleanup)
        self.root = Path(self.directory.name).resolve()
        self.root_patch = patch.object(hook, "ROOT", self.root)
        self.root_patch.start()
        self.addCleanup(self.root_patch.stop)
        for name in ["Tests/Example.cs", "Exchange/Other.cs", "Tests/obj/Generated.cs", "README.md"]:
            path = self.root / name
            path.parent.mkdir(parents=True, exist_ok=True)
            path.write_text("class Example {}", encoding="utf-8")

    def event(self, command):
        return {"tool_name": "apply_patch", "cwd": str(self.root),
                "tool_input": {"command": command}, "tool_response": {"exit_code": 0}}

    def test_patch_selects_only_existing_source_files(self):
        event = self.event("*** Update File: Tests/Example.cs\n*** Add File: Exchange/Other.cs\n"
                           "*** Update File: README.md\n*** Update File: Tests/obj/Generated.cs\n"
                           "*** Delete File: Deleted.cs\n*** Update File: Missing.cs\n"
                           "*** Update File: Tests/Example.cs\n")
        self.assertEqual(hook.edited_files(event), ["Exchange/Other.cs", "Tests/Example.cs"])

    def test_outside_repository_is_rejected(self):
        with tempfile.TemporaryDirectory() as directory:
            outside = Path(directory) / "Outside.cs"
            outside.write_text("class Outside {}", encoding="utf-8")
            self.assertEqual(hook.edited_files(self.event(f"*** Update File: {outside}\n")), [])

    def test_edit_supports_absolute_path_and_failed_edits_are_skipped(self):
        event = {"tool_name": "Edit", "tool_input": {"file_path": str(self.root / "Tests/Example.cs")}}
        self.assertEqual(hook.edited_files(event), ["Tests/Example.cs"])
        event["tool_response"] = {"isError": True}
        self.assertEqual(hook.edited_files(event), [])

    def test_formatter_receives_only_edited_paths(self):
        event = self.event("*** Update File: Tests/Example.cs\n")
        with patch("sys.stdin", io.StringIO(json.dumps(event))), patch("sys.stdout", io.StringIO()):
            with patch.object(hook.subprocess, "run", return_value=subprocess.CompletedProcess([], 0)) as run:
                self.assertEqual(hook.main(), 0)
        self.assertEqual(run.call_args.args[0], ["dotnet", "format", "whitespace", "AIPractice1.slnx",
                         "--no-restore", "--include", "Tests/Example.cs", "--verbosity", "minimal"])
        self.assertEqual(run.call_args.kwargs["cwd"], self.root)

    def test_non_source_edit_does_not_start_formatter(self):
        with patch("sys.stdin", io.StringIO(json.dumps(self.event("*** Update File: README.md\n")))):
            with patch.object(hook.subprocess, "run") as run:
                self.assertEqual(hook.main(), 0)
                run.assert_not_called()

    def test_failure_returns_blocking_hook_feedback(self):
        event = self.event("*** Update File: Tests/Example.cs\n")
        failure = subprocess.CompletedProcess([], 1, "", "formatter error")
        with patch("sys.stdin", io.StringIO(json.dumps(event))), patch("sys.stderr", io.StringIO()) as error:
            with patch.object(hook.subprocess, "run", return_value=failure):
                self.assertEqual(hook.main(), 2)
        self.assertIn("formatter error", error.getvalue())

    def test_timeout_returns_blocking_hook_feedback(self):
        event = self.event("*** Update File: Tests/Example.cs\n")
        with patch("sys.stdin", io.StringIO(json.dumps(event))), patch("sys.stderr", io.StringIO()):
            with patch.object(hook.subprocess, "run", side_effect=subprocess.TimeoutExpired("dotnet", 120)):
                self.assertEqual(hook.main(), 2)


if __name__ == "__main__":
    unittest.main()
