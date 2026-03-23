using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public partial class GameManager
    {
        CardData ChooseAiCardToPlay(PlayerData ai, Suit? leadSuit)
        {
            if (leadSuit == null)
            {
                return ChooseAiLeadCard(ai);
            }

            Suit lead = leadSuit.Value;
            CardData currentBest = GetCurrentWinningCardOnTable(lead);

            List<CardData> followSuit = ai.hand.FindAll(card => card.suit == lead);
            if (followSuit.Count > 0)
            {
                followSuit.Sort(CompareCardsByRank);

                CardData winningFollowCard = FindLowestWinningFollowCard(followSuit, currentBest, lead);
                if (winningFollowCard != null) return winningFollowCard;

                return followSuit[0];
            }

            List<CardData> spades = ai.hand.FindAll(card => card.suit == Suit.Spades);
            if (spades.Count > 0)
            {
                spades.Sort(CompareCardsByRank);

                CardData winningTrump = FindLowestWinningTrump(spades, currentBest);
                if (winningTrump != null) return winningTrump;
            }

            return ChooseLowestDiscardCard(ai);
        }

        CardData ChooseAiLeadCard(PlayerData ai)
        {
            int clubs = ai.hand.FindAll(card => card.suit == Suit.Clubs).Count;
            int diamonds = ai.hand.FindAll(card => card.suit == Suit.Diamonds).Count;
            int hearts = ai.hand.FindAll(card => card.suit == Suit.Hearts).Count;

            Suit bestSuit = Suit.Clubs;
            int bestCount = clubs;

            if (diamonds > bestCount)
            {
                bestSuit = Suit.Diamonds;
                bestCount = diamonds;
            }

            if (hearts > bestCount)
            {
                bestSuit = Suit.Hearts;
                bestCount = hearts;
            }

            List<CardData> candidates = ai.hand.FindAll(card => card.suit == bestSuit);
            candidates.Sort(CompareCardsByRank);

            for (int i = candidates.Count - 1; i >= 0; i--)
            {
                if (candidates[i].rank == Rank.Ace ||
                    candidates[i].rank == Rank.King ||
                    candidates[i].rank == Rank.Queen)
                {
                    return candidates[i];
                }
            }

            return ChooseLowestDiscardCard(ai);
        }

        CardData ChooseLowestDiscardCard(PlayerData ai)
        {
            List<CardData> nonSpades = ai.hand.FindAll(card => card.suit != Suit.Spades);
            if (nonSpades.Count > 0)
            {
                nonSpades.Sort(CompareCardsByRank);
                return nonSpades[0];
            }

            List<CardData> spades = ai.hand.FindAll(card => card.suit == Suit.Spades);
            spades.Sort(CompareCardsByRank);
            return spades[0];
        }

        CardData GetCurrentWinningCardOnTable(Suit leadSuit)
        {
            CardData best = null;

            foreach (var playedCard in trick.played)
            {
                CardData card = playedCard.Value;
                if (best == null || DoesCardBeatCurrentWinner(card, best, leadSuit))
                {
                    best = card;
                }
            }

            return best;
        }

        CardData FindLowestWinningFollowCard(List<CardData> followSuit, CardData currentBest, Suit leadSuit)
        {
            for (int i = 0; i < followSuit.Count; i++)
            {
                if (DoesCardBeatCurrentWinner(followSuit[i], currentBest, leadSuit))
                {
                    return followSuit[i];
                }
            }

            return null;
        }

        CardData FindLowestWinningTrump(List<CardData> spades, CardData currentBest)
        {
            if (currentBest == null) return spades[0];
            if (currentBest.suit != Suit.Spades) return spades[0];

            for (int i = 0; i < spades.Count; i++)
            {
                if (spades[i].rank > currentBest.rank)
                {
                    return spades[i];
                }
            }

            return null;
        }

        bool DoesCardBeatCurrentWinner(CardData challenger, CardData currentWinner, Suit leadSuit)
        {
            if (challenger.suit == Suit.Spades && currentWinner.suit != Suit.Spades) return true;
            if (challenger.suit != Suit.Spades && currentWinner.suit == Suit.Spades) return false;
            if (challenger.suit == currentWinner.suit) return challenger.rank > currentWinner.rank;
            if (challenger.suit == leadSuit && currentWinner.suit != leadSuit) return true;

            return false;
        }

        int CompareCardsByRank(CardData first, CardData second)
        {
            return first.rank.CompareTo(second.rank);
        }
    }
}
