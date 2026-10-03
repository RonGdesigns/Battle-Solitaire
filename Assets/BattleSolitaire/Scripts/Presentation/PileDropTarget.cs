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

            if (_kind == DropTargetKind.Tableau)
                _board.TryMoveCardToTableau(dragged, _column);
            else
                _board.TryMoveCardToFoundation(dragged, _suit);

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
