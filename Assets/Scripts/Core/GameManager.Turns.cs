using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public partial class GameManager
    {
        void BeginTrick()
        {
            if (players[0].hand.Count == 0)
            {
                EndRoundAndShowResults();
                return;
            }

            trick.Reset(leaderIndex);
            currentPlayerIndex = leaderIndex;
            ClearPlayedCardsOnTable();
            TryStartTurnLoop();
        }

        void TryStartTurnLoop()
        {
            if (!CanStartTurnLoop()) return;

            StartCoroutine(RunTurnLoop());
        }

        IEnumerator RunTurnLoop()
        {
            isTurnRoutineRunning = true;
            HighlightActivePlayer(currentPlayerIndex);

            if (trick.played.Count >= 4)
            {
                yield return StartCoroutine(ResolveCompletedTrick());
                yield break;
            }

            if (currentPlayerIndex == 0)
            {
                ShowHumanTurnOptions();
                isTurnRoutineRunning = false;
                yield break;
            }

            waitingForHuman = false;
            yield return new WaitForSeconds(aiPlayDelay);
            yield return StartCoroutine(RunAiTurn());

            isTurnRoutineRunning = false;
            TryStartTurnLoop();
        }

        void HandleHumanCardSelected(CardData card)
        {
            if (!CanHumanPlaySelectedCard(card)) return;

            waitingForHuman = false;
            StartCoroutine(PlayHumanSelectedCard(card));
        }

        IEnumerator PlayHumanSelectedCard(CardData card)
        {
            yield return StartCoroutine(AnimateCardFromHandToTable(0, card));

            CommitPlayedCard(0, card);
            UpdateHumanHandDisplay(players[0].hand, false);
            MoveToNextPlayer();
            TryStartTurnLoop();
        }

        void CommitPlayedCard(int playerIndex, CardData card)
        {
            if (trick.leadSuit == null)
            {
                trick.leadSuit = card.suit;
            }

            players[playerIndex].hand.Remove(card);
            trick.played[playerIndex] = card;

            if (uiManager) uiManager.ShowPlayedCard(playerIndex, card);
            else if (trickUI) trickUI.SetCardForPlayer(playerIndex, card);

            PlayCardThrowSound();
        }

        IEnumerator AnimateTrickCardsToWinner(int winnerIndex)
        {
            RectTransform winnerPile = GetDealTarget(winnerIndex);
            List<RectTransform> flyers = new List<RectTransform>();
            List<Vector3> startPositions = new List<Vector3>();

            foreach (var playedCard in trick.played)
            {
                int playerIndex = playedCard.Key;
                CardData card = playedCard.Value;
                RectTransform playSlot = GetPlayTarget(playerIndex);

                FlyingCardUI flyer = Instantiate(flyingCardPrefab, canvas.transform);
                RectTransform flyerTransform = flyer.GetComponent<RectTransform>();

                flyer.SetFront(card);
                flyerTransform.position = playSlot.position;

                flyers.Add(flyerTransform);
                startPositions.Add(playSlot.position);
            }

            yield return StartCoroutine(AnimateCardsMovingToTarget(flyers, startPositions, winnerPile.position, 0.12f));

            for (int i = 0; i < flyers.Count; i++)
            {
                if (flyers[i]) Destroy(flyers[i].gameObject);
            }

            trick.played.Clear();
            ClearPlayedCardsOnTable();
        }

        void HighlightActivePlayer(int playerIndex)
        {
            ResetTurnIndicatorColors();

            if (playerIndex < 0) return;

            switch (playerIndex)
            {
                case 0: if (bottomSeatImage) bottomSeatImage.color = activeColor; break;
                case 1: if (leftSeatImage) leftSeatImage.color = activeColor; break;
                case 2: if (topSeatImage) topSeatImage.color = activeColor; break;
                case 3: if (rightSeatImage) rightSeatImage.color = activeColor; break;
            }
        }

        IEnumerator AnimateCardFlight(CardData card, Vector3 startWorldPos, RectTransform targetSeat, bool faceDown)
        {
            FlyingCardUI flyer = Instantiate(flyingCardPrefab, canvas.transform);
            RectTransform flyerTransform = flyer.GetComponent<RectTransform>();

            if (faceDown) flyer.SetBack();
            else flyer.SetFront(card);

            flyerTransform.position = startWorldPos;
            Vector3 endPosition = targetSeat.position;

            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / throwDuration;
                float smooth = Mathf.SmoothStep(0f, 1f, t);
                flyerTransform.position = Vector3.Lerp(startWorldPos, endPosition, smooth);
                yield return null;
            }

            flyerTransform.position = endPosition;
            Destroy(flyer.gameObject);
        }

        IEnumerator ResolveCompletedTrick()
        {
            yield return new WaitForSeconds(winnerDelay);

            int winner = Rules.GetTrickWinner(trick.played, trick.leadSuit.Value);
            players[winner].tricksWon++;

            yield return StartCoroutine(AnimateTrickCardsToWinner(winner));

            PlayTrickWinSound();
            UpdateScoreboardDisplay();

            leaderIndex = winner;
            isTurnRoutineRunning = false;
            BeginTrick();
        }

        void ShowHumanTurnOptions()
        {
            waitingForHuman = true;
            UpdateHumanHandDisplay(players[0].hand, true);
        }

        IEnumerator RunAiTurn()
        {
            PlayerData ai = players[currentPlayerIndex];
            CardData chosen = ChooseAiCardToPlay(ai, trick.leadSuit);

            yield return StartCoroutine(AnimateCardFromHandToTable(currentPlayerIndex, chosen));

            CommitPlayedCard(currentPlayerIndex, chosen);
            MoveToNextPlayer();
        }

        bool CanHumanPlaySelectedCard(CardData card)
        {
            if (!biddingDone) return false;
            if (waitingForNextRoundButton) return false;
            if (!waitingForHuman) return false;

            return IsCardPlayableForHuman(card);
        }

        bool CanStartTurnLoop()
        {
            return biddingDone && !waitingForNextRoundButton && !isTurnRoutineRunning;
        }

        IEnumerator AnimateCardFromHandToTable(int playerIndex, CardData card)
        {
            Vector3 startPosition = GetDealTarget(playerIndex).position;
            RectTransform playTarget = GetPlayTarget(playerIndex);

            yield return StartCoroutine(AnimateCardFlight(card, startPosition, playTarget, false));
        }

        void MoveToNextPlayer()
        {
            currentPlayerIndex = (currentPlayerIndex + 1) % 4;
        }
    }
}
