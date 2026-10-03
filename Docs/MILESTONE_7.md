# Milestone 7 — Targeting, Difficulty, and Production Art

Milestone 7 focuses on battle readability and test quality rather than adding more rules.

## Manual targeting

LOCK and BLOCK no longer automatically choose a rival column.

Tapping either attack opens a targeting overlay showing all seven rival lanes with:
- lane number,
- total card count,
- hidden-card count,
- whether the lane is currently targetable.

The battle pauses while the targeting overlay is open. Energy is not spent until a valid target is selected.

FOG remains an immediate full-rival effect and does not need a column target.

## AI difficulty

The loadout screen now includes three rival-AI settings.

### Casual
- slower move cadence,
- less frequent attacks,
- less accurate disruption targeting,
- waits until the player is further ahead before prioritizing Blocker.

### Standard
The previous intended balance point.

### Expert
- faster move cadence,
- attacks whenever resources allow,
- prioritizes columns with more hidden cards and deeper stacks,
- begins using Blocker earlier.

Difficulty changes AI decision behavior only. It does not secretly change HP, energy generation, solitaire rules, or battler passive strength.

## Production art integration

Milestone 7 introduces runtime art hooks for:
- Vesper portrait,
- Kael portrait,
- Aldric portrait,
- premium card-back art,
- Battle Solitaire crest.

The menu and battle HUD use battler portraits when the assets are present. Face-down cards use the imported card-back texture. Code-driven placeholders remain as a fallback if an art asset cannot load.

## Battle feedback

Attack feedback now includes a larger impact-frame animation in addition to the existing:
- banner callout,
- screen flash,
- sound,
- haptic feedback.

## Card placement

The Milestone 6 placement fix remains authoritative. A card visually stays at a destination only when SolitaireGame accepts that move.

## Android

Version: 0.7.0
Version code: 7

Play-test APK:
Builds/Android/BattleSolitaire-M7-Playtest.apk
