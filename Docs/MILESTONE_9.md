# Milestone 9 — first phone-playtest fixes

October 3, 2026

## Feedback addressed

The owner reported frequent blackout effects, rejected drops at the bottom of a long stack, missing card suits on Android, fixed ace destinations, no way to quit a match, a tutorial that did not match the game, and no title screen. “Quit” means ending the current battle while keeping the app open.

- **Readable cards:** suits now use vector meshes instead of font glyphs. Each face-up card shows a suit beside its upper rank, a lower suit, and a large center suit on number cards. Court paintings remain. Upper suit marks stay visible in ordinary overlapping runs.
- **Drops along a column:** cards forward drops to their own tableau column or foundation; empty lane space accepts drops too. A drag-end geometry fallback handles a missed drop event. An attempted illegal move returns to its original pile without trying a second destination.
- **Flexible foundations:** any ace can begin any empty foundation position. Its suit binds that position until emptied; subsequent cards ascend in that suit. The player's chosen positions are saved and restored. Old checkpoints without a layout assign their existing foundations automatically.
- **Less intrusive Fog:** new matches have a 15-second opening grace period. Fog lasts two seconds, affects only foundation card visibility, and cannot hit the same side again for 20 seconds. Tableau and waste remain readable and playable. The AI chooses Fog less often, and the ability displays its remaining cooldown. Cooldowns persist in checkpoints. Legacy saves can finish an already-active older Fog timer, which was at most four seconds.
- **Quit Battle:** Pause includes an explicit Quit Battle action. Confirmation offers Keep Current Battle first. Confirming removes the unfinished checkpoint and returns to the title screen; it does not close the app or alter career stats. Save & Loadout remains the way to leave a match for later.
- **Title and help:** startup opens a castle-and-portrait title screen with Play, Continue, How to Play, and Settings. The tutorial uses the game's navy panels, gold frames, Cinzel headings, Cormorant body text, and drawn suit symbols. It has Back, Next/Done, and Close controls with contained keyboard navigation.

## Verification

`FrontEndReview.Run` includes `MobileReview.Run` and `PhoneFeedbackReview.Run`. The phone-feedback harness exercises portrait-viewport raycasts at a king-through-five stack, drops on the stack and below its last card, the drag-end fallback, multi-card runs, same-color rejection, every ace in every foundation position, non-ace and wrong-suit rejection, saved foundation order, Fog grace/duration/cooldown and energy preservation, readable tableau during Fog, quit cancellation, confirmed quit, and title/tutorial navigation.

Rendered review covers 390 × 844 phone and 1280 × 1920 desktop portrait views, all four tutorial pages, the title screen, all suits, a long run after a successful drop, Fog, and the quit confirmation. The title lettering and tutorial body were enlarged after reviewing the first renders. Evidence is retained in `Review-M9/`.

The final run passed `MOBILE_REVIEW_PASS`, `PHONE_FEEDBACK_REVIEW_PASS`, and `FRONT_END_REVIEW_PASS`. Representative contrast on rendered surfaces: tutorial body 16.02:1, quit heading 11.15:1, title gold 10.55:1, red card suits 5.80:1, and black card suits 15.09:1. American English checks passed for all changed source and documentation files. The existing editor SearchDatabase startup exception is still separate from these passing checks.

The automated pointer test uses a portrait render camera to keep card bounds and pointer coordinates in the same viewport. It is not a physical Android touch test. The owner should retest actual finger placement and Android suit visibility using `PHONE-PLAYTEST.md`. Secondary battle HUD text remains compact at phone width; this pass concentrates on card readability and the reported interactions.

The save schema remains version 1 with optional added layout/cooldown fields for backward compatibility. Core solitaire suit/order rules are unchanged. No cloud save or online match behavior is introduced.

## Android build

Version 0.9.0 (9), package `com.rgdevelops.battlesolitaire`, ARM64, minimum Android API 26, IL2CPP, development signing. Build with **Battle Solitaire > Android > Build Play-Test APK** or `BattleSolitaire.EditorTools.BattleSolitaireAndroidBuild.BuildBatch`. The default output is `Builds/Android/BattleSolitaire-M9-Playtest.apk`.

The APK build passed. `aapt` confirmed version 0.9.0 (9), minimum API 26, target API 36, and ARM64. `apksigner` verified its v2 signature; the certificate matches 0.8.0 so it can update the existing installation. File size and SHA-256 are retained in `Review-M9/APK-VERIFICATION.txt`.

The prior APK was uploaded to the wrong connected Google Drive. Do not reuse that destination for this build; the requested “It's Ron DG” account/folder still needs to be identified.
