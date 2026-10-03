# Battle Solitaire

Battle Solitaire is a mobile-first competitive solitaire game prototype built around real solitaire decisions plus real-time battle pressure.

## Target

- Unity 6.6
- Android first
- Portrait orientation
- Local player-vs-AI prototype first
- Online 1v1 and ranked play later

## Architecture

Gameplay rules are kept in pure C# and separated from Unity presentation.

`Core/` owns cards, seeded deals, legal solitaire moves, foundations, tableau state, and win detection.

`Battle/` owns HP, energy, shield, combo timing, attacks, disruption state, damage, and match results.

`AI/` begins the opponent decision layer. The current controller schedules human-like move attempts and decides when to spend battle energy; the actual solitaire move solver comes next.

## Completed

### Milestone 1
- deterministic 52-card seeded deal
- seven-column tableau
- stock and waste
- four foundations
- legal tableau sequencing
- draw/recycle flow
- tableau/foundation/waste moves
- hidden-card reveal
- perfect-clear detection

### Milestone 2
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
- AI move pacing and attack-decision scaffold
- anti-energy-farming rule for foundation rollback moves

See `Docs/MILESTONE_2.md` for the current battle tuning and integration contract.

## Next milestone

Build the Unity portrait battle board:

- render card prefabs from pure game state,
- drag/tap interaction,
- animate legal and rejected moves,
- battle HUD,
- visual Lock/Fog/Blocker effects,
- AI solitaire move solver,
- playable local match and rematch loop.
