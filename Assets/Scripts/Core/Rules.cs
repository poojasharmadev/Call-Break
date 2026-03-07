using System.Collections.Generic;

namespace Core
{
    public static class Rules
    {
        // User-requested rule:
        // 1) If you have a higher card of lead suit, you must play a higher card of lead suit.
        // 2) Else if you have any card of lead suit, you may play that suit.
        // 3) Else if you have spade, you must play spade.
        // 4) Else you may play anything.
        public static bool IsLegalMove(
            List<CardData> hand,
            CardData card,
            Suit? leadSuit,
            Dictionary<int, CardData> played)
        {
            // First player of trick can play anything
            if (leadSuit == null || played == null || played.Count == 0)
                return true;

            Suit lead = leadSuit.Value;

            CardData currentWinning = GetCurrentWinningCard(played, lead);

            bool hasLeadSuit = false;
            bool hasHigherLeadSuit = false;
            bool hasSpade = false;

            for (int i = 0; i < hand.Count; i++)
            {
                CardData h = hand[i];

                if (h.suit == lead)
                {
                    hasLeadSuit = true;

                    // higher card of same lead suit than current winning card
                    if (currentWinning != null &&
                        currentWinning.suit == lead &&
                        h.rank > currentWinning.rank)
                    {
                        hasHigherLeadSuit = true;
                    }
                }

                if (h.suit == Suit.Spades)
                    hasSpade = true;
            }

            // Rule 1: if player has a higher card of lead suit, must play higher lead suit
            if (hasHigherLeadSuit)
            {
                return card.suit == lead &&
                       currentWinning != null &&
                       currentWinning.suit == lead &&
                       card.rank > currentWinning.rank;
            }

            // Rule 2: if player has lead suit but cannot beat, must still follow suit
            if (hasLeadSuit)
            {
                return card.suit == lead;
            }

            // Rule 3: if no lead suit but has spade, must play spade
            if (hasSpade)
            {
                return card.suit == Suit.Spades;
            }

            // Rule 4: otherwise anything allowed
            return true;
        }

        public static CardData GetCurrentWinningCard(Dictionary<int, CardData> played, Suit leadSuit)
        {
            CardData best = null;

            foreach (var kv in played)
            {
                CardData c = kv.Value;

                if (best == null)
                {
                    best = c;
                    continue;
                }

                if (Beats(c, best, leadSuit))
                    best = c;
            }

            return best;
        }

        public static int GetTrickWinner(Dictionary<int, CardData> played, Suit leadSuit)
        {
            int winner = -1;
            CardData best = null;

            foreach (var kv in played)
            {
                if (best == null || Beats(kv.Value, best, leadSuit))
                {
                    best = kv.Value;
                    winner = kv.Key;
                }
            }

            return winner;
        }

        static bool Beats(CardData a, CardData b, Suit leadSuit)
        {
            if (a.suit == Suit.Spades && b.suit != Suit.Spades) return true;
            if (a.suit != Suit.Spades && b.suit == Suit.Spades) return false;

            if (a.suit == b.suit) return a.rank > b.rank;

            if (a.suit == leadSuit && b.suit != leadSuit) return true;

            return false;
        }
    }
}