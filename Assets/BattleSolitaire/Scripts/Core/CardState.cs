using System;

namespace BattleSolitaire.Core
{
    public enum Suit
    {
        Clubs,
        Diamonds,
        Hearts,
        Spades
    }

    public enum Rank
    {
        Ace = 1,
        Two,
        Three,
        Four,
        Five,
        Six,
        Seven,
        Eight,
        Nine,
        Ten,
        Jack,
        Queen,
        King
    }

    [Serializable]
    public sealed class CardState
    {
        public Suit Suit { get; }
        public Rank Rank { get; }
        public bool IsFaceUp { get; set; }

        public CardState(Suit suit, Rank rank, bool isFaceUp = false)
        {
            Suit = suit;
            Rank = rank;
            IsFaceUp = isFaceUp;
        }

        public bool IsRed => Suit == Suit.Hearts || Suit == Suit.Diamonds;
        public string Id => $"{Rank}_{Suit}";

        public override string ToString() => $"{Rank} of {Suit}";
    }
}
