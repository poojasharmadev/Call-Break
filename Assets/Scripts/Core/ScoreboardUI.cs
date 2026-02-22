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

        int currentRound = 1;
        int maxRounds = 5;

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
        }

        string BuildLine(string name, PlayerData p)
        {
            // Only show progress like 1/3
            // If bid is 0 during bidding, show "0/-" instead of "0/0"
            string progress = (p.bid > 0) ? $"{p.tricksWon}/{p.bid}" : $"{p.tricksWon}/-";

            return $"{name}: {progress}";
        }
    }
}