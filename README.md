# ✦ The Weaver: Resonance

**Stabilise a collapsing reality in short, intense sessions — then recover in a calm, luminous sanctuary.**

A short-session, emotionally engaging, visually iconic hybrid strategy game. The emotional hook is the contrast between the frantic stabilisation phase and the serene Dream Warren recovery — failure is not a punishment, it is the most beautiful part of the loop.

![Genre](https://img.shields.io/badge/genre-hybrid--casual%20strategy-4ECDC4) ![Platform](https://img.shields.io/badge/platform-iOS%20%2B%20WebGL-16213e) ![Engine](https://img.shields.io/badge/engine-Unity%202D%20URP-5F27CD) ![Session](https://img.shields.io/badge/session-2--4%20minutes-F7B731)

---

## The Core Loop

> **Enter → Anchor → Stabilise → Overload → Collapse → Dream → Upgrade → Repeat**

Every session delivers a complete emotional arc: **tension → crisis → relief → growth**. Even a 2-minute run feels like a mini-narrative.

| Phase | What happens |
|---|---|
| **Sector** | Activate 3–5 anchor nodes by holding a channel (1.5–3s). Any hit interrupts it. Place Torii Gates (max 2) to block the Drift. |
| **Mental Guard (MGS)** | One circular meter = health + sanity + stamina. Passively drains. Below 60: *Strain*. Below 30: *Reality Fade* — input lag, ghost frames, muffled audio. |
| **Collapse** | MGS hits 0 → character downed → white-out → **Dream Warren**. |
| **Dream Warren** | 20–40s sanctuary. Tap Lume-Bunnies (+5 MGS), rest near Rams the Guardian (+2/s). Zero enemies. Pure recovery. |
| **Progression** | Earn Resonance Fragments → upgrades (anchor speed, gate duration, recovery) & cosmetics. Never sell power. |

---

## Core Systems (implemented in `Assets/Scripts/`)

| Script | Responsibility |
|---|---|
| `PlayerController.cs` | Drag/click movement, hold-to-activate anchors, gate placement, damage |
| `MGSManager.cs` | The single MGS bar, threshold states (Stable/Strain/Fade/Collapse), Dream recovery |
| `AnchorSystem.cs` | Anchor nodes + `SectorManager` (instability tracking, win condition) |
| `GateSystem.cs` | Torii Gates — duration, slow aura, blocking, damage absorption |
| `EnemySpawner.cs` | Wave-based spawning; base `DriftEnemy` + Swarmer/Breaker/Phantom behaviours |
| `RealityFadeEffect.cs` | Post-processing (chromatic aberration, motion blur, vignette), audio low-pass, UI flicker, input delay |
| `DreamWarrenManager.cs` | Dream scene, Lume-Bunnies, Guardian regen, MGS restore |

Architecture: component-based, event-driven (`System.Action`), no singletons, all tunables exposed via `[SerializeField]`.

## Enemies (MVP = 3, no more)

| Type | Speed | Damage | Behaviour |
|---|---|---|---|
| **Swarmers** | Fast | -10 | Rush the player, die on contact |
| **Breakers** | Slow | -20 | Siege anchors/gates, 3-hit structures |
| **Phantoms** | Medium | -15 | Phase in/out every 2s, unpredictable |

## MGS Thresholds

| State | Range | Signature effect |
|---|---|---|
| Stable | 100–60 | Clean cyan glow |
| Strain | 60–30 | Amber shift, faster pulse, tension audio layer |
| Reality Fade | 30–10 | Input delay 100–300ms, ghosting, chromatic aberration, 1000Hz low-pass |
| Collapse | 0 | White-out → Dream Warren |

---

## Design Docs

- 📕 **MVP Master Bible** — product positioning, full system specs, visual style guide with hex codes, audio design brief, 4-week build plan, monetisation & scaling roadmap (£10k–£50k/month path), analytics targets.
- 📗 **Web App Visual Preview** — 16 screen-by-screen mockups, HUD component wireframes, complete user flow, frame-by-frame screenplay of a full session, responsive breakpoints, animation specs.

*(Both PDFs are design deliverables — ask for them in the project workspace or check releases.)*

## Tech Stack

- **Engine:** Unity 2022.3 LTS, 2D URP
- **Post-processing:** Post Processing Stack v2 (Reality Fade)
- **Audio:** Unity Audio Mixer, dynamic low-pass filtering
- **Analytics:** Unity Analytics + Firebase (session length, collapse rate, dream usage)
- **Monetisation:** Unity IAP (cosmetics only) + rewarded ads (cap: 5/session, no interstitials)
- **Targets:** 60fps on iPhone 12, <100MB textures, <50 draw calls/frame

## MVP Success Criteria

- ✅ >60% of players start a second run within 30s of their first ending
- ✅ >80% of Dream Warren time used (players don't exit early)
- ✅ Zero "the controls suck" mentions in playtests
- ✅ Players describe the game as *"beautiful, intense, calming"*
- ✅ >2% IAP conversion or >£500 ad revenue in Month 1

**Rule:** build exactly **1 level, 3 enemies, 1 character, 1 loop**. Then test. Then scale.

## Roadmap

| Phase | Scope | Timeline |
|---|---|---|
| 1 — MVP | 1 sector, 3 enemies, core loop, iOS TestFlight + WebGL | 4 weeks |
| 2 — Content | 3 new sectors, 2 enemy types, daily challenges, social | Months 2–3 |
| 3 — Revenue | Season pass, cosmetic marketplace, premium mode | Months 4–6 |
| 4 — Platforms | Android, Steam, Nintendo Switch | Months 6–12 |

---

*The Weaver: Resonance is an emotional game. Every pixel, sound and animation serves the tension/recovery duality. If a feature looks cool but doesn't serve that duality — cut it.*
