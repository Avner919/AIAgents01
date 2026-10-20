"""Format C# paths from Codex file-edit events without touching other files."""

import json
from pathlib import Path
import re
import subprocess
import sys


ROOT = Path(__file__).resolve().parents[2]
IGNORED = {"bin", "obj", ".git", ".artifacts"}


def edited_files(event):
    response = event.get("tool_response")
    if isinstance(response, dict) and (
        response.get("isError") or response.get("exit_code", 0) not in (0, None)
    ):
        return []

    tool_input = event.get("tool_input", {})
    if isinstance(tool_input, str):
        tool_input = {"command": tool_input}
    if not isinstance(tool_input, dict):
        return []

    paths = []
    if event.get("tool_name") in {"apply_patch", "Edit", "Write"}:
        file_path = tool_input.get("file_path")
        if isinstance(file_path, str):
            paths.append(file_path)
        patch = tool_input.get("command", "")
        if isinstance(patch, str):
            paths.extend(re.findall(r"^\*\*\* (?:Add|Update) File: (.+)$", patch, re.MULTILINE))

    cwd = Path(event.get("cwd") or ROOT)
    if not cwd.is_absolute():
        cwd = ROOT / cwd
    result = set()
    for value in paths:
        path = Path(value.strip())
        path = (path if path.is_absolute() else cwd / path).resolve()
        if not path.is_relative_to(ROOT) or path.suffix.lower() != ".cs" or not path.is_file():
            continue
        relative = path.relative_to(ROOT)
        if not any(part.lower() in IGNORED for part in relative.parts):
            result.add(relative.as_posix())
    return sorted(result)


def main():
    try:
        event = json.load(sys.stdin)
        if not isinstance(event, dict):
            raise ValueError("Expected a JSON hook event object")
        files = edited_files(event)
        if not files:
            return 0

        completed = subprocess.run(
            ["dotnet", "format", "whitespace", "AIPractice1.slnx", "--no-restore",
             "--include", *files, "--verbosity", "minimal"],
            cwd=ROOT, capture_output=True, text=True, timeout=120,
        )
        if completed.returncode != 0:
            print("Formatter hook failed; check restore/build and formatter diagnostics.\n"
                  + (completed.stdout + completed.stderr)[-4000:], file=sys.stderr)
            return 2
        print(json.dumps({"systemMessage": "Formatted C# files: " + ", ".join(files)}))
        return 0
    except (OSError, ValueError, subprocess.TimeoutExpired) as error:
        print(f"Formatter hook failed: {error}", file=sys.stderr)
        return 2


if __name__ == "__main__":
    sys.exit(main())
