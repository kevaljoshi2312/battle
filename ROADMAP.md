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
- [ ] **Day 1 milestone:** Click ground → one unit moves there (`UnitMovement.cs` + **Battle → Setup Milestone 1** in Unity)

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
│
├── Unit.cs               ← Week 4 (shared stats)
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
| 1 | Click-to-move | Capsule follows mouse clicks on the plane |
| 2 | Selection | Click soldier → highlight → click ground → only selected unit moves |
| 3 | Multi-select | Shift-click or drag-box → all selected units move |
| 4 | One enemy | Red unit auto-attacks or chases when in range |
| 5 | Three unit types | Different stats on shared Health + UnitCombat components |

---

## Day 1 starter script reference

`Assets/Scripts/UnitMovement.cs`:

```csharp
using UnityEngine;

public class UnitMovement : MonoBehaviour
{
    [SerializeField] float moveSpeed = 5f;

    Vector3 targetPosition;
    bool hasTarget;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                targetPosition = hit.point;
                hasTarget = true;
            }
        }

        if (!hasTarget) return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            hasTarget = false;
    }
}
```

### Common Day 1 pitfalls

1. **Camera not tagged `MainCamera`** — `Camera.main` returns null
2. **Clicking the capsule** — ray hits the soldier first; expected until selection is added
3. **Over-engineering** — one script, one unit, one behavior. That's it.

---

## North star

> 🎯 **"I can select one soldier and move him around a battlefield."**

That is the correct starting point for this game.
