# Battle Solitaire

Battle Solitaire is a mobile-first competitive solitaire game prototype built around real solitaire decisions plus real-time battle pressure.

## Current target

- Unity 6000.6.3f1
- Android first
- Portrait orientation
- Local player-vs-AI prototype
- Online 1v1 and ranked play later

## Architecture

Gameplay rules remain separate from Unity presentation.

- `Core/` — cards, seeded deals, legal solitaire moves, foundations, tableau state, win detection.
- `Battle/` — HP, energy, shield, combo timing, attacks, disruption state, damage, match results.
- `AI/` — human-like action timing plus the first solitaire move solver.
- `Presentation/` — mobile board, input, HUD, tutorial, feedback, attacks, results, safe-area handling.
- `Editor/` — scene setup and Android play-test build tools.

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
- HP, energy, shield, combo timing
- Lock, Fog and Blocker
- progress-based damage
- battle win states
- opponent progress
- anti-energy-farming rules

### Milestone 3 — Playable Unity prototype
- portrait mobile battle board
- generated card visuals
- tap and drag controls
- battle HUD
- AI solitaire move solver
- local match/rematch flow

### Milestone 4 — Game feel + Android play-test
- safe-area mobile layout
- smoother HUD animation
- card selection/drag polish
- generated move/attack/hit/result audio
- mobile haptics
- battle flash/callout effects
- first-run tutorial with replayable help
- one-click Android play-test APK builder

## Play in Unity

1. Open the repo in Unity 6000.6.3f1.
2. Let Unity finish compiling/importing.
3. Open **Battle Solitaire > Open Battle Scene**.
4. Press **Play**.

If the scene has not been generated yet, use **Battle Solitaire > Setup Playable Prototype** first.

## Build an Android play-test APK

1. Install Android Build Support, SDK/NDK Tools and OpenJDK for Unity 6000.6.3f1 through Unity Hub.
2. Open the project.
3. Choose **Battle Solitaire > Android > Build Play-Test APK**.
4. Unity creates:

`Builds/Android/BattleSolitaire-M4-Playtest.apk`

This is a development/test build, not a Play Store release build.

See `Docs/MILESTONE_4.md` for details.


### Milestone 5 — Visual identity + front end
- pre-battle battler/loadout screen
- Vesper, Kael and Aldric starter roster
- local profile stats stored on-device
- battler identity carried into the battle HUD
- dark navy / gold / cyan interface theme
- green felt battle table refinement
- upgraded card faces with real suit glyphs and premium card backs
- result screen can return to loadout
- Android play-test version advanced to 0.5.0
