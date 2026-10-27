"""Reject direct access to repository secret files before a tool runs.

This is a tool-input guard, not a sandbox for arbitrary programs or scripts.
Never read secret contents or include tool arguments in denial messages.
"""

import json
from pathlib import Path
import re
import sys


ROOT = Path(__file__).resolve().parents[2]
PATCH_PATH = re.compile(r"^\*\*\* (?:Add File|Update File|Delete File|Move to): (.+)$", re.MULTILINE)
# Deliberately reject reads too, so shell output cannot disclose secrets.
SECRET_REFERENCE = re.compile(
    r"(?<![\w.])\.env(?:\.[\w.*?\-]+)?(?![\w.])", re.IGNORECASE
)
REASON = "Protected secret file access blocked. Users maintain .env files manually; edit .env.example instead."


def secret_name(name):
    name = name.casefold()
    return name != ".env.example" and (name == ".env" or name.startswith(".env."))


def protected_path(value, cwd):
    path = Path(value.strip())
    path = path if path.is_absolute() else cwd / path
    # Check both the named path and its resolved target (symlink/junction aliases).
    for candidate in (path.absolute(), path.resolve()):
        if candidate.is_relative_to(ROOT) and secret_name(candidate.name):
            return True
    return False


def strings(value):
    if isinstance(value, str):
        yield value
    elif isinstance(value, dict):
        for item in value.values():
            yield from strings(item)
    elif isinstance(value, list):
        for item in value:
            yield from strings(item)


def blocked(event):
    if not isinstance(event, dict):
        raise ValueError("Expected an event object")
    cwd = Path(event.get("cwd") or ROOT)
    if not cwd.is_absolute():
        cwd = ROOT / cwd
    tool_input = event.get("tool_input")
    if event.get("tool_name") in {"apply_patch", "Edit", "Write"}:
        if isinstance(tool_input, str):
            tool_input = {"command": tool_input}
        if not isinstance(tool_input, dict):
            raise ValueError("Expected edit arguments")
        paths = []
        for field in ("file_path", "path", "target_file"):
            if isinstance(tool_input.get(field), str):
                paths.append(tool_input[field])
        patch = tool_input.get("command", tool_input.get("patch", ""))
        if isinstance(patch, str):
            paths.extend(PATCH_PATH.findall(patch.replace("\r\n", "\n")))
        if not paths:
            # Unknown edit format must not silently evade the path check.
            return True
        return any(protected_path(path, cwd) for path in paths)
    # Scan shell and other tool arguments conservatively: even a mention is
    # denied. This also covers literal paths in Python/Node/PowerShell code.
    return any(secret_name(match.group())
               for value in strings(tool_input)
               for match in SECRET_REFERENCE.finditer(value))


def main():
    try:
        deny = blocked(json.load(sys.stdin))
    except (ValueError, OSError, TypeError, RecursionError):
        deny = True
    if deny:
        print(json.dumps({"hookSpecificOutput": {
            "hookEventName": "PreToolUse",
            "permissionDecision": "deny",
            "permissionDecisionReason": REASON,
        }}))
    return 0


if __name__ == "__main__":
    sys.exit(main())
