using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    [System.Serializable]
    public class PlayerData
    {
        public int index;
        public bool isHuman;

        public List<CardData> hand = new List<CardData>();

        public int bid;
        public int tricksWon;

        // ✅ Callbreak decimal scoring
        public float totalScore;
        public float lastRoundScore;
        public float[] roundScores;

        public PlayerData(int index, bool isHuman, int maxRounds)
        {
            this.index = index;
            this.isHuman = isHuman;

            bid = 0;
            tricksWon = 0;

            totalScore = 0f;
            lastRoundScore = 0f;
            roundScores = new float[maxRounds];
        }

        public void SortHand()
        {
            // Keep your existing sort if you had one. Example:
            hand.Sort((a, b) =>
            {
                int suit = a.suit.CompareTo(b.suit);
                if (suit != 0) return suit;
                return a.rank.CompareTo(b.rank);
            });
        }
    }
}