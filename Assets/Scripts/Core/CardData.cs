using System;

namespace Core
{
    public enum Suit { Clubs, Diamonds, Hearts, Spades }

    public enum Rank
    {
        Two = 2, Three, Four, Five, Six, Seven, Eight, Nine, Ten,
        Jack = 11, Queen = 12, King = 13, Ace = 14
    }

    [Serializable]
    public class CardData
    {
        public Suit suit;
        public Rank rank;

        public CardData(Suit suit, Rank rank)
        {
            this.suit = suit;
            this.rank = rank;
        }

        public override string ToString()
        {
            return $"{rank} of {suit}";
        }
    }
}