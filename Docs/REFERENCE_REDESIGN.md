# Battle Solitaire reference redesign

October 3, 2026

The user supplied a battler-selection reference and a live solitaire-battle reference. This pass applies their dark fantasy direction to both runtime screens: prominent character paintings, clipped gold and steel frames, navy material panels, blue abilities, a red Battle action, and an emerald felt board. The supplied direction is the visual brief; no alternate identity study was needed.

## Implemented behavior

- The selected battler occupies the large card; the other two remain selectable beside it. Identity, passive, difficulty, rank, and career values come from the existing profile.
- The HUD retains live health, shield, energy, combo, opponent progress, messages, attack actions, tutorial, rematch, and return-to-loadout behavior.
- The board scales to its available width. Long tableau stacks compress their spacing to stay inside the table. Court cards use the corresponding battler artwork; number cards preserve clear ranks and suits.
- Portrait crops preserve image proportions and remain within the correct atlas region. Original artwork and its metadata are preserved as fallback resources.
- Frames and symbols are runtime UI geometry. Selection and keyboard focus have visible outlines. New button feedback is immediate; no decorative animation was added. Existing health interpolation and card motion remain.
- The mockups' invented store/deck-management tabs, example statistics, and different ability costs were not added. The existing 25/25/50 energy costs and attack semantics remain authoritative.

## Review and validation

Unity 6000.6.3f1 imported the fonts and textures and compiled the runtime and editor assemblies. `BattleSolitaire.EditorTools.FrontEndReview.Run` completed with `FRONT_END_REVIEW_PASS` in an isolated copy of the project.

The harness checked all three battler selections and difficulty persistence, passive updates, atlas containment and aspect ratios, keyboard navigation and submit, pointer selection, launching a match, tutorial opening, Lock/Block targeting, invalid-target recovery without energy loss, legal and illegal drag dispatch, match-result recording, return to Loadout, refreshed career values, portrait settings, and nonempty visible frame/icon meshes. Core, Battle, and AI source files were also hash-compared with the original project and remained unchanged. The final menu captures produced no text-overflow or small-target diagnostics under the harness's thresholds.

Rendered review covered 1080 × 1920, 390 × 844, 1200 × 1600, and a 1080 × 2340 inset-safe-area simulation, including all battlers, a missing-portrait fallback, live battle, court cards, and career values after a win. Corrections included missing CanvasRenderer components on custom graphics, font compatibility through static font weights, active-card text spacing on tablets, portrait atlas containment, and upper card-corner suit visibility.

Representative sRGB contrast measurements from the final screenshot surfaces: gold on navy 11.16:1; silver on navy 16.03:1; blue on navy 10.93:1; muted text on navy 11.23:1; gold on felt 8.54:1; silver on the red Battle button 14.82:1. These are representative surface checks, not an exhaustive accessibility certification.

Evidence retained in `Review-Reference/`: menu, phone menu, tablet menu, battle, `verification.txt`, and `contrast.json`. Additional captures are in the task's output folder.

### Remaining limitations

This is a functional Unity interpretation of the references, not a pixel-for-pixel reproduction. Runtime frame/icon geometry is simpler than the painted mockup ornament. Secondary copy is compact at 390-pixel width. Android Build Support is not installed in this Unity editor, so no APK or physical-device touch/performance test was performed. Pointer and drag checks use Unity event dispatch, not physical touch. No operating-system reduced-motion integration was tested; preexisting card and health transitions remain. The editor emitted a preexisting SearchDatabase indexing exception during startup; it did not prevent compilation or the passing play-mode review.

## Art and font provenance

Four raster resources were generated with the built-in image generation tool, using the user's supplied menu reference for character identity and visual direction. The source images were copied intact into `Assets/BattleSolitaire/Resources/Art/Premium/`; no external image service or CLI fallback was used. Their import settings preserve dimensions and clamp UV sampling. The new atlas is sampled in three equal vertical regions: Vesper, Kael, Aldric.

