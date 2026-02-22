using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Core
{
    public class GameManager : MonoBehaviour
    {
        public List<PlayerData> players = new List<PlayerData>();

        [Header("Turn Indicator")]
        public Image bottomSeatImage;
        public Image leftSeatImage;
        public Image topSeatImage;
        public Image rightSeatImage;

        public Color normalColor = Color.white;
        public Color activeColor = new Color(1f, 1f, 0.4f); // soft yellow

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
                p.totalScore = 0f;
                p.lastRoundScore = 0f;
                p.tricksWon = 0;
                p.bid = 0;

                for (int i = 0; i < p.roundScores.Length; i++)
                    p.roundScores[i] = 0f;
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

            if (trickUI) trickUI.Clear();

            biddingDone = false;

            // ✅ round starter rotates
            leaderIndex = (currentRound - 1) % 4;
            currentPlayerIndex = leaderIndex;

            if (scoreboardUI)
            {
                scoreboardUI.SetRound(currentRound, maxRounds);
                scoreboardUI.Refresh(players);
            }

            // ✅ IMPORTANT: run only ONE deal flow
            if (animateDeal && deckPoint != null)
                StartCoroutine(DealThenBid());
            else
            {
                handUI.Render(players[0].hand, OnHumanCardClicked, (c) => false);
                StartBidding();
            }
        }

        IEnumerator DealThenBid()
        {
            // show empty hand while dealing
            handUI.Render(new List<CardData>(), OnHumanCardClicked, (c) => false);

            yield return StartCoroutine(DealAllPlayersAnimated());

            // sort after dealing (optional)
            players[0].SortHand();
            handUI.Render(players[0].hand, OnHumanCardClicked, (c) => false);

            StartBidding();
        }

        IEnumerator DealAllPlayersAnimated()
        {
            if (!deckPoint || !canvas || !flyingCardPrefab || !bottomSeat || !leftSeat || !topSeat || !rightSeat)
            {
                Debug.LogError("Deal animation missing references! Check GameManager inspector.");
                yield break;
            }

            yield return new WaitForSeconds(dealStartDelay);

            List<CardData> p0Temp = new List<CardData>();

            for (int r = 0; r < 13; r++)
            {
                // P0 face up
                yield return StartCoroutine(AnimateThrow(players[0].hand[r], deckPoint.position, bottomSeat, false));
                p0Temp.Add(players[0].hand[r]);
                handUI.Render(p0Temp, OnHumanCardClicked, (c) => false);
                if (SFXManager.I) SFXManager.I.PlayCardThrow();
                yield return new WaitForSeconds(dealInterval);

                // P1 face down
                yield return StartCoroutine(AnimateThrow(players[1].hand[r], deckPoint.position, leftSeat, true));
                if (SFXManager.I) SFXManager.I.PlayCardThrow();
                yield return new WaitForSeconds(dealInterval);

                // ✅ P2 correct
                yield return StartCoroutine(AnimateThrow(players[2].hand[r], deckPoint.position, topSeat, true));
                if (SFXManager.I) SFXManager.I.PlayCardThrow();
                yield return new WaitForSeconds(dealInterval);

                // ✅ P3 correct
                yield return StartCoroutine(AnimateThrow(players[3].hand[r], deckPoint.position, rightSeat, true));
                if (SFXManager.I) SFXManager.I.PlayCardThrow();
                yield return new WaitForSeconds(dealInterval);
            }
        }

        void DealCards(Deck deck)
        {
            foreach (var p in players)
            {
                p.hand.Clear();
                p.bid = 0;
                p.tricksWon = 0;
                p.lastRoundScore = 0f;
            }

            for (int r = 0; r < 13; r++)
                for (int i = 0; i < players.Count; i++)
                    players[i].hand.Add(deck.Draw());

            // ✅ Don't sort here if you want realistic deal order
            // Sorting is done after dealing (for P0) in DealThenBid
        }

        // =================== BIDDING ===================

        void StartBidding()
        {
            StartCoroutine(BidRoutine());
        }

        IEnumerator BidRoutine()
        {
            biddingDone = false;

            for (int i = 0; i < players.Count; i++)
                players[i].bid = 0;

            if (scoreboardUI) scoreboardUI.Refresh(players);

            // bidding order: leader+1 ... leader (leader last)
            int bidder = (leaderIndex + 1) % 4;

            for (int step = 0; step < 4; step++)
            {
                if (bidder == 0)
                {
                    waitingForHumanBid = true;
                    bidUI.Open();

                    while (waitingForHumanBid)
                        yield return null;

                    players[0].bid = humanBidValue;
                    bidUI.gameObject.SetActive(false);

                    if (scoreboardUI) scoreboardUI.Refresh(players);
                }
                else
                {
                    players[bidder].bid = GetAIBid(players[bidder]);
                    if (scoreboardUI) scoreboardUI.Refresh(players);
                    yield return new WaitForSeconds(0.25f);
                }

                bidder = (bidder + 1) % 4;
            }

            biddingDone = true;

            StartNewTrick();
        }

        public void OnHumanBidConfirmed(int bid)
        {
            humanBidValue = bid;
            waitingForHumanBid = false;
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
            UpdateTurnIndicator(currentPlayerIndex);

            // Trick complete?
            if (trick.played.Count >= 4)
            {
                yield return new WaitForSeconds(winnerDelay);

                int winner = Rules.GetTrickWinner(trick.played, trick.leadSuit.Value);
                players[winner].tricksWon++;

                yield return StartCoroutine(AnimateTrickToWinner(winner));

                if (SFXManager.I) SFXManager.I.PlayTrickWin();

                if (scoreboardUI) scoreboardUI.Refresh(players);

                leaderIndex = winner;

                isTurnRoutineRunning = false;
                StartNewTrick();
                yield break;
            }

            // Human
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

            // AI
            waitingForHuman = false;

            yield return new WaitForSeconds(aiPlayDelay);

            PlayerData ai = players[currentPlayerIndex];
            CardData chosen = ChooseCard_Auto(ai, trick.leadSuit);

            RectTransform target = GetSeatTarget(currentPlayerIndex);

            Vector3 startPos = target.position;
            startPos += (target.position - bottomSeat.position).normalized * 200f;

            yield return StartCoroutine(AnimateThrow(chosen, startPos, target, false));

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

            RectTransform target = GetSeatTarget(0);
            Vector3 startPos = target.position + Vector3.down * 200f;

            StartCoroutine(HumanPlayRoutine(card, startPos, target));
        }

        IEnumerator HumanPlayRoutine(CardData card, Vector3 startPos, RectTransform target)
        {
            yield return StartCoroutine(AnimateThrow(card, startPos, target, false));

            PlayCard(0, card);

            handUI.Render(players[0].hand, OnHumanCardClicked, (c) => false);

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
            if (SFXManager.I) SFXManager.I.PlayCardThrow();
        }

        // =================== TRICK COLLECT ANIM ===================

        IEnumerator AnimateTrickToWinner(int winnerIndex)
        {
            RectTransform winnerSeat = GetSeatTarget(winnerIndex);

            List<RectTransform> flyers = new List<RectTransform>();
            List<Vector3> starts = new List<Vector3>();

            foreach (var kv in trick.played)
            {
                int playerIndex = kv.Key;
                RectTransform trickSlot = trickUI.GetSlot(playerIndex);

                FlyingCardUI fly = Instantiate(flyingCardPrefab, canvas.transform);
                RectTransform rt = fly.GetComponent<RectTransform>();

                fly.SetFront(kv.Value);
                rt.position = trickSlot.position;

                flyers.Add(rt);
                starts.Add(trickSlot.position);
            }

            float duration = 0.12f;
            float t = 0f;

            while (t < 1f)
            {
                t += Time.deltaTime / duration;
                float smooth = Mathf.SmoothStep(0f, 1f, t);

                for (int i = 0; i < flyers.Count; i++)
                    if (flyers[i])
                        flyers[i].position = Vector3.Lerp(starts[i], winnerSeat.position, smooth);

                yield return null;
            }

            for (int i = 0; i < flyers.Count; i++)
                if (flyers[i]) Destroy(flyers[i].gameObject);

            if (trickUI) trickUI.Clear();
        }

        // =================== SMART AI ===================

        CardData ChooseCard_Auto(PlayerData ai, Suit? leadSuit)
        {
            if (leadSuit == null)
                return ChooseLeadCard(ai);

            Suit lead = leadSuit.Value;
            CardData currentBest = GetCurrentWinningCard(lead);

            List<CardData> followSuit = ai.hand.FindAll(c => c.suit == lead);
            if (followSuit.Count > 0)
            {
                followSuit.Sort(CardCompare);

                CardData winCard = FindLowestWinningFollow(followSuit, currentBest, lead);
                if (winCard != null) return winCard;

                return followSuit[0];
            }

            List<CardData> spades = ai.hand.FindAll(c => c.suit == Suit.Spades);
            if (spades.Count > 0)
            {
                spades.Sort(CardCompare);
                CardData trumpWin = FindLowestTrumpWin(spades, currentBest);
                if (trumpWin != null) return trumpWin;
            }

            return ChooseLowestDiscard(ai);
        }

        CardData ChooseLeadCard(PlayerData ai)
        {
            int clubs = ai.hand.FindAll(c => c.suit == Suit.Clubs).Count;
            int diamonds = ai.hand.FindAll(c => c.suit == Suit.Diamonds).Count;
            int hearts = ai.hand.FindAll(c => c.suit == Suit.Hearts).Count;

            Suit bestSuit = Suit.Clubs;
            int bestCount = clubs;

            if (diamonds > bestCount) { bestSuit = Suit.Diamonds; bestCount = diamonds; }
            if (hearts > bestCount) { bestSuit = Suit.Hearts; bestCount = hearts; }

            List<CardData> candidates = ai.hand.FindAll(c => c.suit == bestSuit);
            candidates.Sort(CardCompare);

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
                if (best == null) { best = c; continue; }
                if (Beats(c, best, leadSuit)) best = c;
            }

            return best;
        }

        CardData FindLowestWinningFollow(List<CardData> followSuit, CardData currentBest, Suit leadSuit)
        {
            for (int i = 0; i < followSuit.Count; i++)
                if (Beats(followSuit[i], currentBest, leadSuit))
                    return followSuit[i];

            return null;
        }

        CardData FindLowestTrumpWin(List<CardData> spades, CardData currentBest)
        {
            if (currentBest == null) return spades[0];

            if (currentBest.suit != Suit.Spades)
                return spades[0];

            for (int i = 0; i < spades.Count; i++)
                if (spades[i].rank > currentBest.rank)
                    return spades[i];

            return null;
        }

        bool Beats(CardData a, CardData b, Suit leadSuit)
        {
            if (a.suit == Suit.Spades && b.suit != Suit.Spades) return true;
            if (a.suit != Suit.Spades && b.suit == Suit.Spades) return false;

            if (a.suit == b.suit) return a.rank > b.rank;
            if (a.suit == leadSuit && b.suit != leadSuit) return true;

            return false;
        }

        int CardCompare(CardData a, CardData b)
        {
            return a.rank.CompareTo(b.rank);
        }

        // =================== ROUND END ===================

        void EndRoundAndShowPanel()
        {
            UpdateTurnIndicator(-1);

            int roundIndex = currentRound - 1;

            for (int i = 0; i < players.Count; i++)
            {
                var p = players[i];

                float roundScore;

                if (p.tricksWon >= p.bid)
                {
                    int extra = p.tricksWon - p.bid;
                    roundScore = p.bid + (extra * 0.1f); // ✅ 0.1 per extra trick
                }
                else
                {
                    roundScore = -p.bid; // ✅ -4.0 formatting in UI
                }

                p.lastRoundScore = roundScore;
                p.totalScore += roundScore;
                p.roundScores[roundIndex] = roundScore;
            }

            if (scoreboardUI) scoreboardUI.Refresh(players);

            waitingForNextRoundButton = true;
            if (SFXManager.I) SFXManager.I.PlayRoundEnd();

            if (currentRound < maxRounds)
            {
                roundResultUI.Show(this, currentRound, maxRounds, players);
            }
            else
            {
                if (SFXManager.I) SFXManager.I.PlayFinal();
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

        RectTransform GetSeatTarget(int playerIndex)
        {
            switch (playerIndex)
            {
                case 0: return bottomSeat;
                case 1: return leftSeat;
                case 2: return topSeat;
                case 3: return rightSeat;
            }
            return bottomSeat;
        }

        void UpdateTurnIndicator(int playerIndex)
        {
            if (bottomSeatImage) bottomSeatImage.color = normalColor;
            if (leftSeatImage) leftSeatImage.color = normalColor;
            if (topSeatImage) topSeatImage.color = normalColor;
            if (rightSeatImage) rightSeatImage.color = normalColor;

            if (playerIndex < 0) return;

            switch (playerIndex)
            {
                case 0: if (bottomSeatImage) bottomSeatImage.color = activeColor; break;
                case 1: if (leftSeatImage) leftSeatImage.color = activeColor; break;
                case 2: if (topSeatImage) topSeatImage.color = activeColor; break;
                case 3: if (rightSeatImage) rightSeatImage.color = activeColor; break;
            }
        }

        IEnumerator AnimateThrow(CardData card, Vector3 startWorldPos, RectTransform targetSeat, bool faceDown)
        {
            FlyingCardUI fly = Instantiate(flyingCardPrefab, canvas.transform);
            RectTransform rt = fly.GetComponent<RectTransform>();

            if (faceDown) fly.SetBack();
            else fly.SetFront(card);

            rt.position = startWorldPos;
            Vector3 endPos = targetSeat.position;

            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / throwDuration;
                float smooth = Mathf.SmoothStep(0f, 1f, t);
                rt.position = Vector3.Lerp(startWorldPos, endPos, smooth);
                yield return null;
            }

            rt.position = endPos;
            Destroy(fly.gameObject);
        }
    }
}