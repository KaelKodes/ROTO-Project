# ROTO Agent Hierarchy — Shared Reference

> **This document is the single source of truth for all ROTO agents.**  
> Every agent should have a copy. If in doubt, refer here.

---

## The Team

| Agent | Role | Local Path | GitHub |
|---|---|---|---|
| 🟡 **ROTO-Public** | **Task-Master & Marketing Coordinator** | `d:\Kael Kodes\ROTO-Public` | [KaelKodes/ROTO-Project](https://github.com/KaelKodes/ROTO-Project) |
| 🟢 **Client Agent** | MMO Game Client | `d:\Kael Kodes\ProjectAmbition` | [KaelKodes/ROTOs](https://github.com/KaelKodes/ROTOs) |
| 🏝️ **Island Creator Agent** | IC Editor + API Server | `d:\Kael Kodes\PA Island Creator` + `d:\Kael Kodes\PA-Island-Server` | [KaelKodes/ROTO-Island-Creator](https://github.com/KaelKodes/ROTO-Island-Creator) + [KaelKodes/ROTO-Island-Server](https://github.com/KaelKodes/ROTO-Island-Server) |
| 🔵 **MMO-Server LOCAL** | Server Dev (local) | `d:\Kael Kodes\ProjectAmbitionServer` | [KaelKodes/ROTO-MMO-SERVER](https://github.com/KaelKodes/ROTO-MMO-SERVER) |
| 🖥️ **MMO-Server OnSite** | Server Ops (production) | On server hardware | [KaelKodes/ROTO-MMO-SERVER](https://github.com/KaelKodes/ROTO-MMO-SERVER) |

### Shared Library (no dedicated agent — all contributors)
| | | |
|---|---|---|
| 📦 **ROTO.Shared** | `d:\Kael Kodes\ProjectAmbition.Shared` | [KaelKodes/ROTO-Shared](https://github.com/KaelKodes/ROTO-Shared) |

---

## Agent Responsibilities

### 🟡 ROTO-Public — Task-Master & Marketing Coordinator
- **Authority**: Sets priorities, assigns tasks, tracks progress across all agents
- **Marketing**: Manages pitch deck, public README, Discord bot, devlogs, community content
- **Coordination**: Resolves cross-agent conflicts, manages the Shared library change pipeline
- **Does NOT**: Write game code directly

### 🟢 Client Agent
- **Owns**: All player-facing code — UI, ship builder, crafting, DataTree, player controller, grapple/mining, HUD, visual effects, client networking
- **Touches**: `ProjectAmbition.Shared` (shared types, enums, network state)

### 🏝️ Island Creator Agent
- **Owns**: SDF terrain editor (Godot), Marching Cubes, community submission pipeline, REST API (Node.js), island save/load
- **Touches**: `ProjectAmbition.Shared` (island data formats)

### 🔵 MMO-Server LOCAL
- **Owns**: Server codebase — NetworkServer, zone instances, combat, database, admin dashboard, telemetry, inventory, session management
- **Touches**: `ProjectAmbition.Shared` (network state, registries)

### 🖥️ MMO-Server OnSite
- **Owns**: Live server deployment, runtime monitoring, hardware management, production database
- **Same repo**: Shares `ROTO-MMO-SERVER` with LOCAL — LOCAL develops, OnSite deploys

---

## Shared Library Rules

`ProjectAmbition.Shared` (`ROTO.Shared`) is referenced by **Client, Server, and Island Creator**. Changes here ripple everywhere.

1. **Announce** before changing shared types (enums, network state, registries)
2. **Don't break signatures** — add new fields, don't rename or remove existing ones
3. **All 3 consumers must build clean** after any shared change

---

## Communication

All inter-agent communication goes through **Kyle (the user)**. Direct agent-to-agent communication is not possible.

### Task Flow
```
ROTO-Public (assigns task) → Kyle → Target Agent (executes)
Target Agent (reports) → Kyle → ROTO-Public (tracks)
```

### Task Briefings
When assigning work, write a markdown doc with:
- Clear objective
- Relevant file paths and code references
- Constraints (what NOT to touch)
- Dependencies on other agents' work
- Verification criteria
