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
            players.Add(new PlayerData(0, true, maxRounds));   // You
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

                // clear all round scores
                for (int i = 0; i < p.roundScores.Length; i++)
                    p.roundScores[i] = 0;
            }
        }

        // =================== ROUND START ===================

        void StartRound(int roundNumber)
        {
            waitingForNextRoundButton = false;

            // fresh deck
            deck = new Deck();
            deck.Build52();
            deck.Shuffle();

            DealCards(deck);

            // show your hand before bidding (disable click)
            handUI.Render(players[0].hand, OnHumanCardClicked, (c) => false);

            if (trickUI) trickUI.Clear();

            biddingDone = false;
            leaderIndex = 0;
            currentPlayerIndex = 0;

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
            bidUI.Open();
        }

        public void OnHumanBidConfirmed(int bid)
        {
            players[0].bid = bid;

            for (int i = 1; i < players.Count; i++)
                players[i].bid = GetAIBid(players[i]);

            biddingDone = true;

            if (scoreboardUI) scoreboardUI.Refresh(players);

            StartNewTrick();
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
            // Round complete when no cards left
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
            handUI.SetAllInteractable(false);

            PlayCard(0, card);

            currentPlayerIndex = (currentPlayerIndex + 1) % 4;

            RunTurnLoop();
        }

        void PlayCard(int playerIndex, CardData card)
        {
            if (trick.leadSuit == null)
                trick.leadSuit = card.suit;

            players[playerIndex].hand.Remove(card);
            trick.played[playerIndex] = card;

            // seat-based
            if (trickUI) trickUI.SetCardForPlayer(playerIndex, card);
        }

        CardData ChooseCard_Auto(PlayerData p, Suit? leadSuit)
        {
            for (int i = 0; i < p.hand.Count; i++)
            {
                CardData c = p.hand[i];
                if (Rules.IsLegalMove(p.hand, c, leadSuit))
                    return c;
            }
            return p.hand[0];
        }

        // =================== ROUND END + PANELS ===================

        void EndRoundAndShowPanel()
        {
            // Calculate scores
            int roundIndex = currentRound - 1;

            for (int i = 0; i < players.Count; i++)
            {
                var p = players[i];

                int roundScore;
                if (p.tricksWon >= p.bid)
                    roundScore = p.bid + (p.tricksWon - p.bid); // pass
                else
                    roundScore = -p.bid; // fail

                p.lastRoundScore = roundScore;
                p.totalScore += roundScore;
                p.roundScores[roundIndex] = roundScore;
            }

            if (scoreboardUI) scoreboardUI.Refresh(players);

            // Pause game until button click
            waitingForNextRoundButton = true;

            if (currentRound < maxRounds)
            {
                // Show Round Result Panel with Next button
                roundResultUI.Show(this, currentRound, maxRounds, players);

            }
            else
            {
                // Final panel
                finalResultUI.Show(this, maxRounds, players);

            }
        }

        // Called from RoundResultUI button
        public void StartNextRoundFromUI()
        {
            if (currentRound >= maxRounds) return;

            currentRound++;
            StartRound(currentRound);
        }
        
        public void RestartMatch()
        {
            // Hide panels
            if (finalResultUI && finalResultUI.panel) finalResultUI.panel.SetActive(false);
            if (roundResultUI && roundResultUI.panel) roundResultUI.panel.SetActive(false);

            // Show game UI again
            if (finalResultUI && finalResultUI.gameUIRoot) finalResultUI.gameUIRoot.SetActive(true);

            // Reset and start from round 1
            currentRound = 1;
            ResetAllTotals();
            StartRound(currentRound);
        }

    }
}
