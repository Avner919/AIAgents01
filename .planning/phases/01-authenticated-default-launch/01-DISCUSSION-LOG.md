# Phase 1: Authenticated Default Launch - Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions are captured in 01-CONTEXT.md; this log preserves alternatives considered.

**Date:** 2026-10-31
**Phase:** 01-authenticated-default-launch
**Areas discussed:** API-key operation; Deployment and configuration

The user selected areas 2 and 3. HTTP contract and detailed launch errors were offered but not selected.

## API-key operation

**Question:** How should callers send the API key?

**Options presented:**
- X-Api-Key header (recommended)
- Authorization: Bearer <key>
- You decide

**User choice:** X-Api-Key header

**Question:** If the server API key is missing or blank, what should happen?

**Options presented:**
- Refuse API startup (recommended)
- Start, but disable launches

**User choice:** Refuse API startup

**Question:** How should the server receive its API key?

**Options presented:**
- Environment variable (recommended)
- ASP.NET Core configuration
- You decide

**User choice:** Standard ASP.NET Core configuration sources, including deployment-managed JSON and environment overrides. Repository configuration contains no secret values.

**Question:** What should callers receive when their API key is missing or invalid?

**Options presented:**
- Same 401 Unauthorized response (recommended)
- 401 Unauthorized with distinct messages

**User choice:** Same generic 401 Unauthorized response; never echo the supplied key.

**Question:** Discuss API-key operation further or move on?

**Options presented:**
- Move to deployment and configuration
- Discuss API-key operation further

**User choice:** Move to deployment and configuration

## Deployment and configuration

**Question:** How do you intend to run the API on Windows?

**Options presented:**
- Manually as a console app initially
- As a Windows Service
- Under IIS

**User choice:** Manually as a console app initially.

**Question:** Where should the API find Exchange.exe?

**Options presented:**
- Configured full path (recommended)
- Alongside the API
- You decide

**User choice:** Configured full path to a separately published utility folder.

**Question:** Which configuration should supply the exchange settings reported as defaults?

**Options presented:**
- The utility configuration (recommended)
- The API configuration

**User choice:** Read the utility configuration from its deployment folder, apply environment overrides the child will inherit, and use that folder as its working directory. Default definitions remain in ExchangeProcess.

**Question:** If the configured Exchange.exe path or required configuration files are missing at API startup, what should happen?

**Options presented:**
- Refuse API startup (recommended)
- Start the API, reject launch requests

**User choice:** Refuse API startup and report the deployment problem immediately.

**Question:** Finish deployment discussion or discuss further?

**Options presented:**
- Finish deployment discussion and review captured decisions
- Discuss deployment and configuration further

**User choice:** Finish deployment discussion and review captured decisions.

## Final Review

The user selected "Create the Phase 1 context from these decisions" rather than "Explore more decisions first".

## Agent Discretion

No explicit "you decide" choices were selected. Endpoint naming, response shape, configuration key names, and detailed launch-error formatting remain open for planning.

## Deferred Ideas

None introduced. Existing Phase 2 scope remains unchanged.
