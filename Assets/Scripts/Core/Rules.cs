using System.Collections.Generic;

namespace Core
{
    public static class Rules
    {
        // Must follow suit if possible
        public static bool IsLegalMove(List<CardData> hand, CardData card, Suit? leadSuit)
        {
            if (leadSuit == null) return true;

            bool hasLeadSuit = false;
            for (int i = 0; i < hand.Count; i++)
            {
                if (hand[i].suit == leadSuit.Value)
                {
                    hasLeadSuit = true;
                    break;
                }
            }

            if (hasLeadSuit && card.suit != leadSuit.Value)
                return false;

            return true;
        }

        // Winner: if any spade -> highest spade, else highest of lead suit
        public static int GetTrickWinner(Dictionary<int, CardData> played, Suit leadSuit)
        {
            bool anySpade = false;
            foreach (var kv in played)
            {
                if (kv.Value.suit == Suit.Spades)
                {
                    anySpade = true;
                    break;
                }
            }

            int winner = -1;
            int bestRank = -1;

            foreach (var kv in played)
            {
                int playerIndex = kv.Key;
                CardData card = kv.Value;

                if (anySpade)
                {
                    if (card.suit != Suit.Spades) continue;
                    int r = (int)card.rank;
                    if (r > bestRank)
                    {
                        bestRank = r;
                        winner = playerIndex;
                    }
                }
                else
                {
                    if (card.suit != leadSuit) continue;
                    int r = (int)card.rank;
                    if (r > bestRank)
                    {
                        bestRank = r;
                        winner = playerIndex;
                    }
                }
            }

            return winner;
        }
    }
}
