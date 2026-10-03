using System;
using System.Collections.Generic;

namespace BattleSolitaire.Core
{
    public sealed class SolitaireGame
    {
        public const int CardCount = 52;

        public List<CardState> Stock { get; } = new List<CardState>();
        public List<CardState> Waste { get; } = new List<CardState>();
        public List<List<CardState>> Tableau { get; } = new List<List<CardState>>();
        public Dictionary<Suit, List<CardState>> Foundations { get; }
            = new Dictionary<Suit, List<CardState>>();

        public int Seed { get; }

        public SolitaireGame(int seed)
        {
            Seed = seed;

            foreach (Suit suit in Enum.GetValues(typeof(Suit)))
                Foundations[suit] = new List<CardState>();

            for (int i = 0; i < 7; i++)
                Tableau.Add(new List<CardState>());

            Deal(seed);
        }

        private static List<CardState> CreateDeck()
        {
            var deck = new List<CardState>(CardCount);

            foreach (Suit suit in Enum.GetValues(typeof(Suit)))
            {
                for (int rank = 1; rank <= 13; rank++)
                    deck.Add(new CardState(suit, (Rank)rank));
            }

            return deck;
        }

        private static void Shuffle(List<CardState> deck, int seed)
        {
            var random = new Random(seed);

            for (int i = deck.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                CardState temp = deck[i];
                deck[i] = deck[j];
                deck[j] = temp;
            }
        }

        private void Deal(int seed)
        {
            List<CardState> deck = CreateDeck();
            Shuffle(deck, seed);

            int deckIndex = 0;

            for (int column = 0; column < 7; column++)
            {
                for (int row = 0; row <= column; row++)
                {
                    CardState card = deck[deckIndex++];
                    card.IsFaceUp = row == column;
                    Tableau[column].Add(card);
                }
            }

            while (deckIndex < deck.Count)
            {
                CardState card = deck[deckIndex++];
                card.IsFaceUp = false;
                Stock.Add(card);
            }
        }

        public bool DrawCard()
        {
            if (Stock.Count == 0)
                return RecycleWaste();

            CardState card = Stock[Stock.Count - 1];
            Stock.RemoveAt(Stock.Count - 1);
            card.IsFaceUp = true;
            Waste.Add(card);

            return true;
        }

        private bool RecycleWaste()
        {
            if (Waste.Count == 0)
                return false;

            for (int i = Waste.Count - 1; i >= 0; i--)
            {
                CardState card = Waste[i];
                card.IsFaceUp = false;
                Stock.Add(card);
            }

            Waste.Clear();
            return true;
        }

        public MoveResult MoveWasteToTableau(int destinationColumn)
        {
            if (Waste.Count == 0 || !ValidColumn(destinationColumn))
                return MoveResult.Failed();

            CardState card = Waste[Waste.Count - 1];
            List<CardState> destination = Tableau[destinationColumn];

            bool valid = destination.Count == 0
                ? SolitaireRules.CanPlaceOnEmptyTableau(card)
                : SolitaireRules.CanPlaceOnTableau(card, destination[destination.Count - 1]);

            if (!valid)
                return MoveResult.Failed();

            Waste.RemoveAt(Waste.Count - 1);
            destination.Add(card);

            return MoveResult.Succeeded(MoveKind.WasteToTableau);
        }

        public MoveResult MoveWasteToFoundation()
        {
            if (Waste.Count == 0)
                return MoveResult.Failed();

            CardState card = Waste[Waste.Count - 1];
            List<CardState> foundation = Foundations[card.Suit];

            if (!SolitaireRules.CanPlaceOnFoundation(card, foundation))
                return MoveResult.Failed();

            Waste.RemoveAt(Waste.Count - 1);
            foundation.Add(card);

            return MoveResult.Succeeded(
                MoveKind.WasteToFoundation,
                foundationMove: true);
        }

        public MoveResult MoveTableauToFoundation(int sourceColumn)
        {
            if (!ValidColumn(sourceColumn))
                return MoveResult.Failed();

            List<CardState> source = Tableau[sourceColumn];

            if (source.Count == 0)
                return MoveResult.Failed();

            CardState card = source[source.Count - 1];
            List<CardState> foundation = Foundations[card.Suit];

            if (!SolitaireRules.CanPlaceOnFoundation(card, foundation))
                return MoveResult.Failed();

            source.RemoveAt(source.Count - 1);
            foundation.Add(card);

            bool revealed = RevealTopCard(source);
            bool cleared = source.Count == 0;

            return MoveResult.Succeeded(
                MoveKind.TableauToFoundation,
                revealedHiddenCard: revealed,
                clearedColumn: cleared,
                foundationMove: true);
        }

        public MoveResult MoveFoundationToTableau(Suit sourceSuit, int destinationColumn)
        {
            if (!ValidColumn(destinationColumn))
                return MoveResult.Failed();

            List<CardState> foundation = Foundations[sourceSuit];

            if (foundation.Count == 0)
                return MoveResult.Failed();

            CardState card = foundation[foundation.Count - 1];
            List<CardState> destination = Tableau[destinationColumn];

            bool valid = destination.Count == 0
                ? SolitaireRules.CanPlaceOnEmptyTableau(card)
                : SolitaireRules.CanPlaceOnTableau(card, destination[destination.Count - 1]);

            if (!valid)
                return MoveResult.Failed();

            foundation.RemoveAt(foundation.Count - 1);
            destination.Add(card);

            return MoveResult.Succeeded(MoveKind.FoundationToTableau);
        }

        public MoveResult MoveTableauToTableau(
            int sourceColumn,
            int startIndex,
            int destinationColumn)
        {
            if (!ValidColumn(sourceColumn) ||
                !ValidColumn(destinationColumn) ||
                sourceColumn == destinationColumn)
            {
                return MoveResult.Failed();
            }

            List<CardState> source = Tableau[sourceColumn];
            List<CardState> destination = Tableau[destinationColumn];

            if (!SolitaireRules.IsValidTableauSequence(source, startIndex))
                return MoveResult.Failed();

            CardState movingCard = source[startIndex];

            bool valid = destination.Count == 0
                ? SolitaireRules.CanPlaceOnEmptyTableau(movingCard)
                : SolitaireRules.CanPlaceOnTableau(
                    movingCard,
                    destination[destination.Count - 1]);

            if (!valid)
                return MoveResult.Failed();

            int moveCount = source.Count - startIndex;
            List<CardState> movingCards = source.GetRange(startIndex, moveCount);

            source.RemoveRange(startIndex, moveCount);
            destination.AddRange(movingCards);

            bool revealed = RevealTopCard(source);
            bool cleared = source.Count == 0;

            return MoveResult.Succeeded(
                MoveKind.TableauToTableau,
                revealedHiddenCard: revealed,
                clearedColumn: cleared);
        }

        private static bool RevealTopCard(List<CardState> column)
        {
            if (column.Count == 0)
                return false;

            CardState top = column[column.Count - 1];

            if (top.IsFaceUp)
                return false;

            top.IsFaceUp = true;
            return true;
        }

        private bool ValidColumn(int column)
        {
            return column >= 0 && column < Tableau.Count;
        }

        public int GetFoundationCardCount()
        {
            int count = 0;

            foreach (KeyValuePair<Suit, List<CardState>> pair in Foundations)
                count += pair.Value.Count;

            return count;
        }

        public bool IsVictory() => GetFoundationCardCount() == CardCount;

        public float GetClearPercentage()
        {
            return GetFoundationCardCount() / (float)CardCount;
        }
    }
}
