# Drag and Placement Rules

The board view is visual only. SolitaireGame remains the authoritative card state.

## Invalid drag behavior

A dragged card or sequence may only remain at its destination when the rules engine accepts the move.

If a card is dropped:
- between cards where it is not a legal continuation,
- on the wrong color/rank,
- into a non-King empty tableau lane,
- onto the wrong foundation,
- from inside a tableau sequence to a foundation,
- onto a locked or blocked lane,

the move returns false and the dragged visual snaps back to the board state.

PileDropTarget must never mark a drop as handled until BattleBoardView reports that the move succeeded.

This prevents visual card positions from disagreeing with the authoritative solitaire state.