| Resource | Role |
| --- | --- |
| `BattlerAtlas.png` | Three character portraits and court-card paintings |
| `CastleBackdrop.png` | Castle backdrop and subdued panel material |
| `RoyalCardBack.png` | Navy and gold card back |
| `EmeraldFelt.png` | Green table material |

Cinzel (static weight 600) and Cormorant Garamond (static weight 500) are bundled under `Resources/Fonts/` with their original SIL Open Font License notices. Sources: [Google Fonts Cinzel](https://github.com/google/fonts/tree/main/ofl/cinzel) and [Google Fonts Cormorant Garamond](https://github.com/google/fonts/tree/main/ofl/cormorantgaramond). Static instances were produced from the variable fonts for predictable Unity font rendering.

## Generation prompts

### BattlerAtlas.png

Create ONE production game-art texture atlas, a clean horizontal triptych of three equal-width portrait paintings, edge to edge with NO gaps. Use the attached Battle Solitaire reference solely as the character identity and painting style reference. Landscape 1536x1024 output. Left third: Vesper, the same pale silver-haired woman in black/gold armor with a crimson gemstone, crimson-lit backdrop. Center third: Kael, the same young dark-haired clean-shaven male warrior with blue-black armor and blue lightning backdrop. Right third: Aldric, the same mature black-bearded king, gold crown, blue/gold armor and gray fur mantle, wintry blue-gray backdrop. Each portrait occupies its exact one-third vertical region; head centered horizontally, complete crown/hair visible with some headroom, head and shoulders/upper torso, face around 35% down from top. Preserve their recognizable faces, costume details, and premium illustrated fantasy realism from the reference. Detailed crisp faces and armor, restrained painterly texture, dramatic cinematic rim lighting. This atlas will be sampled as three independent portraits in a functioning Unity game. No UI, no cards, no frames, no text, no lettering, no symbols, no interface numbers. Do not reproduce the screenshot layout. Keep all three portraits equally sized.

### CastleBackdrop.png

Create ONE clean game background texture based on the attached Battle Solitaire interface's dark fantasy atmosphere. Output portrait 1024x1536. This is artwork to sit UNDER live Unity UI, NOT a screenshot or mockup. Upper quarter: moonlit gothic castle silhouettes in deep blue, mist, distant tiny warm torchlight, one subtle burgundy hanging banner. Most of the middle and lower image: extremely dark desaturated blue-black slate / aged navy leather, subtle realistic fine-grain weathering and faint angled metallic architectural shapes at the outer edges only. Center must remain very dark and calm for text and UI panels. Premium painted realistic fantasy card-battle atmosphere. NO text, NO logos, NO lettering, NO portraits, NO cards, NO buttons, NO frames, NO HUD, NO bars, NO green felt. Keep lighting low, almost black navy, with restrained cyan reflected light along the far edges.

### RoyalCardBack.png

Create one premium playing-card BACK texture for the Battle Solitaire game, matching the navy and gold card backs visible in the attached reference. Portrait format, straight-on flat orthographic full card image, no perspective or tilt. Rich very dark navy leather/paper with subtle intricate fine texture. Elegant thin embossed double gold border with clipped decorative corners and fine delicate gold filigree, central large faceted gold spade emblem. Symmetrical, restrained luxurious dark fantasy, crisp detail. The card fills the whole canvas edge to edge, no outer background, no cast shadow. No text, no lettering, no numbers, no characters, no other cards, no UI. This will be a reusable live card texture.

### EmeraldFelt.png

One flat seamless-looking emerald green felt material texture for a premium fantasy solitaire table. Portrait 1024x1536. Straight orthographic, perfectly flat. Deep forest green woven wool baize, visible extremely fine fibers and subtle natural mottled wear, brighter restrained emerald center and gently darker forest edges. No objects, no cards, no borders, no emblems, no symbols, no lettering, no grid. Realistic fine-grain material, low contrast, sufficient darkness for cream cards to stand out.
