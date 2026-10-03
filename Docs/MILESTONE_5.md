# Milestone 5 — Visual Identity and Front End

Milestone 5 turns the visual-pass concepts into the actual Unity runtime UI.

## Front end

The game now opens to a pre-battle screen instead of immediately starting a match.

Starter battlers:
- Vesper — The Crimson Deal
- Kael — The Resolute
- Aldric — The Iron Mark

Battler differences remain cosmetic in this milestone. Gameplay bonuses are deliberately deferred so the visual pass does not silently rebalance combat.

The screen includes:
- active battler identity and quote,
- Lock / Fog / Block loadout,
- local wins,
- longest combo,
- perfect clears,
- local win/loss record,
- Battle button.

## Local profile

BattleProfile stores the selected battler and local career stats with PlayerPrefs.

Stats update when a match ends. A perfect clear is counted when the player completes the solitaire board.

## Battle identity

The selected battler and an automatically chosen rival now appear in the battle HUD. The rival rotates among the other starter battlers.

## Card and table pass

The prototype now uses:
- real suit glyphs,
- rank/suit corner treatment,
- large center suit marks,
- navy/gold card backs,
- cream card faces,
- dark green felt,
- gold table framing,
- deep navy panels,
- cyan energy accents,
- gold combo accents,
- crimson danger accents.

No imported art is required yet.

## Android

Play-test version: 0.5.0
Version code: 5

APK output:
Builds/Android/BattleSolitaire-M5-Playtest.apk

## Next asset step

The runtime UI is now ready for imported production art:
- Vesper portrait,
- Kael portrait,
- Aldric portrait,
- final card-back texture,
- ability icons,
- arena/table background,
- branded app icon and splash screen.

Those assets can replace the current generated presentation without rewriting the game rules.
