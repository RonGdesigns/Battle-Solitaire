# Milestone 10 — Combat clarity, hints, and results

October 3, 2026

## Changes

- Rival abilities now announce their type, target column when applicable, and a two-second countdown before taking effect. A fifteen-second opening grace and ten-second cooldown after an ability lands prevent overlapping interruptions. Existing Fog limits remain two seconds of foundation-only obscuring, a twenty-second cooldown, and opening protection.
- The board shows remaining Lock/Fog time and Block's remaining moves and maximum duration. Pausing, reading hints, viewing help, targeting an ability, or leaving the active battle freezes the warning with the rest of the match. Warnings and combat totals survive a checkpoint restore.
- Incoming feedback distinguishes actual HP lost from shield absorbed. Player damage feedback uses the same accounting, including overkill clamping. Energy and shield gains continue to report actual credited amounts.
- Hint pauses the match and explains a route without moving a card or spending energy. Its bounded search checks tableau runs, waste/foundations, the full draw-one stock/recycle cycle, and short rearrangements including foundation returns. It avoids equivalent king-column swaps and immediate foundation-return loops. Two-step routes include the next move.
- Temporary disruptions get a waiting explanation. When no useful route is found, the player can resume or choose New Battle, with the existing replacement confirmation. Recycling checks for a useful route; an empty stock/waste opens assistance. A failed search does not claim the deal is unwinnable or automatically end the match.
- Victory, defeat, and draw screens show HP damage dealt, rival shield damage, damage blocked by the player's shield, longest combo, current foundation cards out of 52, and energy earned. Rematch, Title Screen, and Change Loadout work from the result panel. Career results are recorded once.
- The tutorial mentions attack warnings and hints. Android version is 0.10.0, version code 10.

## Design and rendered review

Preserved the established user-supplied fantasy direction: navy panels, gold/steel frames, emerald felt, existing character paintings, Cinzel and Cormorant Garamond. These are operational game states, so clarity and recovery govern the layout. No alternate identity or generated artwork was needed.

Reviewed actual Unity renders at 390 × 844 and 1280 × 1920 for warnings, hints, no-progress options, and results. Warning and result text was enlarged after the first phone render. The existing suite also captures tablet and inset-safe-area layouts. Representative text/surface contrast ranges from 9.37:1 to 16.11:1; `contrast.json` records sampled pixels and foreground values. This is not an exhaustive accessibility certification.

The Hint, Rematch, and Title Screen buttons pass actual Unity raycast/pointer-click dispatch at simulated phone size, with measured touch targets at least 44 pixels. The harness checks dialog focus, explicit result navigation, hint replacement/cancel/resume, text fit, reduced-motion warning visibility, and fresh-match statistics. Existing pause and replacement navigation remains intact.

## Validation

Unity 6000.6.3f1 compiled the changes and completed `FrontEndReview.Run` with `MOBILE_REVIEW_PASS`, `PHONE_FEEDBACK_REVIEW_PASS`, `BATTLE_POLISH_REVIEW_PASS`, and `FRONT_END_REVIEW_PASS`.

New deterministic checks cover warning delay and target, no early energy charge, opening grace, cooldown boundary, no overlapping or post-result attack, paused warning, disk round trip, legacy saves without new fields, independent snapshot objects, shield-only damage, HP damage, overkill, peak combo, hint immutability, reachable buried waste, stock hints, foundation returns, blocked routes, Fog, reversible loops, a complete no-progress fixture, all three results, rematch/title pointer routing, and career recording exactly once. Forty fresh-deal hint probes took 10 ms total in the editor; this is not a phone benchmark.

The legacy regression found that the new timer guard initially skipped winner evaluation on a zero-time tick. Corrected it so zero-time ticks still evaluate a completed match, then reran the full suite. Existing card-drop, suit, ace-slot, tutorial, quit, lifecycle, corrupt-save recovery, and AI continuity tests pass.

Evidence is in `Review-M10/`. APK identity, size, hash, and signing verification are recorded separately in `APK-VERIFICATION.txt` after packaging.

## Limits and next phone test

Hints search up to three moves and 240 board states; they are assistance, not a complete solitaire solver or a guarantee of a winning deal. Stock lookahead checks whether recycling offers progress without naming hidden cards. Old saves restore safely, but damage/energy totals from before this version cannot be reconstructed and start at zero; saved peak combo is retained.

No physical Android touch, frame-rate, audio/haptic, or human balance playtest was performed. The user will install the APK and test. The largest remaining visual weakness is compact legacy HUD copy on a small phone; the new warnings and result copy are larger. No new general drag-target highlighting or unrelated progression features were added.

Google Drive delivery remains pending identification of the user's intended “It's Ron DG” destination. This APK is provided locally and must not be uploaded to the previously used account.
