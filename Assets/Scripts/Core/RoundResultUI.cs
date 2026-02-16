using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Core
{
    public class RoundResultUI : MonoBehaviour
    {
        [Header("Root Panel")]
        public GameObject panel;

        [Header("Title")]
        public TMP_Text titleText;

        [Header("Progress Table (multiline)")]
        public TMP_Text tableText;

        GameManager gm;

        public void Show(GameManager gameManager, int roundNumber, int maxRounds, List<PlayerData> players)
        {
            gm = gameManager;

            if (panel) panel.SetActive(true);
            if (titleText) titleText.text = $"Round {roundNumber} Results";

            if (tableText)
            {
                tableText.enableWordWrapping = false; // columns
                tableText.text = BuildProgressTable(players, roundNumber);
            }
        }

        string BuildProgressTable(List<PlayerData> players, int roundNumber)
        {
            // roundNumber = 1..5  (show only up to that round)
            string s = "";
            s += "Player |";

            for (int r = 1; r <= roundNumber; r++)
                s += $" R{r}";

            s += " | Total\n";

            s += "------------------------------\n";

            s += LineFor(0, "You", players[0], roundNumber) + "\n";
            s += LineFor(1, "P1 ", players[1], roundNumber) + "\n";
            s += LineFor(2, "P2 ", players[2], roundNumber) + "\n";
            s += LineFor(3, "P3 ", players[3], roundNumber) + "\n";

            return s;
        }

        string LineFor(int idx, string name, PlayerData p, int roundNumber)
        {
            string s = $"{name.PadRight(4)} |";

            for (int r = 0; r < roundNumber; r++)
            {
                s += " " + Format(p.roundScores[r]);
            }

            s += $" | {p.totalScore}";
            return s;
        }

        string Format(int v)
        {
            // fixed width like +7, -3,  0
            if (v > 0) return $"+{v}".PadLeft(3);
            if (v < 0) return $"{v}".PadLeft(3);
            return " 0".PadLeft(3);
        }

        // Button OnClick
        public void OnNextRoundClicked()
        {
            if (panel) panel.SetActive(false);
            gm?.StartNextRoundFromUI();
        }
    }
}
