# Milestone 7.1 — Portrait front end

October 3, 2026

## Direction and implementation

The supplied Option C brief is the selected direction. This operational game menu lets the player compare Vesper, Kael, and Aldric, inspect a passive, select rival difficulty, review career progress, and launch a battle. No alternative identity study was needed because the user supplied the layout and palette.

The menu now follows the requested header, active battler, three battler cards, three loadout tiles, and career/Battle hierarchy. The existing navy, gold, cyan, and crimson palette and built-in LegacyRuntime font remain. No new font or replacement art was added. The title uses gold and silver as explicitly requested. The crest is a small crop of the emblem in the existing AppCrest asset; its clipped lettering is excluded.

Hero and selector art use 4:5 containers with AspectFillRawImage center crops. Both HUD portraits use the same crop component. It responds to texture changes, rectangle changes, and texture-size changes, and guards missing textures and zero-size rectangles. Source images and their existing metadata are unchanged. The HUD combo label was shifted left so it no longer covers the player portrait.

Panels use a single outline. Selection has an ACTIVE marker or difficulty underline as well as color. Buttons have immediate press/focus feedback and explicit keyboard navigation within the menu. The Battle button uses a crimson vertex gradient and restrained shadow. The menu adds no animation or motion-dependent content. Ability icons are simple UI geometry; they do not rely on platform emoji support.

Portrait orientation is enforced in saved project settings. The old settings allowed all rotations despite the runtime controller requesting portrait. The 1080x1920 canvas reference, safe-area component, gameplay systems, and Android build utility are preserved.

## Rendered evidence and corrections

Screenshots were rendered from the actual runtime Unity UI in an isolated copy with Unity 6000.6.3f1, using a camera/RenderTexture at each target size. These are not browser mockups or physical-device screenshots.

| View/state | Evidence | Review |
| --- | --- | --- |
| Vesper, 1080x1920 | [Menu](Review-M7.1/vesper-1080x1920.png) | Hero, selector, loadout, career, and Battle hierarchy; undistorted portrait. |
| Kael, 1080x1920 | [Menu](Review-M7.1/kael-1080x1920.png) | Selected marker, title, quote, and passive update. |
| Aldric, 1080x1920 | [Menu](Review-M7.1/aldric-1080x1920.png) | Long passive remains readable. |
| Phone, 390x844 | [Menu](Review-M7.1/phone-390x844.png) | Portrait layout retained. |
| Tablet/desktop render, 1200x1600 | [Menu](Review-M7.1/tablet-1200x1600.png) | Portrait containers retain their shape on a wider portrait surface. |
| Tall phone with simulated top/bottom insets, 1080x2340 | [Safe area](Review-M7.1/phone-safe-area.png) | Standard originally wrapped; its button was widened and recaptured. |
| Missing hero image | [Fallback](Review-M7.1/missing-portrait.png) | Readable portrait-unavailable fallback; menu remains usable. |
| Battle HUD | [HUD](Review-M7.1/battle-hud.png) | Both character crops retain proportions; combo text moved off portrait. |
| Return after win | [Career](Review-M7.1/career-after-win.png) | Updated win/RP data appears after returning from results. |

Visual judgment: the hierarchy and portrait proportions now match the brief. The largest remaining visual weakness is source-art resolution: all three portraits are only 96x117 pixels. AppCrest is 72x72 and CardBack is 72x101. The enlarged portraits remain visibly soft; this task preserves those assets rather than generating substitutes.

## Verification

- Unity editor and runtime assemblies compiled without C# errors, including the unchanged Android build script.
- Crop tests cover portrait, landscape, and tall sources across portrait, square, and wide destinations; resulting UVs preserve aspect ratio and stay within texture bounds. Empty dimensions return safe UVs.
- All three battlers persist when read through a fresh BattleProfile; active names and passive text update.
- All three difficulties persist. Difficulty labels fit on one line in captured menu sizes.
- Pointer-click dispatch selects battlers; keyboard move dispatch stays inside the menu; keyboard submit launches Battle.
- Battle creates a running match. Tutorial opens and closes. A simulated win records stats, and the result Loadout button returns to the updated menu.
- Lock and Block open manual targeting without spending energy; invalid targets preserve energy; valid targets spend the correct amount.
- No detected menu text-height overflow or reference-space button height below 70 units. The smallest difficulty control is about 86 units high at 1080x1920. No physical touchscreen usability test was performed.
- Core rules, AI, progression, card drag/drop, manual targeting implementation, and existing art were left unchanged. Drag/drop was checked by source preservation, not an end-to-end gesture test.
- Normal/focused difficulty text contrast is 5.53:1 / 4.72:1. Gold on the background is 9.99:1, muted body text on the panel is 7.68:1, and ordinary control borders against their fill are 3.48:1. Rendered Battle gradient samples give 4.68:1–8.80:1 for its large label. See [measurements](Review-M7.1/contrast.json).
- The new menu uses immediate feedback, with no animated layout or motion-gated content. Device-level reduced-motion settings and existing battle effects were not tested.

The automated play-mode review backs up and restores the profile keys it exercises. See [verification output](Review-M7.1/verification.txt). The editor review helper is batch-only and should run in an isolated copy:

```text
Unity.exe -batchmode -projectPath <isolated-copy> -executeMethod BattleSolitaire.EditorTools.FrontEndReview.Run -reviewOutput <evidence-folder> -logFile <review-log>
```

Do not pass -quit: the helper exits after its play-mode checks. Without -reviewOutput it writes to Temp/FrontEndReview.

## Limitations

Android Build Support, SDK/NDK, and OpenJDK are absent from this Unity installation, so no APK was built or tested. The build utility compiled but its Android packaging path remains unverified. Native iOS/Android safe-area behavior and touch interaction require device testing.

Unity's editor SearchDatabase startup indexing emitted an ArgumentOutOfRangeException during batch review. Its stack is entirely within UnityEditor.Search; the play-mode assertions completed and emitted FRONT_END_REVIEW_PASS with exit code 0. Existing FindFirstObjectByType deprecation warnings remain in BattleFeedback and BattleGameController. No new gameplay exception was observed.
