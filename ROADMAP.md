# Tactical Battle Game — Roadmap

> **Guiding principle:** Every milestone must improve **player decisions** or **battle excitement**. If open-field combat isn't fun, terrain is just decorating a boring sandwich.

**Project:** `battle` · Unity 6 LTS · URP

---

## Pivot: Open field first

The bridge introduced a lot of technical complexity before we validated whether **core combat is fun**. We now test on a clean battlefield:

```text
┌──────────────────────────────────┐
│       🔴 🔴 🔴 🔴 🔴              │
│             OPEN                 │
│            BATTLEFIELD           │
│       🔵 🔵 🔵 🔵 🔵              │
└──────────────────────────────────┘
```

No bridge. No chokepoint. No terrain advantage.

**The question:**

> Can the battle be fun purely through unit abilities, positioning, and decisions?

**Validation goal — the player should think:**

> *"My defenders are holding the front. I'll move attackers around the left. Wait, their archers are killing my attackers. Use Volley. Now charge their weakened side."*

---

## ✅ Foundation (Milestones 1–10) — DONE

Click-to-move → selection → multi-select → combat → unit types → tactics → health/win → visuals → projectiles → NavMesh + bridge map (legacy).

Bridge map remains available as **legacy** (`Battle → Setup Milestone 10`). Primary development uses **open field**.

---

## Roadmap

```text
CURRENT PROTOTYPE
        │
        ▼
11 ─ Open Field Setup          ← primary scene
        │
        ▼
12 ─ Active Abilities          ← prototype shipped, iterate
        │
        ▼
13 ─ Target Priority
        │
        ▼
14 ─ Flanking System
        │
        ▼
15 ─ Dynamic Events
        │
        ▼
16 ─ Squad System
        │
        ▼
17 ─ Formations
        │
        ▼
18 ─ Combat Polish
        │
        ▼
19 ─ Maps & Terrain
```

> **Do 11–14 before squads or terrain.** Prove the empty field can be exciting first.

---

# Milestone 11: Open Field Battle Setup

### Goal

Clean battlefield with room to move and flank.

### Setup

**Menu:** `Battle → Setup Milestone 11 (Open Field)` or `Battle → Create Open Field Scene`

| | Player | Enemy |
|---|--------|-------|
| 🛡️ Defenders | 3 (front) | 3 (front, 80 HP) |
| ⚔️ Attackers | 3 (mid) | 4 (mid) |
| 🏹 Archers | 3 (back) | 3 (back, ranged) |
| **Total** | **9** | **10** |

- Large open ground (no walls, barriers, bridge logic)
- Wide spacing between lines
- Bridge clamp disabled (`BattlefieldConfig`)

### Success criteria

You can clearly see frontline, backline, flanks, and units moving around each other.

### Status

- [x] Open field scene + setup menu
- [ ] Playtest — confirm spacing and readability feel good

---

# Milestone 12: Active Abilities

**Most important milestone.** Prove decisions matter.

| Unit | Ability | Current prototype |
|------|---------|-------------------|
| 🛡️ Defender | Shield Wall | H — 65% DR, immobile, 8s / 15s CD |
| ⚔️ Attacker | Charge | Q — rush + +14 first hit, 12s CD |
| 🏹 Archer | Volley | W — AoE at cursor, 18s CD |

### Success criteria

During battle you naturally think:

- *"Not yet, save Volley."*
- *"I need Shield Wall now!"*
- *"Charge the flank!"*

If not → **redesign abilities**, don't add systems.

### Status

- [x] Prototype (H/Q/W, ability bar, click-to-target)
- [ ] Honest playtest on open field
- [ ] Balance / VFX pass based on fun

---

# Milestone 13: Focus Fire & Target Priority

Let the player choose **what** to kill:

```text
🛡️ 🛡️ 🛡️  Frontline
🏹 🏹 🏹  Archers
```

- Attack frontline vs flank archers vs Volley a cluster
- Positioning matters without terrain

### Status

- [x] Click enemy to focus squad attack + orange highlight on target
- [x] Focus line in ability bar (`DEF → E2 | ARC → E7`)
- [ ] Playtest — does target choice feel meaningful?

---

# Milestone 14: Flanking Mechanic

| Approach | Damage |
|----------|--------|
| Front | Normal |
| Side | +25% |
| Behind | +50% |

Uses unit **nose direction** (forward). Melee and archer shots apply flanking. Toast feedback on side/back hits.

### Status

- [x] Flanking damage multipliers
- [x] Player feedback on flank/back hits
- [ ] Playtest — circling with attackers feels rewarding

---

# Milestone 15: Battle Events

**Only after 11–14 work.** Scripted mid-fight pressure.

- Enemy reinforcements, flanks, commander exposed
- Warning banners (`BattleWarningUI`)
- Timeline (`BattleScenario`)

**Menu:** `Battle → Setup Milestone 15 (Battle Events)` — overlays waves on current open-field scene.

### Status

- [x] Code prototype (waves at 0/15/30/45s)
- [ ] Enable after open-field tactics validated
- [ ] Playtest timing

---

# Milestone 16: Squad System

Command squads, not individuals. Soldiers stay visible; squad handles formation movement.

---

# Milestone 17: Squad Formations

Default shapes per squad (shield block, archer line, attack wedge). Move · Hold · Attack.

---

# Milestone 18: Combat Polish

Swing animations, impacts, arrow trails, recoil, screen shake, audio, camera emphasis.

---

# Milestone 19: Maps & Terrain

Bridge, forest, castle, village — **terrain as amplifier**, not crutch. Only after open-field loop is fun.

---

## Editor menus

| Menu | Purpose |
|------|---------|
| **`Battle → Setup Milestone 11 (Open Field)`** | **Primary** — 9v10 open battle |
| **`Battle → Create Open Field Scene`** | Dedicated `OpenBattle.unity` |
| `Battle → Setup Milestone 15 (Battle Events)` | Add scripted waves to current scene |
| `Battle → Setup Milestone 10 (Tactical Terrain)` | Legacy bridge map |

---

## Controls (current)

| Input | Action |
|-------|--------|
| Left-click unit | Select squad (by type) |
| Shift + click | Multi-select squads |
| Left-click ground / enemy | Move / attack |
| **H** | Shield Wall / hold (archers) |
| **Q** | Charge (click target) |
| **W** | Volley (click target) |
| Right-click | Deselect |

---

## North star

**Now validating:**

> 🎯 *"I'm making tactical decisions on an empty field — abilities, targets, positioning — not just moving capsules."*

**Next:** Playtest Milestones 13–14 on open field. If circling + focus fire feel good, consider Milestone 15 (battle events).
