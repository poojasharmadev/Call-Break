using System.Collections.Generic;

namespace Core
{
    public class TrickData
    {
        public int leaderIndex;
        public Suit? leadSuit = null;
        public Dictionary<int, CardData> played = new Dictionary<int, CardData>();

        public void Reset(int newLeader)
        {
            leaderIndex = newLeader;
            leadSuit = null;
            played.Clear();
        }
    }
}