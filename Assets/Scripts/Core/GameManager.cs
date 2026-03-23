using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Core
{
    public partial class GameManager : MonoBehaviour
    {
        public List<PlayerData> players = new List<PlayerData>();

        [Header("Deal Targets (hand positions)")]
        public RectTransform dealBottom;
        public RectTransform dealLeft;
        public RectTransform dealTop;
        public RectTransform dealRight;

        [Header("Play Targets (table positions)")]
        public RectTransform playBottom;
        public RectTransform playLeft;
        public RectTransform playTop;
        public RectTransform playRight;

        [Header("Turn Indicator")]
        public Image bottomSeatImage;
        public Image leftSeatImage;
        public Image topSeatImage;
        public Image rightSeatImage;

        public Color normalColor = Color.white;
        public Color activeColor = new Color(1f, 1f, 0.4f);

        [Header("Deal Animation Pro")]
        public RectTransform deckPoint;
        public bool animateDeal = true;
        public float dealInterval = 0.03f;
        public float dealStartDelay = 0.2f;

        [Header("Animation")]
        public Canvas canvas;
        public FlyingCardUI flyingCardPrefab;
        public RectTransform bottomSeat;
        public RectTransform leftSeat;
        public RectTransform topSeat;
        public RectTransform rightSeat;
        public float throwDuration = 0.25f;

        [Header("UI")]
        public GameUIManager uiManager;
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
        int currentRound = 1;
        int humanBidValue = 0;

        bool waitingForHuman = false;
        bool waitingForHumanBid = false;
        bool biddingDone = false;
        bool isTurnRoutineRunning = false;
        bool waitingForNextRoundButton = false;

        void Start()
        {
            InitializePlayers();
            ResetMatchScores();
            BeginRound(currentRound);
        }

        void InitializePlayers()
        {
            players.Clear();
            players.Add(new PlayerData(0, true, maxRounds));
            players.Add(new PlayerData(1, false, maxRounds));
            players.Add(new PlayerData(2, false, maxRounds));
            players.Add(new PlayerData(3, false, maxRounds));
        }

        RectTransform GetDealTarget(int playerIndex)
        {
            switch (playerIndex)
            {
                case 0: return dealBottom;
                case 1: return dealLeft;
                case 2: return dealTop;
                case 3: return dealRight;
            }

            return dealBottom;
        }

        RectTransform GetPlayTarget(int playerIndex)
        {
            switch (playerIndex)
            {
                case 0: return playBottom;
                case 1: return playLeft;
                case 2: return playTop;
                case 3: return playRight;
            }

            return playBottom;
        }

        void ResetMatchScores()
        {
            foreach (var player in players)
            {
                ResetPlayerMatchScore(player);

                for (int i = 0; i < player.roundScores.Length; i++)
                {
                    player.roundScores[i] = 0f;
                }
            }
        }

        void BeginRound(int roundNumber)
        {
            waitingForNextRoundButton = false;
            DealHandsUntilHumanHasSpade();
            ClearPlayedCardsOnTable();

            biddingDone = false;
            leaderIndex = GetStartingPlayerIndexForRound();
            currentPlayerIndex = leaderIndex;

            UpdateScoreboardDisplay();

            if (animateDeal && deckPoint != null)
            {
                StartCoroutine(DealCardsThenStartBidding());
                return;
            }

            UpdateHumanHandDisplay(players[0].hand, false);
            BeginBiddingPhase();
        }

        IEnumerator DealCardsThenStartBidding()
        {
            UpdateHumanHandDisplay(new List<CardData>(), false);
            yield return StartCoroutine(AnimateInitialDeal());

            players[0].SortHand();
            UpdateHumanHandDisplay(players[0].hand, false);
            BeginBiddingPhase();
        }

        IEnumerator AnimateInitialDeal()
        {
            if (!HasAllDealAnimationReferences())
            {
                Debug.LogError("Deal animation missing references! Check GameManager inspector.");
                yield break;
            }

            yield return new WaitForSeconds(dealStartDelay);

            List<CardData> visibleHumanCards = new List<CardData>();
            for (int round = 0; round < 13; round++)
            {
                for (int playerIndex = 0; playerIndex < 4; playerIndex++)
                {
                    CardData card = players[playerIndex].hand[round];
                    yield return StartCoroutine(AnimateCardDealToPlayer(playerIndex, card));

                    if (playerIndex == 0)
                    {
                        visibleHumanCards.Add(card);
                        UpdateHumanHandDisplay(visibleHumanCards, false);
                    }

                    PlayCardThrowSound();
                    yield return new WaitForSeconds(dealInterval);
                }
            }
        }

        void DealHandsFromDeck(Deck sourceDeck)
        {
            foreach (var player in players)
            {
                player.hand.Clear();
                ResetPlayerRoundStateForNewRound(player);
            }

            for (int round = 0; round < 13; round++)
            {
                for (int playerIndex = 0; playerIndex < players.Count; playerIndex++)
                {
                    players[playerIndex].hand.Add(sourceDeck.Draw());
                }
            }
        }

        bool HandHasSpade(List<CardData> hand)
        {
            for (int i = 0; i < hand.Count; i++)
            {
                if (hand[i].suit == Suit.Spades)
                {
                    return true;
                }
            }

            return false;
        }

        void DealHandsUntilHumanHasSpade()
        {
            int safety = 100;

            do
            {
                deck = new Deck();
                deck.Build52();
                deck.Shuffle();
                DealHandsFromDeck(deck);
                safety--;
            }
            while (!HandHasSpade(players[0].hand) && safety > 0);

            if (safety <= 0)
            {
                Debug.LogWarning("Redeal safety limit reached.");
            }
        }

        void EndRoundAndShowResults()
        {
            HighlightActivePlayer(-1);

            int roundIndex = currentRound - 1;
            for (int i = 0; i < players.Count; i++)
            {
                PlayerData player = players[i];
                float roundScore = CalculateRoundScoreForPlayer(player);

                player.lastRoundScore = roundScore;
                player.totalScore += roundScore;
                player.roundScores[roundIndex] = roundScore;
            }

            UpdateScoreboardDisplay();
            waitingForNextRoundButton = true;
            PlayRoundEndSound();

            if (currentRound < maxRounds)
            {
                if (uiManager) uiManager.ShowRoundResults(this, currentRound, maxRounds, players);
                else if (roundResultUI) roundResultUI.Show(this, currentRound, maxRounds, players);
                return;
            }

            PlayFinalResultSound();
            if (uiManager) uiManager.ShowFinalResults(this, maxRounds, players);
            else if (finalResultUI) finalResultUI.Show(this, maxRounds, players);
        }

        public void StartNextRoundFromUI()
        {
            if (currentRound >= maxRounds) return;

            currentRound++;
            BeginRound(currentRound);
        }

        public void RestartMatch()
        {
            HideResultsAndShowGameUI();
            currentRound = 1;
            ResetMatchScores();
            BeginRound(currentRound);
        }
    }
}
