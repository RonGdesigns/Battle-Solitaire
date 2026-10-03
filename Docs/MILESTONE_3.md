# Milestone 3 — Playable Unity Prototype

Milestone 3 connects the pure solitaire and battle rules to a Unity 6.6 portrait interface and a functioning local AI opponent.

## Included

- self-bootstrapping runtime controller,
- portrait 1080x1920 reference layout,
- generated card visuals with no external art dependency,
- stock, waste, tableau, and foundation rendering,
- tap-to-select controls,
- drag-and-drop tableau/foundation controls,
- double-tap exposed cards to send them to foundation,
- player HP, shield, energy, combo, and opponent progress HUD,
- Lock, Fog, and Blocker attack buttons,
- visible disruption overlays,
- match result overlay and rematch button,
- AI solitaire move solver,
- AI attack timing and energy spending,
- Unity Editor setup command that generates the Battle scene and enables it for builds.

## Run it

1. Clone the repository.
2. Add/open the repository folder in Unity Hub using Unity 6000.6.3f1.
3. Let Unity import and compile.
4. Choose **Battle Solitaire > Setup Playable Prototype**.
5. Press **Play**.

The setup command creates:

`Assets/BattleSolitaire/Scenes/Battle.unity`

and adds it to the build scene list.

## Prototype controls

### Stock
Tap **DRAW** to reveal the next waste card. When stock is exhausted, the control changes to **RECYCLE**.

### Tap movement
Tap a face-up source card, then tap the destination tableau lane or foundation.

### Drag movement
Drag an exposed card or valid face-up sequence to another tableau lane or the matching foundation.

### Fast foundation
Double-tap an exposed tableau or waste card to attempt an automatic foundation move.

### Attacks
- **LOCK 25** targets the rival's largest currently targetable tableau lane for 3 seconds.
- **FOG 25** hides the rival's visible card information at the rules/presentation boundary.
- **BLOCK 50** obstructs the rival's largest targetable lane.

The AI uses the same battle energy rules and attacks the player. When the AI is Fogged, it loses reliable card information: its move cadence slows and it skips some move opportunities until Fog expires.

## AI behavior

The first move solver prioritizes:

1. tableau moves that reveal hidden cards,
2. legal foundation progress,
3. useful tableau rearrangement,
4. waste-to-tableau moves,
5. drawing/recycling stock.

It deliberately avoids moving a complete visible stack from one empty lane to another so it does not enter an obvious no-progress loop.

## Visual scope

This milestone intentionally uses generated placeholder card visuals. It proves the full interaction and battle loop before investing in final art, animation, sound, characters, or online networking.

## Known prototype limitations

- Card drag shows the lead card rather than a fully fanned dragged sequence.
- The AI is heuristic rather than search/solver optimal.
- No dead-deal detection or seed quality scoring yet.
- No audio or particle feedback yet.
- No safe-area/notch-specific layout pass yet.
- Attacks automatically select a strategic target rather than asking the player to manually target the rival board.

## Milestone 4 candidates

- final card/deck art direction,
- card movement tweening and hit effects,
- sound/haptics,
- onboarding tutorial,
- stronger seed solvability/fairness analysis,
- smarter AI difficulty levels,
- manual opponent targeting,
- Android build/installation pipeline,
- online 1v1 architecture.
