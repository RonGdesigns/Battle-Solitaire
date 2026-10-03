# Milestone 2 — Battle Systems Foundation

Milestone 2 turns the deterministic solitaire engine into the first battle-capable rules layer.

## Starting battle values

| System | Value |
| --- | ---: |
| Health | 100 |
| Maximum energy | 100 |
| Maximum shield | 30 |
| Normal progress move | +1 energy |
| Reveal hidden card | +3 energy |
| Foundation move | +2 energy |
| Clear tableau column | +10 energy |
| 3x combo threshold | +4 energy |
| 5x combo threshold | +8 energy |
| Foundation damage | 1 |
| Cleared-column damage | 5 |
| Foundation shield | +2 |
| Full combo window | 2.5 sec |
| Combo expiration | 5 sec |
| Lock | 25 energy / 3 sec |
| Fog | 25 energy / 4 sec |
| Blocker | 50 energy / 3 progress moves / 8 sec max |

## Attack behavior

### Lock
Disables interaction with one target tableau column for 3 seconds.

### Fog
Hides exposed card values in presentation for 4 seconds. The underlying card state is unchanged.

### Blocker
Places a battle obstruction on one tableau column. It clears after the defender completes 3 valid progress moves, or after 8 seconds as a safety timeout.

Attack effects intentionally do not deal direct damage in this milestone. Damage comes from solitaire progress so disruption does not double-punish the defender.

## Anti-farming rule

Moves from a foundation back to the tableau remain legal solitaire moves, but they do not:

- generate battle energy,
- advance combo,
- damage the opponent,
- generate shield,
- clear blocker durability.

This prevents resource farming by repeatedly bouncing the same card between foundation and tableau.

## AI boundary

BattleAIController currently handles human-like move timing and attack decisions. A solitaire move solver is intentionally separate and belongs to the next gameplay/presentation milestone.

## Integration contract

A visual board controller should:

1. call the appropriate SolitaireGame move method,
2. pass the returned MoveResult to BattleMatch.RegisterMove,
3. check BattleMatch.CanUseColumn before interactions involving a tableau column,
4. call BattleMatch.Tick every frame,
5. render HP, energy, shield, combo, opponent progress, fog, locks, and blockers from battle state,
6. execute BattleAIIntent requests through the future AI move solver.
