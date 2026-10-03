using BattleSolitaire.Core;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BattleSolitaire.Presentation
{
    public enum DropTargetKind
    {
        Tableau,
        Foundation
    }

    public sealed class PileDropTarget :
        MonoBehaviour,
        IDropHandler,
        IPointerClickHandler
    {
        private BattleBoardView _board;
        private DropTargetKind _kind;
        private int _column;
        private Suit _suit;

        public void Initialize(
            BattleBoardView board,
            DropTargetKind kind,
            int column,
            Suit suit)
        {
            _board = board;
            _kind = kind;
            _column = column;
            _suit = suit;
        }

        public void OnDrop(PointerEventData eventData)
        {
            CardView dragged = CardView.CurrentDrag;
            if (dragged == null)
                return;

            bool moved;

            if (_kind == DropTargetKind.Tableau)
            {
                moved = _board.TryMoveCardToTableau(
                    dragged,
                    _column);
            }
            else
            {
                moved = _board.TryMoveCardToFoundation(
                    dragged,
                    _suit);
            }

            // Only suppress CardView's snap-back when the rules engine
            // actually accepted the move. Previously every drop onto a
            // pile target was marked handled, which let an illegal card
            // remain visually parked between cards even though the game
            // state had rejected the placement.
            if (moved)
                dragged.MarkDropHandled();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_kind == DropTargetKind.Tableau)
                _board.EmptyTableauTapped(_column);
            else
                _board.FoundationSlotTapped(_suit);
        }
    }
}
