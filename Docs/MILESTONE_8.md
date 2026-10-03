# Milestone 8 — phone-ready battle experience

October 3, 2026

## Player changes

- Pause stops both boards, AI decisions, combo windows, and disruption timers. Android Back or desktop Escape opens the pause menu; returning from the background requires an explicit resume.
- Sound, vibration, and reduced-motion preferences persist locally. Reduced motion removes card-hover interpolation, health-bar interpolation, combat flashes, and banner scaling while preserving readable messages.
- Save & Loadout and Continue preserve an unfinished local battle. A new battle requires confirmation before replacing a saved match. Career records remain separate.
- Checkpoints include both complete decks, face-up states, resources, combos, disruptions, battlers, difficulty, and AI scheduling/random state. Writes occur after player actions, on pause/backgrounding, and approximately every two active seconds.
- Checkpoints use a versioned JSON schema, validation, atomic replacement, and a previous-copy backup. Invalid or unsupported saves fail safely. A disk error displays a recovery notice; the current session can still continue in memory.
- Larger pause/help/ability controls, DPI-aware drag thresholds, a finger-offset drag preview, canceled drags on backgrounding, and long-stack containment improve phone handling. The seven-column solitaire layout remains intentionally dense.

## Build

Unity 6000.6.3f1; Android package `com.rgdevelops.battlesolitaire`; version 0.8.0 (8); ARM64; minimum API 26; IL2CPP; development/debug signing. This is a sideloaded playtest APK, not a store release.

Use **Battle Solitaire > Android > Build Play-Test APK**, or run Unity in batch mode with:

```
-batchmode -quit -projectPath <project> -buildTarget Android -executeMethod BattleSolitaire.EditorTools.BattleSolitaireAndroidBuild.BuildBatch -apkOutput <absolute-apk-path>
```

The batch command creates the playable scene if absent. Optional `-androidToolsRoot <folder>` selects a folder containing `SDK`, `NDK`, and `OpenJDK`. This workstation uses `C:/Users/ronal/Documents/Codex/android-m8`, populated from the official Unity module downloads. Unity's Android Build Support installation was repaired after an incomplete installation; no runtime files were substituted from another Unity version.

## Verification and review

`FrontEndReview.Run` includes `MobileReview.Run`. Run it in an isolated project copy with `-batchmode -executeMethod BattleSolitaire.EditorTools.FrontEndReview.Run -reviewOutput <folder>`; omit `-quit` because the harness exits after play-mode checks. Batch saves use a process-specific temporary folder to protect player data, and the harness restores preferences.

Checks cover paused input and timer invariance, settings persistence, confirmation cancellation, menu selection isolation from the checkpoint, disk deserialization into a fresh match, AI scheduling continuity, simulated background drag cancellation, explicit resume, corrupt-primary backup recovery, corrupt/future/duplicate-card rejection, and long-stack bounds. Existing menu, battler, difficulty, keyboard, targeting, drag, result, and career regression checks remain included.

Rendered states include phone and tablet pause screens, reduced-motion settings, replacement confirmation, Continue on the loadout, and a long-stack phone board, alongside the existing reference-design views. The confirmation layout was tightened after rendered review. Old card views are disabled before deferred destruction to prevent duplicate frames and stale raycast targets.

The final editor run passed both `MOBILE_REVIEW_PASS` and `FRONT_END_REVIEW_PASS`, including explicit keyboard navigation contained within the pause dialog and initial focus on Keep Current Battle. Representative contrast measurements on the rendered phone pause screen are 11.09:1 for the gold title, 15.94:1 for silver body copy, 15.91:1 for button labels, and 11.36:1 for the save status. This is a representative review, not an exhaustive accessibility certification. Evidence is retained in `Review-M8/`.

## Limits

Physical Android testing is assigned to the owner. Editor event dispatch and simulated lifecycle callbacks do not prove physical touch behavior, Android process-death recovery, thermal behavior, frame rate, or battery life. Follow `PHONE-PLAYTEST.md` on the actual device. Very small exposed card strips remain below a 44-pixel touch target because the board shows seven overlapping columns. Secondary copy remains compact on narrow screens.

The existing Unity SearchDatabase indexing exception and obsolete-object-search warnings may appear during editor startup. They are distinct from the play-mode assertions and APK build result. Save recovery is local to the device and does not provide cloud synchronization.
