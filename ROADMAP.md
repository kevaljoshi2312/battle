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
- [ ] **Milestone 8:** More polish (selection UI, effects, maps)

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

- Maps
- Terrain
- Better AI
- Effects
- UI

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
└── GameManager.cs        ← when needed
```

> Don't create `UltimateAdvancedAbstractUnitCombatFactoryManager.cs` on day one.

---

## Installed packages (use later)

| Package | Use when |
|---------|----------|
| `com.unity.inputsystem` | Cleaner multi-platform input (optional upgrade from legacy Input) |
| `com.unity.ai.navigation` | NavMesh pathfinding around obstacles (Week 5+) |
| `com.unity.ugui` | Selection rings, health bars, UI (Week 6+) |

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
| 8 | More polish | Selection UI, effects, maps |

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

### Controls (Milestone 6+)

| Input | Action |
|-------|--------|
| Left-click unit | Select |
| Shift + left-click | Multi-select |
| Left-click ground | Move selected units |
| Left-click enemy | Attack target with selected units |
| **H** | Hold position (defenders widen to block) |
| Right-click | Deselect all |

---

## North star

> 🎯 **"I can fight a full battle, see unit health, and get a clear win or lose result."**

Next up: selection UI, visual effects, and map variety.
