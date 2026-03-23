using System.Collections;
using UnityEngine;

namespace Core
{
    public partial class GameManager
    {
        void BeginBiddingPhase()
        {
            StartCoroutine(RunBiddingPhase());
        }

        IEnumerator RunBiddingPhase()
        {
            biddingDone = false;

            for (int i = 0; i < players.Count; i++)
            {
                players[i].bid = 0;
            }

            UpdateScoreboardDisplay();

            int bidder = (leaderIndex + 1) % 4;
            for (int step = 0; step < 4; step++)
            {
                if (bidder == 0)
                {
                    yield return StartCoroutine(WaitForHumanBidSelection());
                }
                else
                {
                    AssignAiBid(bidder);
                    yield return new WaitForSeconds(0.25f);
                }

                bidder = (bidder + 1) % 4;
            }

            biddingDone = true;
            BeginTrick();
        }

        public void OnHumanBidConfirmed(int bid)
        {
            humanBidValue = bid;
            waitingForHumanBid = false;
        }

        int CalculateAiBid(PlayerData ai)
        {
            int spades = 0;
            int highCards = 0;

            foreach (var card in ai.hand)
            {
                if (card.suit == Suit.Spades) spades++;

                if (card.rank == Rank.Ace ||
                    card.rank == Rank.King ||
                    card.rank == Rank.Queen ||
                    card.rank == Rank.Jack)
                {
                    highCards++;
                }
            }

            int bid = 1 + (spades / 3) + (highCards / 4);
            return Mathf.Clamp(bid, 1, 8);
        }

        IEnumerator WaitForHumanBidSelection()
        {
            waitingForHumanBid = true;

            if (uiManager) uiManager.ShowBidPanel();
            else if (bidUI) bidUI.Open();

            while (waitingForHumanBid)
            {
                yield return null;
            }

            players[0].bid = humanBidValue;

            if (uiManager) uiManager.HideBidPanel();
            else if (bidUI) bidUI.gameObject.SetActive(false);

            UpdateScoreboardDisplay();
        }

        void AssignAiBid(int bidder)
        {
            players[bidder].bid = CalculateAiBid(players[bidder]);
            UpdateScoreboardDisplay();
        }
    }
}
