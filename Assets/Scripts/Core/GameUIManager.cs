using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public class GameUIManager : MonoBehaviour
    {
        [Header("Main UI")]
        public GameObject gameUIRoot;
        public BidUI bidUI;
        public HandUI handUI;
        public TrickUI trickUI;
        public ScoreboardUI scoreboardUI;

        [Header("Result UI")]
        public RoundResultUI roundResultUI;
        public FinalResultUI finalResultUI;

        public void RenderPlayerHand(
            List<CardData> hand,
            Action<CardData> onCardClicked,
            Func<CardData, bool> isPlayable)
        {
            if (!handUI) return;
            handUI.Render(hand, onCardClicked, isPlayable);
        }

        public void ClearPlayedCards()
        {
            if (trickUI) trickUI.Clear();
        }

        public void ShowPlayedCard(int playerIndex, CardData card)
        {
            if (trickUI) trickUI.SetCardForPlayer(playerIndex, card);
        }

        public void UpdateScoreboardDisplay(int currentRound, int maxRounds, List<PlayerData> players)
        {
            if (!scoreboardUI) return;

            scoreboardUI.SetRound(currentRound, maxRounds);
            scoreboardUI.Refresh(players);
        }

        public void ShowBidPanel()
        {
            if (bidUI) bidUI.Open();
        }

        public void HideBidPanel()
        {
            if (bidUI) bidUI.gameObject.SetActive(false);
        }

        public void ShowRoundResults(GameManager gameManager, int currentRound, int maxRounds, List<PlayerData> players)
        {
            if (roundResultUI) roundResultUI.Show(gameManager, currentRound, maxRounds, players);
        }

        public void ShowFinalResults(GameManager gameManager, int maxRounds, List<PlayerData> players)
        {
            if (finalResultUI) finalResultUI.Show(gameManager, maxRounds, players);
        }

        public void HideResultsAndShowGameUI()
        {
            if (roundResultUI && roundResultUI.panel) roundResultUI.panel.SetActive(false);
            if (finalResultUI && finalResultUI.panel) finalResultUI.panel.SetActive(false);
            if (gameUIRoot) gameUIRoot.SetActive(true);
        }
    }
}
