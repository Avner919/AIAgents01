import importlib.util
import json
from pathlib import Path
import subprocess
import sys
import tempfile
import unittest
from unittest.mock import patch


SCRIPT = Path(__file__).resolve().parents[1] / "protect_files.py"
SPEC = importlib.util.spec_from_file_location("protect_files", SCRIPT)
hook = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(hook)


class ProtectFilesTests(unittest.TestCase):
    def setUp(self):
        self.directory = tempfile.TemporaryDirectory()
        self.addCleanup(self.directory.cleanup)
        self.root = Path(self.directory.name).resolve()
        root_patch = patch.object(hook, "ROOT", self.root)
        root_patch.start()
        self.addCleanup(root_patch.stop)

    def event(self, tool, arguments, cwd=None):
        return {"tool_name": tool, "tool_input": arguments, "cwd": str(cwd or self.root)}

    def test_patch_blocks_every_secret_operation_and_move_destination(self):
        for operation in ("Add File", "Update File", "Delete File", "Move to"):
            for name in (".env", "nested/.env", ".ENV", ".env.local"):
                with self.subTest(operation=operation, name=name):
                    self.assertTrue(hook.blocked(self.event(
                        "apply_patch", {"command": f"*** {operation}: {name}\n"})))

    def test_patch_allows_example_and_documentation_mentions(self):
        self.assertFalse(hook.blocked(self.event("apply_patch", {"command":
            "*** Update File: README.md\n+Do not modify .env\n"
            "*** Add File: .env.example\n+KEY=\n"})))

    def test_edit_checks_relative_absolute_and_parent_paths(self):
        for name in (".env", str(self.root / ".env"), "../.env"):
            with self.subTest(name=name):
                self.assertTrue(hook.blocked(self.event(
                    "Edit", {"file_path": name}, self.root / "nested")))

    def test_edit_allows_files_outside_project(self):
        self.assertFalse(hook.blocked(self.event(
            "Write", {"file_path": str(self.root.parent / ".env")})))

    def test_shell_and_other_tools_block_literal_secret_references(self):
        for command in ("Set-Content .env value", "echo value > .env",
                        "Remove-Item -Recurse .env.local", "cat nested/.ENV",
                        "python -c \"open('.env','w')\"", "rm .env.*"):
            with self.subTest(command=command):
                self.assertTrue(hook.blocked(self.event("Bash", {"command": command})))
        self.assertTrue(hook.blocked(self.event("mcp__fs__write", {"path": "nested/.env"})))

    def test_allows_routine_commands_and_example(self):
        for command in ("dotnet test AIPractice1.slnx", "git status --short",
                        "Get-Content .env.example", "echo environment"):
            self.assertFalse(hook.blocked(self.event("Bash", {"command": command})))

    def test_unknown_edit_payload_is_denied(self):
        self.assertTrue(hook.blocked(self.event("apply_patch", {"unexpected": "value"})))

    def test_process_returns_supported_deny_without_echoing_input(self):
        result = subprocess.run([sys.executable, str(SCRIPT)], input=json.dumps(
            self.event("Bash", {"command": "echo private-placeholder > .env"})),
            text=True, capture_output=True, timeout=10)
        self.assertEqual(result.returncode, 0)
        self.assertEqual(json.loads(result.stdout)["hookSpecificOutput"]["permissionDecision"], "deny")
        self.assertNotIn("private-placeholder", result.stdout + result.stderr)

    def test_malformed_json_returns_explicit_deny(self):
        result = subprocess.run([sys.executable, str(SCRIPT)], input="{", text=True,
                                capture_output=True, timeout=10)
        self.assertEqual(result.returncode, 0)
        self.assertEqual(json.loads(result.stdout)["hookSpecificOutput"]["permissionDecision"], "deny")


if __name__ == "__main__":
    unittest.main()
