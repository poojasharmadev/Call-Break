using UnityEngine;
using TMPro;
using System.Collections.Generic;

namespace Core
{
    public class ScoreboardUI : MonoBehaviour
    {
        [Header("Round")]
        public TMP_Text roundText;

        [Header("Player Lines")]
        public TMP_Text bottomLine; // You
        public TMP_Text leftLine;   // P1
        public TMP_Text topLine;    // P2
        public TMP_Text rightLine;  // P3

        [Header("Turn Highlight")]
        public Color normalLineColor = Color.white;
        public Color activeLineColor = new Color(1f, 0.93f, 0.35f);
        public float normalLineScale = 1f;
        public float activeLineScale = 1.12f;

        int currentRound = 1;
        int maxRounds = 5;
        int activePlayerIndex = -1;

        public void SetRound(int round, int max)
        {
            currentRound = round;
            maxRounds = max;
            if (roundText) roundText.text = $"Round {currentRound}/{maxRounds}";
        }

        public void Refresh(List<PlayerData> players)
        {
            if (players == null || players.Count < 4) return;

            if (bottomLine) bottomLine.text = BuildLine("You", players[0]);
            if (leftLine)   leftLine.text   = BuildLine("P1", players[1]);
            if (topLine)    topLine.text    = BuildLine("P2", players[2]);
            if (rightLine)  rightLine.text  = BuildLine("P3", players[3]);

            ApplyTurnHighlight();
        }

        public void SetActivePlayerHighlight(int playerIndex)
        {
            activePlayerIndex = playerIndex;
            ApplyTurnHighlight();
        }

        string BuildLine(string name, PlayerData p)
        {
            string progress = (p.bid > 0) ? $"{p.tricksWon}/{p.bid}" : $"{p.tricksWon}/-";
            return $"{name}: {progress}";
        }

        void ApplyTurnHighlight()
        {
            ApplyLineStyle(bottomLine, activePlayerIndex == 0);
            ApplyLineStyle(leftLine, activePlayerIndex == 1);
            ApplyLineStyle(topLine, activePlayerIndex == 2);
            ApplyLineStyle(rightLine, activePlayerIndex == 3);
        }

        void ApplyLineStyle(TMP_Text line, bool isActive)
        {
            if (!line) return;

            line.color = isActive ? activeLineColor : normalLineColor;
            line.fontStyle = isActive ? FontStyles.Bold : FontStyles.Normal;
            line.rectTransform.localScale = Vector3.one * (isActive ? activeLineScale : normalLineScale);
        }
    }
}
