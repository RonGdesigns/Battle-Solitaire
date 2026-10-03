# Battle Solitaire 0.9.0 — phone retest

Install `BattleSolitaire-M9-Playtest.apk` as an update to 0.8.0. Keep the existing installation so career progress and its saved match remain available.

1. Launch the app. It should start on the new title screen. Try How to Play, move backward and forward, and close it.
2. Start a battle. Confirm clubs, diamonds, hearts, and spades are visible on cards, including the exposed edges of overlapping cards.
3. Build a long run. Drop the next card directly on its bottom card, then try the space below the stack. Also drag a valid run of several cards and try an invalid same-color move.
4. Place aces into different empty foundation positions. Follow with the matching twos. Save & Loadout, then Continue: the same foundation positions should remain.
5. Watch Fog: it should leave the tableau and waste readable, hide only foundation cards for two seconds, and have a 20-second cooldown. New matches have a 15-second opening grace period.
6. Open Pause > Quit Battle. Choose Keep Current Battle first. Then repeat and confirm Quit Battle. The app should return to the title screen, remove that unfinished match, and keep your career stats.
7. Check that Save & Loadout still preserves a match, and that closing/reopening the app restores it through Continue.

Report your phone model, Android version, and any failed step. A short recording of a rejected drop is especially useful because it shows where your finger and card land.

Build and editor checks are documented separately. Physical Android touch, lifecycle, font appearance, performance, and battery behavior require this phone test.
