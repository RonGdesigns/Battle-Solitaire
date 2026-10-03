# Battle Solitaire 0.8.0 — phone playtest

This development APK targets Android 8.0 or later on ARM64 devices. It is a local player-versus-AI build.

## Install

Download `BattleSolitaire-M8-Playtest.apk` from your Google Drive to your phone, open it, and allow installation from that download app if Android asks. Launch Battle Solitaire. Keep an existing installation when Android offers an update; uninstalling removes its local career and saved battle.

## Ten-minute check

1. Choose a battler and difficulty, then start a battle. Check the top and bottom edges around your camera cutout and gesture bar.
2. Draw cards, tap a legal move, and drag cards between columns. Try a long stack and a rejected move. Cards should return cleanly after an invalid drop.
3. Open **Pause**. Wait ten seconds. Neither opponent actions nor battle timers should advance. Resume and confirm play continues.
4. Try **Sound**, **Vibration**, and **Reduced Motion**. Reopen settings to check that your choices remain.
5. Start dragging a card, then switch to another app. Return: the drag should be canceled and the battle should be paused.
6. Choose **Save & Loadout**, change the selected battler, then choose **Continue**. The original battle, battler, cards, HP, energy, and disruptions should return paused.
7. While paused, close the app from the recent-apps screen. Reopen it and choose **Continue**, then **Resume Battle**. Your checkpoint should return. Active play also checkpoints approximately every two seconds.
8. With a saved battle available, start a new battle. Choose **Keep Current Battle** first; then repeat and choose **Replace and Start**. Confirm the first action preserves your match and the second replaces it.
9. Finish a battle and check that career progress updates once. A completed battle should no longer appear under Continue.

Please report your phone model, Android version, the step that failed, and a screenshot or short recording if possible. Note any small text, difficult targets, stuttering, excessive heat, or battery drain during a 10–15 minute session.

Editor checks cover pause/input gating, settings, checkpoint serialization and disk recovery, AI scheduling continuity, corrupt-save recovery, and simulated background callbacks. Physical touch, Android app lifecycle behavior, device performance, and battery use still need this phone test.
