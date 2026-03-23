using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public partial class GameManager
    {
        void ResetPlayerMatchScore(PlayerData player)
        {
            player.totalScore = 0f;
            player.lastRoundScore = 0f;
            player.tricksWon = 0;
            player.bid = 0;
        }

        void ResetPlayerRoundStateForNewRound(PlayerData player)
        {
            player.bid = 0;
            player.tricksWon = 0;
            player.lastRoundScore = 0f;
        }

        int GetStartingPlayerIndexForRound()
        {
            return (currentRound - 1) % 4;
        }

        void UpdateScoreboardDisplay()
        {
            if (uiManager)
            {
                uiManager.UpdateScoreboardDisplay(currentRound, maxRounds, players);
                return;
            }

            if (scoreboardUI)
            {
                scoreboardUI.SetRound(currentRound, maxRounds);
                scoreboardUI.Refresh(players);
            }
        }

        void ClearPlayedCardsOnTable()
        {
            if (uiManager)
            {
                uiManager.ClearPlayedCards();
                return;
            }

            if (trickUI) trickUI.Clear();
        }

        void UpdateHumanHandDisplay(List<CardData> hand, bool showPlayableCards)
        {
            Func<CardData, bool> playRule =
                showPlayableCards ? (Func<CardData, bool>)IsCardPlayableForHuman : DisableCardSelection;

            if (uiManager)
            {
                uiManager.RenderPlayerHand(hand, HandleHumanCardSelected, playRule);
                return;
            }

            if (!handUI) return;

            handUI.Render(
                hand,
                HandleHumanCardSelected,
                playRule
            );
        }

        bool DisableCardSelection(CardData card)
        {
            return false;
        }

        bool IsCardPlayableForHuman(CardData card)
        {
            return Rules.IsLegalMove(players[0].hand, card, trick.leadSuit, trick.played);
        }

        bool HasAllDealAnimationReferences()
        {
            return deckPoint && canvas && flyingCardPrefab && bottomSeat && leftSeat && topSeat && rightSeat;
        }

        IEnumerator AnimateCardDealToPlayer(int playerIndex, CardData card)
        {
            bool faceDown = playerIndex != 0;
            yield return StartCoroutine(AnimateCardFlight(card, deckPoint.position, GetDealTarget(playerIndex), faceDown));
        }

        IEnumerator AnimateCardsMovingToTarget(
            List<RectTransform> flyers,
            List<Vector3> startPositions,
            Vector3 targetPosition,
            float duration)
        {
            float t = 0f;

            while (t < 1f)
            {
                t += Time.deltaTime / duration;
                float smooth = Mathf.SmoothStep(0f, 1f, t);

                for (int i = 0; i < flyers.Count; i++)
                {
                    if (flyers[i])
                    {
                        flyers[i].position = Vector3.Lerp(startPositions[i], targetPosition, smooth);
                    }
                }

                yield return null;
            }
        }

        float CalculateRoundScoreForPlayer(PlayerData player)
        {
            if (player.tricksWon >= player.bid)
            {
                int extra = player.tricksWon - player.bid;
                return player.bid + (extra * 0.1f);
            }

            return -player.bid;
        }

        void HideResultsAndShowGameUI()
        {
            if (uiManager)
            {
                uiManager.HideResultsAndShowGameUI();
                return;
            }

            if (finalResultUI && finalResultUI.panel) finalResultUI.panel.SetActive(false);
            if (roundResultUI && roundResultUI.panel) roundResultUI.panel.SetActive(false);
            if (finalResultUI && finalResultUI.gameUIRoot) finalResultUI.gameUIRoot.SetActive(true);
        }

        void ResetTurnIndicatorColors()
        {
            if (bottomSeatImage) bottomSeatImage.color = normalColor;
            if (leftSeatImage) leftSeatImage.color = normalColor;
            if (topSeatImage) topSeatImage.color = normalColor;
            if (rightSeatImage) rightSeatImage.color = normalColor;
        }

        void PlayCardThrowSound()
        {
            if (SFXManager.I) SFXManager.I.PlayCardThrow();
        }

        void PlayTrickWinSound()
        {
            if (SFXManager.I) SFXManager.I.PlayTrickWin();
        }

        void PlayRoundEndSound()
        {
            if (SFXManager.I) SFXManager.I.PlayRoundEnd();
        }

        void PlayFinalResultSound()
        {
            if (SFXManager.I) SFXManager.I.PlayFinal();
        }
    }
}
