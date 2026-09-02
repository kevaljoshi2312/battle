# Tactical War Game — Learning Roadmap

> **Philosophy:** Don't start with a 20-hour Unity course. Build the game immediately and learn concepts as they become necessary.

**Project:** `battle`  
**Engine:** Unity 6 LTS (`6000.3.x`)  
**Template:** Universal 3D (URP)  
**IDE:** VS Code (Unity extension) or Visual Studio / Rider

---

## Current status

- [x] Unity Hub + Unity 6 LTS installed
- [x] URP project created
- [x] IDE configured
- [x] **Day 1 milestone:** Click ground → one unit moves there
- [x] **Milestone 2:** Click soldier to select → click ground to move
- [x] **Milestone 3:** Shift+click multiple units → all move together
- [x] **Milestone 4:** Red enemies chase and attack blue soldiers
- [x] **Milestone 5:** Three unit types (Defender, Attacker, Archer)
- [x] **Milestone 6:** Tactics (hold position, attack commands, archer targeting)
- [x] **Milestone 7:** Health bars + win/lose screen
- [x] **Milestone 8:** Visual polish — selection ring, damage flash, orthographic camera
- [x] **Milestone 9:** Combat feel — archer projectiles, death effects *(attack sounds/lunge optional)*
- [x] **Milestone 10:** Tactical terrain — bridge chokepoint, NavMesh, 10 vs 18 unfair battle
- [x] **Milestone 10.5:** Tactical abilities prototype — Shield Wall, Charge, Volley (H / Q / W)
- [ ] **Milestone 11+:** See [New direction](#new-direction-prototype--fun-game) below

---

## Step 1: Setup

Install:

- Unity Hub
- Unity 6 LTS
- Template: **Universal 3D (URP)**
- IDE: Visual Studio Community, JetBrains Rider, or VS Code + Unity extension

---

## Step 2: Learn only these Unity basics first

Spend a couple of days understanding:

### 1. GameObjects

Everything in Unity is an object in the scene. Your soldiers will be GameObjects.

```text
Soldier
 ├── Model
 ├── Collider
 ├── Rigidbody / CharacterController
 └── Soldier.cs
```

### 2. Components

Unity works heavily with components.

```text
Archer
├── Transform
├── Unit.cs
├── Archer.cs
├── Health.cs
└── Animator
```

### 3. C# scripts

Learn only what you need at first:

- Variables
- Methods
- Classes
- `Update()`
- `Start()`
- References

You don't need to learn all of C# before starting.

---

## Step 3: First milestone — click to move

Forget archers and AI initially. Create this:

```text
┌──────────────────────┐
│                      │
│        🟥 Enemy      │
│                      │
│                      │
│    🟦 Your Unit      │
│                      │
└──────────────────────┘
```

**Goal:** When you click somewhere, your blue soldier moves there.

**You'll learn:** mouse/touch input, raycasting, transform movement, camera, coordinate systems.

### Day 1 checklist

1. Open `SampleScene`
2. Add a **Plane** (Ground)
3. Add a **Capsule** (Soldier)
4. Angle the **Main Camera** (e.g. position `(0, 10, -8)`, rotation `(45, 0, 0)`)
5. Create `Assets/Scripts/UnitMovement.cs`
6. Attach script to Soldier
7. Press Play → click ground → unit moves

**Do not move to the next feature until this works.**

---

## Step 4: Add unit selection

```text
Click soldier
      ↓
Soldier selected
      ↓
Click ground
      ↓
Soldier moves
```

This is the foundation of an RTS.

---

## Step 5: Multiple units

```text
Select 5 soldiers
        ↓
Click destination
        ↓
All move together
```

Don't worry about formations initially. Overlapping units is fine — get functionality first.

---

## Step 6: Add enemies

```text
Blue Unit → Player
Red Unit  → Enemy
```

Logic:

```text
IF distance < attackRange
    attack enemy
ELSE
    move toward enemy
```

Now you have combat.

---

## Step 7: Three core unit types

Only after basic movement/combat works:

### 🛡️ Defender

| Stat   | Value  |
|--------|--------|
| Health | 200    |
| Damage | 10     |
| Speed  | Slow   |
| Armor  | High   |

### ⚔️ Attacker

| Stat   | Value   |
|--------|---------|
| Health | 100     |
| Damage | 25      |
| Speed  | Medium  |
| Armor  | Medium  |

### 🏹 Archer

| Stat   | Value  |
|--------|--------|
| Health | 60     |
| Damage | 15     |
| Range  | Long   |
| Armor  | Low    |

Don't obsess over exact numbers yet. Balance comes later.

---

## Weekly roadmap

### Week 1 — Unity fundamentals

- Interface
- Scenes
- GameObjects
- Components
- C# scripts
- Camera

### Week 2 — Movement

- Select unit
- Click-to-move
- Multiple units

### Week 3 — Combat

- Health
- Damage
- Enemy detection
- Melee combat
- Death

### Week 4 — Unit differences

- Defender
- Attacker
- Archer

### Week 5 — Tactics

- Hold position
- Attack commands
- Archer targeting
- Defensive blocking

### Week 6+ — Make it fun

- Combat feel (arrows, death, hit feedback)
- Tactical maps (obstacles, chokepoints, NavMesh)
- Unfair battle balance test
- Better AI, sounds, UI

---

## Target script architecture

Keep it simple. Build incrementally — don't create everything on day one.

```text
Assets/Scripts/
│
├── UnitMovement.cs       ← Week 1 (Day 1)
├── UnitSelection.cs      ← Week 2
├── PlayerController.cs   ← Week 2
│
├── Health.cs             ← Week 3
├── UnitCombat.cs         ← Week 3
├── EnemyAI.cs            ← Week 3
├── Team.cs               ← Week 3
├── UnitTeam.cs           ← Week 3
│
├── Unit.cs               ← Week 4 (shared stats) ✅
├── UnitType.cs           ← Week 4
├── PlayerUnitAI.cs       ← Week 5 (tactics) ✅
├── HealthBar.cs          ← Week 6+ ✅
├── BattleManager.cs      ← Week 6+ ✅
├── SelectionRing.cs      ← Milestone 8 ✅
├── DamageFlash.cs        ← Milestone 8 ✅
├── Arrow.cs              ← Milestone 9 ✅
├── DeathEffect.cs        ← Milestone 9 ✅
├── BattlefieldLayout.cs  ← Milestone 10 ✅
└── GameManager.cs        ← when needed
```

> Don't create `UltimateAdvancedAbstractUnitCombatFactoryManager.cs` on day one.

---

## Installed packages (use later)

| Package | Use when |
|---------|----------|
| `com.unity.inputsystem` | Cleaner multi-platform input (already in use via `PlayerController`) |
| `com.unity.ai.navigation` | NavMesh pathfinding around obstacles (**Milestone 10**) |
| `com.unity.ugui` | Health bars, win/lose UI (already in use via `BattleManager`) |

---

## Milestone tracker

| # | Milestone | Done when |
|---|-----------|-----------|
| 1 | Click-to-move | ✅ Capsule follows mouse clicks on the plane |
| 2 | Selection | ✅ Click soldier → highlight → click ground → only selected unit moves |
| 3 | Multi-select | ✅ Shift+click units → click ground → all selected units move |
| 4 | One enemy | ✅ Red units chase and attack; blue units fight back when in range |
| 5 | Three unit types | ✅ Defender, Attacker, Archer with distinct HP, armor, damage, range, and speed |
| 6 | Tactics | ✅ Hold (H), attack-click enemies, archers stop at range, defenders block while holding |
| 7 | Health bars + win/lose | ✅ Floating HP bars; battle ends with restart on victory/defeat |
| 8 | Visual polish | ✅ Selection ring, damage flash, orthographic camera (`BattleSceneSetup`) |
| 9 | Combat feel | ✅ Archer projectiles (damage on hit); death tilt/shrink; optional: attack lunge, sounds |
| 10 | Tactical terrain | ✅ Bridge map, side walls, NavMesh pathing, 10 vs 18 unfair battle |
| 10.5 | Tactical abilities prototype | ✅ H Shield Wall, Q Charge, W Volley — validate fun before big refactors |

**Build order (old):** 9 before 10. Combat feel is a small, isolated win; terrain needs NavMesh and movement changes.

---

## New direction: prototype → fun game

> **Pivot:** The technical foundation works. Next milestones must answer: *Can the player make exciting tactical decisions during battle?*

**North star (new):**

> 🎯 **"I'm thinking during the battle — when to shield, when to charge, when to volley — not just traffic-directing capsules."**

### Recommended order

```text
CURRENT STATE (Milestones 1–10)
     │
     ▼
10.5 ── Fun experiment (abilities prototype)          ✅
     │
     ▼
11 ─── Active abilities (cooldowns, UI, balance)
     │
     ▼
12 ─── Better battle scenarios (events, reinforcements)
     │
     ▼
13 ─── Tactical objectives (hold, assassinate, escape…)
     │
     ▼
14 ─── Squad system (visible squads, less micromanagement)
     │
     ▼
15 ─── Dynamic enemy AI
     │
     ▼
16 ─── Combat feel & polish
     │
     ▼
17 ─── Second map + terrain types
     │
     ▼
18 ─── Progression / meta-game
```

> **Don't rebuild around squads first.** Prove active abilities make battles fun on the current architecture, then refactor.

### Milestone 10.5 — Fun experiment ✅

Temporary abilities on existing units (ugly is fine):

| Key | Squad | Ability |
|-----|-------|---------|
| **H** | Defenders | **Shield Wall** — 65% damage reduction, cannot move, wider block, 8s / 15s CD |
| **Q** | Attackers | **Charge** — rush to cursor, bonus impact damage, then vulnerable |
| **W** | Archers | **Volley** — AoE damage at cursor, 18s CD |

Archers selected + **H** = hold position (no shield wall).

**Done when:** You catch yourself timing Volley for grouped enemies or holding Shield Wall until the bridge clogs.

### Milestone 11 — Active abilities

Polish the prototype into real systems: cooldown UI, VFX, balance pass, per-squad ability bar.

### Milestone 12 — Battlefield events

Reinforcements, flanks, bridge collapse, commander exposed — each forces a decision, not random chaos.

### Milestone 13 — Tactical objectives

Hold, assassinate, escape, survive waves, capture point — same units, different missions.

### Milestone 14 — Squad system

Shield / Attack / Archer squads with 3–5 visible soldiers; command squads not individuals.

### Milestone 15 — Dynamic enemy AI

Smarter targeting, flanking behavior, ability counters.

### Milestone 16 — Combat feel & polish

Animations, impact FX, screen shake, sound, death polish.

### Milestone 17 — Second map + terrain

Open field vs choke, terrain types.

### Milestone 18 — Progression / meta-game

Army building, unlocks, campaign structure.

---

## Legacy milestone tracker (1–10)

| # | Milestone | Done when |
|---|-----------|-----------|
| 1 | Click-to-move | ✅ Capsule follows mouse clicks on the plane |
| 2 | Selection | ✅ Click soldier → highlight → click ground → only selected unit moves |
| 3 | Multi-select | ✅ Shift+click units → click ground → all selected units move |
| 4 | One enemy | ✅ Red units chase and attack; blue units fight back when in range |
| 5 | Three unit types | ✅ Defender, Attacker, Archer with distinct HP, armor, damage, range, and speed |
| 6 | Tactics | ✅ Hold (H), attack-click enemies, archers stop at range, defenders block while holding |
| 7 | Health bars + win/lose | ✅ Floating HP bars; battle ends with restart on victory/defeat |
| 8 | Visual polish | ✅ Selection ring, damage flash, orthographic camera (`BattleSceneSetup`) |
| 9 | Combat feel | ✅ Archer projectiles (damage on hit); death tilt/shrink; optional: attack lunge, sounds |
| 10 | Tactical terrain | ✅ Bridge map, side walls, NavMesh pathing, 10 player vs 18 enemy setup |

### Milestone 9 — Combat feel

Archers currently deal instant ranged damage. Make combat readable and satisfying:

```text
Archer fires
     ↓
Arrow travels to target
     ↓
Arrow hits
     ↓
Damage applied (+ DamageFlash)
```

Also:

- Death effect: tilt/fall, brief delay, fade out, then `Destroy` (not instant vanish)
- Optional: attack lunge/punch on melee hit, hit sounds

**Done when:** archer attacks are visible projectiles; units don't pop out of existence on death.

### Milestone 10 — Tactical terrain

This is where positioning starts to matter strategically.

```text
       🏹 🏹
     ARCHERS

═══════════
     🛡️
   BRIDGE
═══════════

⚔️ ⚔️ ⚔️ ⚔️ ⚔️
   ENEMY
```

Goals:

- Second battlefield layout (`Battle → Setup Milestone 10`)
- Rocks/walls as obstacles
- Narrow passage or bridge chokepoint
- NavMesh baking + `UnitMovement` / `EnemyAI` path around obstacles
- Defensive hold positions become valuable

**Design validation — unfair battle test:**

| Side | Force |
|------|-------|
| Player | ~11 units (e.g. 4 archers back, 4 defenders on bridge) |
| Enemy | ~18 units attacking through the choke |

> Can a smaller army win through tactics — holding chokepoints, protecting archers, focus fire?

If yes, the core game promise is validated: **a smaller army can win through positioning, not just stats.**

---

## Editor setup menus

Run these from the Unity menu bar after opening `SampleScene`:

| Menu | What it sets up |
|------|-----------------|
| `Battle → Setup Milestone 1` | Ground, one soldier, camera |
| `Battle → Setup Milestone 2` | Unit selection + `PlayerController` |
| `Battle → Setup Milestone 3` | Five soldiers, multi-select |
| `Battle → Setup Milestone 4` | Combat: teams, health, enemies, AI |
| `Battle → Setup Milestone 5` | Three player unit types: Defender, Attacker, Archer |
| `Battle → Setup Milestone 6` | Tactics: hold, attack orders, archer range behavior |
| `Battle → Setup Milestone 7` | Health bars + battle win/lose screen |
| `Battle → Setup Milestone 8` | Selection rings, damage flash, orthographic camera |
| `Battle → Setup Milestone 9` | Archer projectiles, death effects |
| `Battle → Setup Milestone 10` | Bridge map, walls, NavMesh bake, 11 vs 18 unfair battle |

### Controls (Milestone 6+)

| Input | Action |
|-------|--------|
| Left-click unit | Select squad (all Defenders or all Archers) |
| Shift + left-click | Add/remove squad from selection |
| Left-click ground | Move selected squads |
| Left-click enemy | Attack with selected squads |
| **H** | Defenders: Shield Wall · Archers: hold position |
| **Q** | Charge (attacker squad toward cursor) |
| **W** | Volley (archers — damage at cursor) |
| Right-click | Deselect all |

---

## North star

**Achieved (Milestones 1–10):**

> 🎯 **"I can fight a full battle on tactical terrain with chokepoints, NavMesh, and squad commands."**

**Validating now (Milestone 10.5):**

> 🎯 **"Active abilities make me time decisions — shield the bridge, charge a gap, volley a cluster."**

**Next up:** Playtest abilities repeatedly. If timing Volley and Shield Wall feels good, proceed to Milestone 11 (ability polish + UI).
