# Battle Solitaire

Battle Solitaire is a mobile-first competitive solitaire game prototype built around real solitaire decisions plus real-time battle pressure.

## Current target

- Unity 6000.6.3f1
- Android first
- Portrait orientation
- Local player-vs-AI prototype first
- Online 1v1 and ranked play later

## Architecture

Gameplay rules remain separate from Unity presentation.

- `Core/` — cards, seeded deals, legal solitaire moves, foundations, tableau state, win detection.
- `Battle/` — HP, energy, shield, combo timing, attacks, disruption state, damage, match results.
- `AI/` — human-like action timing plus the first solitaire move solver.
- `Presentation/` — runtime-generated mobile board, card input, HUD, attacks, results.
- `Editor/` — one-click playable-scene setup.

## Completed

### Milestone 1 — Solitaire engine
- deterministic 52-card seeded deal
- seven-column tableau
- stock and waste
- four foundations
- legal tableau sequencing
- draw/recycle flow
- tableau/foundation/waste moves
- hidden-card reveal
- perfect-clear detection

### Milestone 2 — Battle systems
- 100 HP battle state
- 100 max energy
- shield system
- 2.5 second full combo window / 5 second expiration
- Lock attack
- Fog attack
- Blocker attack
- progress-based damage
- opponent progress reporting
- perfect-clear battle victory
- AI attack pacing
- anti-energy-farming rule for foundation rollback moves

### Milestone 3 — Playable Unity prototype
- portrait mobile battle board
- generated card visuals
- tap and drag interaction
- stock/waste/foundation/tableau controls
- player and opponent HUD
- attack buttons and disruption overlays
- AI solitaire move solver
- local player-vs-AI match loop
- victory/defeat/rematch flow
- Editor command to generate the playable Battle scene

## Play the prototype

1. Open the repo in Unity 6000.6.3f1.
2. Wait for scripts/packages to finish importing.
3. Select **Battle Solitaire > Setup Playable Prototype**.
4. Press **Play**.

See `Docs/MILESTONE_3.md` for controls and current limitations.

## Next

Milestone 4 should focus on game feel and mobile delivery: animation, sound/haptics, polished card art, onboarding, stronger AI/fairness tools, and a repeatable Android APK build path before online multiplayer.
