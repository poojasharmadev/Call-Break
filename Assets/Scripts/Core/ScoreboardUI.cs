using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Core
{
    public class ScoreboardUI : MonoBehaviour
    {
        public TMP_Text p0; // You (Bottom)
        public TMP_Text p1; // Left
        public TMP_Text p2; // Top
        public TMP_Text p3; // Right

        public TMP_Text roundText; // optional: shows "Round 1/5"

        public void SetRound(int roundIndex, int maxRounds)
        {
            if (roundText) roundText.text = $"Round {roundIndex}/{maxRounds}";
        }

        public void Refresh(List<PlayerData> players)
        {
            if (players == null || players.Count < 4) return;

            // Display: won/bid  |  Total
            if (p0) p0.text = $"You: {players[0].tricksWon}/{players[0].bid} ";
            if (p1) p1.text = $"P1: {players[1].tricksWon}/{players[1].bid} ";
            if (p2) p2.text = $"P2: {players[2].tricksWon}/{players[2].bid}";
            if (p3) p3.text = $"P3: {players[3].tricksWon}/{players[3].bid} ";
        }
    }
}