using System.Collections.Generic;

namespace Core
{
    [System.Serializable]
    public class PlayerData
    {
        public int playerId;
        public bool isHuman;

        public List<CardData> hand = new List<CardData>();

        public int bid;
        public int tricksWon;

        public int totalScore;
        public int lastRoundScore;

        // ✅ store each round score (index 0 = Round1, 4 = Round5)
        public int[] roundScores;

        public PlayerData(int id, bool human, int maxRounds = 5)
        {
            playerId = id;
            isHuman = human;

            roundScores = new int[maxRounds];
        }

        public void SortHand()
        {
            hand.Sort((a, b) =>
            {
                int suitCompare = a.suit.CompareTo(b.suit);
                if (suitCompare != 0) return suitCompare;
                return a.rank.CompareTo(b.rank);
            });
        }
    }
}