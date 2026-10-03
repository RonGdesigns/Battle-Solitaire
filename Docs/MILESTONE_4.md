# Milestone 4 — Game Feel and Android Play-Test Build

Milestone 4 turns the functional prototype into a better mobile play-test and adds a repeatable APK build path.

## Added

### Mobile presentation
- safe-area fitting for notches, camera cutouts, rounded corners, and gesture areas,
- smoother HP, energy, combo, and opponent-progress bars,
- card selection and drag scaling,
- card shadows,
- battle flash/banner feedback,
- combo callouts,
- incoming-damage callouts,
- attack callouts.

### Feedback
All audio in this milestone is generated at runtime, so the repository does not need external sound files yet.

- card move sound,
- foundation success sound,
- attack sound,
- hit sound,
- invalid move sound,
- victory/defeat sounds,
- mobile vibration feedback for meaningful battle moments.

### Tutorial
A four-step first-run quick-start overlay explains:
1. solitaire movement,
2. battle energy/shield/damage,
3. Lock/Fog/Blocker attacks,
4. win conditions.

The tutorial pauses the battle while open. It is shown once automatically and can be reopened with the **?** button.

### Android play-test build
New Unity menu items:

- **Battle Solitaire > Android > Prepare Android Settings**
- **Battle Solitaire > Android > Build Play-Test APK**

The build helper configures:
- package ID: `com.rgdevelops.battlesolitaire`,
- version: `0.4.0`,
- Android version code: `4`,
- portrait orientation,
- ARM64,
- IL2CPP,
- minimum Android API 26,
- APK output rather than App Bundle.

The play-test build is a development APK with debugging enabled.

Output:

`Builds/Android/BattleSolitaire-M4-Playtest.apk`

## Build requirements

Unity Hub must have the Android modules installed for Unity 6000.6.3f1:

- Android Build Support,
- Android SDK & NDK Tools,
- OpenJDK.

If those modules are missing, the build menu reports that instead of attempting the build.

## Current art strategy

Milestone 4 intentionally stays asset-light. The cards and interface are still generated with Unity UI so gameplay can be tested without blocking on final art production.

The next visual pass can replace the temporary card presentation with:
- final RG Develops visual identity,
- custom card faces and backs,
- animated attack VFX,
- character portraits,
- arena/table themes.

## Still intentionally deferred

- release keystore/signing,
- Google Play AAB release pipeline,
- production icons/splash screen,
- online multiplayer,
- ranked matchmaking,
- account/progression backend,
- final sound/music assets.

Those should happen after the local combat loop has been played enough to justify locking balance and presentation.
