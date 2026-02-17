using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public class GameManager : MonoBehaviour
    {
        public List<PlayerData> players = new List<PlayerData>();

        [Header("UI")]
        public BidUI bidUI;
        public HandUI handUI;
        public TrickUI trickUI;
        public ScoreboardUI scoreboardUI;
        
        bool waitingForHumanBid = false;
        int humanBidValue = 0;


        [Header("Result Panels")]
        public RoundResultUI roundResultUI;
        public FinalResultUI finalResultUI;

        [Header("Rounds")]
        public int maxRounds = 5;

        [Header("Timing")]
        public float aiPlayDelay = 0.8f;
        public float winnerDelay = 0.8f;

        Deck deck;
        TrickData trick = new TrickData();

        int leaderIndex = 0;
        int currentPlayerIndex = 0;

        bool waitingForHuman = false;
        bool biddingDone = false;
        bool isTurnRoutineRunning = false;

        int currentRound = 1;
        bool waitingForNextRoundButton = false;

        void Start()
        {
            CreatePlayers();
            ResetAllTotals();
            StartRound(currentRound);
        }

        void CreatePlayers()
        {
            players.Clear();
            players.Add(new PlayerData(0, true, maxRounds));   // You (Bottom)
            players.Add(new PlayerData(1, false, maxRounds));  // Left
            players.Add(new PlayerData(2, false, maxRounds));  // Top
            players.Add(new PlayerData(3, false, maxRounds));  // Right
        }

        void ResetAllTotals()
        {
            foreach (var p in players)
            {
                p.totalScore = 0;
                p.lastRoundScore = 0;
                p.tricksWon = 0;
                p.bid = 0;

                for (int i = 0; i < p.roundScores.Length; i++)
                    p.roundScores[i] = 0;
            }
        }

        // =================== ROUND START ===================

        void StartRound(int roundNumber)
        {
            waitingForNextRoundButton = false;

            deck = new Deck();
            deck.Build52();
            deck.Shuffle();

            DealCards(deck);

            // show your hand before bidding (disable click)
            handUI.Render(players[0].hand, OnHumanCardClicked, (c) => false);

            if (trickUI) trickUI.Clear();

            biddingDone = false;
            leaderIndex = (currentRound - 1) % 4;   // ✅ round starter rotates
            currentPlayerIndex = leaderIndex;

            if (scoreboardUI)
            {
                scoreboardUI.SetRound(currentRound, maxRounds);
                scoreboardUI.Refresh(players);
            }

            StartBidding();
        }

        void DealCards(Deck deck)
        {
            foreach (var p in players)
            {
                p.hand.Clear();
                p.bid = 0;
                p.tricksWon = 0;
                p.lastRoundScore = 0;
            }

            for (int r = 0; r < 13; r++)
                for (int i = 0; i < players.Count; i++)
                    players[i].hand.Add(deck.Draw());

            foreach (var p in players) p.SortHand();
        }

        void StartBidding()
        {
            StartCoroutine(BidRoutine());
        }
        
        IEnumerator BidRoutine()
        {
            biddingDone = false;

            // clear bids first (so UI shows blank/0 while bidding)
            for (int i = 0; i < players.Count; i++)
                players[i].bid = 0;

            if (scoreboardUI) scoreboardUI.Refresh(players);

            Debug.Log($"=== ROUND {currentRound} BIDDING (Leader bids last): Leader=P{leaderIndex} ===");

            // bidding order: leader+1 ... leader (leader last)
            int bidder = (leaderIndex + 1) % 4;

            for (int step = 0; step < 4; step++)
            {
                if (bidder == 0)
                {
                    // Human bids when it's your turn
                    waitingForHumanBid = true;
                    bidUI.Open();

                    // wait until you press confirm
                    while (waitingForHumanBid)
                        yield return null;

                    players[0].bid = humanBidValue;

                    // close UI (use what you have)
                    // bidUI.Close(); // if you have Close()
                    bidUI.gameObject.SetActive(false);

                    if (scoreboardUI) scoreboardUI.Refresh(players);
                }
                else
                {
                    // AI bids
                    players[bidder].bid = GetAIBid(players[bidder]);
                    if (scoreboardUI) scoreboardUI.Refresh(players);

                    yield return new WaitForSeconds(0.4f); // small pause so you can see bids coming
                }

                bidder = (bidder + 1) % 4;
            }

            biddingDone = true;

            // Start trick play from leader
            StartNewTrick();
        }


        public void OnHumanBidConfirmed(int bid)
        {
            humanBidValue = bid;
            waitingForHumanBid = false; // ✅ this releases the BidRoutine()
        }

        int GetAIBid(PlayerData ai)
        {
            int spades = 0;
            int high = 0;

            foreach (var c in ai.hand)
            {
                if (c.suit == Suit.Spades) spades++;
                if (c.rank == Rank.Ace || c.rank == Rank.King || c.rank == Rank.Queen || c.rank == Rank.Jack)
                    high++;
            }

            int bid = 1 + (spades / 3) + (high / 4);
            return Mathf.Clamp(bid, 1, 8);
        }

        // =================== TRICKS ===================

        void StartNewTrick()
        {
            if (players[0].hand.Count == 0)
            {
                EndRoundAndShowPanel();
                return;
            }

            trick.Reset(leaderIndex);
            currentPlayerIndex = leaderIndex;

            if (trickUI) trickUI.Clear();

            RunTurnLoop();
        }

        void RunTurnLoop()
        {
            if (!biddingDone) return;
            if (waitingForNextRoundButton) return;
            if (isTurnRoutineRunning) return;

            StartCoroutine(TurnRoutine());
        }

        IEnumerator TurnRoutine()
        {
            isTurnRoutineRunning = true;

            // Trick complete?
            if (trick.played.Count >= 4)
            {
                yield return new WaitForSeconds(winnerDelay);

                int winner = Rules.GetTrickWinner(trick.played, trick.leadSuit.Value);
                players[winner].tricksWon++;

                if (scoreboardUI) scoreboardUI.Refresh(players);

                leaderIndex = winner;

                isTurnRoutineRunning = false;
                StartNewTrick();
                yield break;
            }

            // Human turn
            if (currentPlayerIndex == 0)
            {
                waitingForHuman = true;

                handUI.Render(
                    players[0].hand,
                    OnHumanCardClicked,
                    (card) => Rules.IsLegalMove(players[0].hand, card, trick.leadSuit)
                );

                isTurnRoutineRunning = false;
                yield break;
            }

            // AI turn
            waitingForHuman = false;

            yield return new WaitForSeconds(aiPlayDelay);

            PlayerData ai = players[currentPlayerIndex];
            CardData chosen = ChooseCard_Auto(ai, trick.leadSuit);

            PlayCard(currentPlayerIndex, chosen);

            currentPlayerIndex = (currentPlayerIndex + 1) % 4;

            isTurnRoutineRunning = false;
            RunTurnLoop();
        }

        void OnHumanCardClicked(CardData card)
        {
            if (!biddingDone) return;
            if (waitingForNextRoundButton) return;
            if (!waitingForHuman) return;

            if (!Rules.IsLegalMove(players[0].hand, card, trick.leadSuit))
                return;

            waitingForHuman = false;

            // Remove & show card in trick
            PlayCard(0, card);

            // 🔥 Immediately refresh hand UI
            handUI.Render(
                players[0].hand,
                OnHumanCardClicked,
                (c) => false
            );

            currentPlayerIndex = (currentPlayerIndex + 1) % 4;

            RunTurnLoop();
        }


        void PlayCard(int playerIndex, CardData card)
        {
            if (trick.leadSuit == null)
                trick.leadSuit = card.suit;

            players[playerIndex].hand.Remove(card);
            trick.played[playerIndex] = card;

            if (trickUI) trickUI.SetCardForPlayer(playerIndex, card);
        }

        // =================== SMART AI ===================

        CardData ChooseCard_Auto(PlayerData ai, Suit? leadSuit)
        {
            // If AI is leading the trick
            if (leadSuit == null)
                return ChooseLeadCard(ai);

            Suit lead = leadSuit.Value;

            CardData currentBest = GetCurrentWinningCard(lead);

            // Must follow suit if possible
            List<CardData> followSuit = ai.hand.FindAll(c => c.suit == lead);
            if (followSuit.Count > 0)
            {
                followSuit.Sort(CardCompare);

                // Try to win with the lowest winning card
                CardData winCard = FindLowestWinningFollow(followSuit, currentBest, lead);
                if (winCard != null) return winCard;

                // Can't win -> throw lowest in that suit
                return followSuit[0];
            }

            // Can't follow suit -> try trumping with spade if it can win
            List<CardData> spades = ai.hand.FindAll(c => c.suit == Suit.Spades);
            if (spades.Count > 0)
            {
                spades.Sort(CardCompare);

                CardData trumpWin = FindLowestTrumpWin(spades, currentBest);
                if (trumpWin != null) return trumpWin;
            }

            // Otherwise discard lowest (prefer non-spade)
            return ChooseLowestDiscard(ai);
        }

        CardData ChooseLeadCard(PlayerData ai)
        {
            // Lead strategy:
            // - Prefer a long non-spade suit
            // - Try leading a high card (A/K/Q) from that suit
            // - Otherwise lead lowest non-spade

            int clubs = ai.hand.FindAll(c => c.suit == Suit.Clubs).Count;
            int diamonds = ai.hand.FindAll(c => c.suit == Suit.Diamonds).Count;
            int hearts = ai.hand.FindAll(c => c.suit == Suit.Hearts).Count;

            Suit bestSuit = Suit.Clubs;
            int bestCount = clubs;

            if (diamonds > bestCount) { bestSuit = Suit.Diamonds; bestCount = diamonds; }
            if (hearts > bestCount) { bestSuit = Suit.Hearts; bestCount = hearts; }

            List<CardData> candidates = ai.hand.FindAll(c => c.suit == bestSuit);
            candidates.Sort(CardCompare);

            // Lead high if possible
            for (int i = candidates.Count - 1; i >= 0; i--)
            {
                if (candidates[i].rank == Rank.Ace ||
                    candidates[i].rank == Rank.King ||
                    candidates[i].rank == Rank.Queen)
                    return candidates[i];
            }

            return ChooseLowestDiscard(ai);
        }

        CardData ChooseLowestDiscard(PlayerData ai)
        {
            List<CardData> nonSpades = ai.hand.FindAll(c => c.suit != Suit.Spades);
            if (nonSpades.Count > 0)
            {
                nonSpades.Sort(CardCompare);
                return nonSpades[0];
            }

            List<CardData> spades = ai.hand.FindAll(c => c.suit == Suit.Spades);
            spades.Sort(CardCompare);
            return spades[0];
        }

        CardData GetCurrentWinningCard(Suit leadSuit)
        {
            CardData best = null;

            foreach (var kv in trick.played)
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

        CardData FindLowestWinningFollow(List<CardData> followSuit, CardData currentBest, Suit leadSuit)
        {
            for (int i = 0; i < followSuit.Count; i++)
            {
                if (Beats(followSuit[i], currentBest, leadSuit))
                    return followSuit[i];
            }
            return null;
        }

        CardData FindLowestTrumpWin(List<CardData> spades, CardData currentBest)
        {
            if (currentBest == null) return spades[0];

            if (currentBest.suit != Suit.Spades)
                return spades[0]; // any spade wins

            // must beat current spade
            for (int i = 0; i < spades.Count; i++)
                if (spades[i].rank > currentBest.rank)
                    return spades[i];

            return null;
        }

        bool Beats(CardData a, CardData b, Suit leadSuit)
        {
            // spade trumps
            if (a.suit == Suit.Spades && b.suit != Suit.Spades) return true;
            if (a.suit != Suit.Spades && b.suit == Suit.Spades) return false;

            // same suit higher rank
            if (a.suit == b.suit) return a.rank > b.rank;

            // only lead suit matters for non-spades
            if (a.suit == leadSuit && b.suit != leadSuit) return true;

            return false;
        }

        int CardCompare(CardData a, CardData b)
        {
            return a.rank.CompareTo(b.rank); // low -> high
        }

        // =================== ROUND END + PANELS ===================

        void EndRoundAndShowPanel()
        {
            int roundIndex = currentRound - 1;

            for (int i = 0; i < players.Count; i++)
            {
                var p = players[i];

                int roundScore;
                if (p.tricksWon >= p.bid)
                    roundScore = p.bid + (p.tricksWon - p.bid);
                else
                    roundScore = -p.bid;

                p.lastRoundScore = roundScore;
                p.totalScore += roundScore;
                p.roundScores[roundIndex] = roundScore;
            }

            if (scoreboardUI) scoreboardUI.Refresh(players);

            waitingForNextRoundButton = true;

            if (currentRound < maxRounds)
            {
                roundResultUI.Show(this, currentRound, maxRounds, players);
            }
            else
            {
                finalResultUI.Show(this, maxRounds, players);
            }
        }

        public void StartNextRoundFromUI()
        {
            if (currentRound >= maxRounds) return;

            currentRound++;
            StartRound(currentRound);
        }

        public void RestartMatch()
        {
            if (finalResultUI && finalResultUI.panel) finalResultUI.panel.SetActive(false);
            if (roundResultUI && roundResultUI.panel) roundResultUI.panel.SetActive(false);

            if (finalResultUI && finalResultUI.gameUIRoot) finalResultUI.gameUIRoot.SetActive(true);

            currentRound = 1;
            ResetAllTotals();
            StartRound(currentRound);
        }
    }
}
