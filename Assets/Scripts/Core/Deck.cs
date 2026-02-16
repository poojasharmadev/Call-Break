using System;
using System.Collections.Generic;

namespace Core
{
    public class Deck
    {
        private List<CardData> cards = new List<CardData>();
        private System.Random rng = new System.Random();

        public int Count => cards.Count;

        public void Build52()
        {
            cards.Clear();
            foreach (Suit s in Enum.GetValues(typeof(Suit)))
            foreach (Rank r in Enum.GetValues(typeof(Rank)))
                cards.Add(new CardData(s, r));
        }

        public void Shuffle()
        {
            for (int i = cards.Count - 1; i > 0; i--)
            {
                int j = rng.Next(0, i + 1);
                (cards[i], cards[j]) = (cards[j], cards[i]);
            }
        }

        public CardData Draw()
        {
            if (cards.Count == 0) return null;
            CardData top = cards[0];
            cards.RemoveAt(0);
            return top;
        }
    }
}