using System.Collections.Generic;

namespace BattleSolitaire.Core
{
    public static class SolitaireRules
    {
        public static bool CanPlaceOnTableau(CardState movingCard, CardState destinationCard)
        {
            if (movingCard == null || destinationCard == null)
                return false;

            if (!movingCard.IsFaceUp || !destinationCard.IsFaceUp)
                return false;

            bool alternatingColor = movingCard.IsRed != destinationCard.IsRed;
            bool descending = (int)movingCard.Rank == (int)destinationCard.Rank - 1;

            return alternatingColor && descending;
        }

        public static bool CanPlaceOnEmptyTableau(CardState card)
        {
            return card != null && card.IsFaceUp && card.Rank == Rank.King;
        }

        public static bool CanPlaceOnFoundation(CardState card, IReadOnlyList<CardState> foundation)
        {
            if (card == null || !card.IsFaceUp)
                return false;

            if (foundation.Count == 0)
                return card.Rank == Rank.Ace;

            CardState top = foundation[foundation.Count - 1];

            return card.Suit == top.Suit &&
                   (int)card.Rank == (int)top.Rank + 1;
        }

        public static bool IsValidTableauSequence(IReadOnlyList<CardState> column, int startIndex)
        {
            if (column == null || startIndex < 0 || startIndex >= column.Count)
                return false;

            for (int i = startIndex; i < column.Count; i++)
            {
                if (!column[i].IsFaceUp)
                    return false;

                if (i == column.Count - 1)
                    continue;

                CardState current = column[i];
                CardState next = column[i + 1];

                bool descending = (int)next.Rank == (int)current.Rank - 1;
                bool alternating = current.IsRed != next.IsRed;

                if (!descending || !alternating)
                    return false;
            }

            return true;
        }
    }
}
