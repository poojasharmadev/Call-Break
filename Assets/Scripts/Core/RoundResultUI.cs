using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Core
{
    public class RoundResultUI : MonoBehaviour
    {
        [Header("Root Panel")]
        public GameObject panel;

        [Header("Hide during results")]
        public GameObject gameUIRoot;

        [Header("Texts")]
        public TMP_Text titleText;
        public TMP_Text tableText;

        [Header("Buttons")]
        public GameObject nextRoundButton;

        GameManager gm;

        public void Show(GameManager gameManager, int currentRound, int maxRounds, List<PlayerData> players)
        {
            gm = gameManager;

            if (gameUIRoot) gameUIRoot.SetActive(false);
            if (panel) panel.SetActive(true);

            if (titleText) titleText.text = $"Round {currentRound} Result";

            if (tableText)
            {
                tableText.enableWordWrapping = false;
                tableText.text = BuildTable(players, currentRound, maxRounds);
            }

            if (nextRoundButton) nextRoundButton.SetActive(currentRound < maxRounds);
        }

        public void OnNextRoundClicked()
        {
            if (panel) panel.SetActive(false);
            if (gameUIRoot) gameUIRoot.SetActive(true);

            if (gm) gm.StartNextRoundFromUI();
        }

        string BuildTable(List<PlayerData> players, int currentRound, int maxRounds)
        {
            string s = "";

            // Header
            s += "Player |";
            for (int r = 1; r <= maxRounds; r++)
                s += $"  R{r}  ";
            s += "| Total\n";

            s += "-----------------------------------------------\n";

            s += LineFor(0, players[0], currentRound, maxRounds) + "\n";
            s += LineFor(1, players[1], currentRound, maxRounds) + "\n";
            s += LineFor(2, players[2], currentRound, maxRounds) + "\n";
            s += LineFor(3, players[3], currentRound, maxRounds) + "\n";

            return s;
        }

        string LineFor(int index, PlayerData p, int currentRound, int maxRounds)
        {
            string name = (index == 0 ? "You" : $"P{index}");
            name = name.PadRight(5); // fixed width name column

            string line = $"{name} |";

            for (int r = 0; r < maxRounds; r++)
            {
                if (r < currentRound)
                    line += $"{FormatScore(p.roundScores[r])} ";
                else
                    line += "  --  ";
            }

            line += $"|{FormatScore(p.totalScore)}";
            return line;
        }

        string FormatScore(float v)
        {
            // +0.0 / -4.0 / 4.1 etc (always 1 decimal)
            string t = v.ToString("+0.0;-0.0;0.0");

            // make every cell same width
            // e.g. " +4.1" " -4.0" "  0.0"
            return t.PadLeft(5);
        }
    }
}