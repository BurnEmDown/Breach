# Breach — Planning Document

## Overview
Breach is a 2-player abstract strategy board game played on a 3×3 tile grid.
Each player controls 2 agents and uses actions to rearrange tiles, working
toward goals revealed by goal tiles (designed in a later phase).

---

## Components

| Component            | Count | Notes                                               |
|----------------------|-------|-----------------------------------------------------|
| Main board           | 1     | 3×3 grid, holds 9 tiles                             |
| Tiles                | 15    | 5 orange, 5 green, 5 purple; each tile has 3 colors |
| Player board         | 2     | One per player; holds 3 tiles each                  |
| Agents               | 4     | 2 per player                                        |
| Goal tiles           | TBD   | Added in a future phase                             |
| Special action cards | TBD   | Added in a future phase                             |

Each tile carries **3 colors** (exact per-tile color distribution TBD; the
primary color identifies which set the tile belongs to).

---

## Setup

1. **Main board:** Place tiles so an orange diagonal runs corner-to-corner
   (top-left → center → bottom-right, positions (0,0), (1,1), (2,2)).
   Green and purple tiles are spread so that each color has **2 tiles on one
   side** and **1 tile on the other side** of the orange diagonal.
   - Upper-right side: (0,1), (0,2), (1,2)
   - Lower-left side:  (1,0), (2,0), (2,1)
   - Default spread: Green at (0,1), (0,2), (1,0) — Purple at (1,2), (2,0), (2,1)
2. **Player boards:** Each player's 3 player-board tile slots start empty
   (exact starting state TBD when player-board rules are finalized).
3. **Agents:** Each player places their 2 agents on **opposing corners** of
   the 3×3 grid.
   - Player 1: top-left (0,0) and bottom-right (2,2)
   - Player 2: top-right (0,2) and bottom-left (2,0)

---

## Gameplay

### Turn structure
- Players alternate turns.
- Each turn grants **2 action points (AP)**, except the **first player's
  very first turn**, which grants only **1 AP**.

### Basic actions (each costs 1 AP by default)

| Action       | Description                                                                   |
|--------------|-------------------------------------------------------------------------------|
| **Move**     | Move one of your agents to an orthogonally adjacent tile.                     |
| **Switch**   | Swap the tiles currently under each of your two agents.                       |
| **Override** | Swap the tile under one of your agents with a tile on your player board; the agent's former tile goes to the player board. |

### Rival-agent surcharge
If a basic action **ends with your agent on a tile that a rival agent also
occupies**, that action costs **2 AP** instead of 1.
Example: Moving into a tile occupied by an opponent's agent costs 2 AP.
*(Currently only Move causes an agent to land on a new tile; surcharge is
evaluated at Move resolution time.)*

---

## Win condition
**TBD** — will be defined by goal tiles in a future design phase.

---

## Software Architecture

### Project layout
```
Breach.Core  — pure domain logic (no I/O)
Breach.Cli   — CLI entry point, renders state, reads input, drives the loop
Breach.Tests — xUnit unit tests targeting Breach.Core
```

### Domain model (Breach.Core)

**Enums**
- `TileColor` — `Orange | Green | Purple`
- `PlayerId` — `One | Two`

**Value types / records**
- `Position` — `(int Row, int Col)` for the 3×3 grid (0-indexed, Row 0 = top)
- `Tile` — immutable record; `TileColor Primary`, `TileColor Secondary1`, `TileColor Secondary2`

**Entities**
- `Board` — 3×3 array of `Tile?`; exposes indexed access and orthogonal adjacency
- `PlayerBoard` — array of 3 `Tile?` slots
- `Agent` — `PlayerId Owner`, `Position Position`
- `Player` — `PlayerId Id`, `Agent[2] Agents`, `PlayerBoard Board`
- `GameState` — `Board`, `Player[2]`, current player index, AP remaining, first-turn flag

**Actions**
- `MoveAction(PlayerId Player, int AgentIndex, Position Target)`
- `SwitchAction(PlayerId Player)` — swaps tiles under the player's 2 agents
- `OverrideAction(PlayerId Player, int AgentIndex, int PlayerBoardSlot)`

**Game engine**
- `GameEngine` — validates and applies actions to `GameState`, manages AP,
  applies surcharge rule, advances turns
- Returns `ActionResult` (Success / Failure + `string Reason`)

**Setup**
- `GameSetup.CreateInitialState()` — returns the standard starting `GameState`

### CLI (Breach.Cli)
- ASCII renderer: prints 3×3 board with color abbreviations and agent markers
- Command parser: reads lines like `move 0 1,2`, `switch`, `override 1 2`
- Game loop: two humans, hot-seat on one terminal

### Tests (Breach.Tests)
- Initial state validation (board layout, agent positions, AP)
- Each action: valid cases and invalid cases
- AP surcharge rule (Move onto rival-occupied tile)
- Turn transition and first-turn 1-AP rule

---

## Implementation Phases

### Phase 1 — Core domain *(current)*
- [x] Define `TileColor`, `PlayerId`, `Position`, `Tile`
- [x] `Board`, `PlayerBoard`, `Agent`, `Player`, `GameState`
- [x] `MoveAction`, `SwitchAction`, `OverrideAction`, `ActionResult`
- [x] `GameEngine` (validate + apply + AP logic)
- [x] `GameSetup.CreateInitialState()`
- [ ] Unit-test all rules

### Phase 2 — CLI
- [ ] ASCII board renderer
- [ ] Command parser and game loop
- [ ] Hot-seat play (two humans, one terminal)

### Phase 3 — Goal tiles & win condition *(future)*
- [ ] Design goal tile components and scoring
- [ ] Integrate win-condition check into `GameEngine`

### Phase 4 — Special action cards *(future)*
- [ ] TBD
