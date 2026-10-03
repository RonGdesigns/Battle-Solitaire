# Milestone 6 — Battler Identity and Local Progression

Milestone 6 makes battler selection affect gameplay and adds a simple local progression loop.

## Battler passives

### Vesper — Crimson Edge
Foundation moves deal +1 additional damage.

### Kael — Focused Hand
At combo x3 or higher, each progress move generates +1 additional energy.

### Aldric — Iron Mark
Maximum shield increases by 10.
Foundation moves generate +1 additional shield.

The AI receives the same battler passives. There are no player-only bonuses.

## Local progression

Matches now award local rank points:

- win: +30 RP
- draw: +8 RP
- loss: -12 RP, floored at 0

Ranks:

- Bronze: 0–199
- Silver: 200–449
- Gold: 450–799
- Platinum: 800–1199
- Diamond: 1200+

## Battler mastery

The selected battler earns mastery XP after every completed match.

Base gain: 60 XP

Additional mastery:
- win: +40 XP
- longest combo contribution: up to +40 XP
- perfect clear: +25 XP

Every 250 mastery XP adds one mastery level.

Mastery is currently a visible progression stat only. Gameplay upgrades from mastery are deliberately deferred to avoid pay-to-win or grind-to-win behavior.

## Placement rule fix

Invalid drag destinations remain controlled by the authoritative SolitaireGame state. A drop is only marked handled when the rules engine accepts it.

## Android

Version: 0.6.0
Version code: 6

Play-test APK:
Builds/Android/BattleSolitaire-M6-Playtest.apk
