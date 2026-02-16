using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Core
{
    public class FinalResultUI : MonoBehaviour
    {
        [Header("Root Panel")]
        public GameObject panel;

        [Header("Hide during results")]
        public GameObject gameUIRoot;

        [Header("Texts")]
        public TMP_Text titleText;
        public TMP_Text tableText;
        public TMP_Text rankingText;

        // ✅ store GM so restart button works
        GameManager gm;

        // ✅ IMPORTANT: now Show takes GameManager
        public void Show(GameManager gameManager, int maxRounds, List<PlayerData> players)
        {
            gm = gameManager;

            if (gameUIRoot) gameUIRoot.SetActive(false);
            if (panel) panel.SetActive(true);

            if (titleText) titleText.text = "Final Results";

            if (tableText)
            {
                tableText.enableWordWrapping = false;
                tableText.text = BuildTable(players, maxRounds);
            }

            if (rankingText)
            {
                rankingText.text = BuildRanking(players);
            }
        }

        // ✅ Button OnClick will call this (no parameters)
        public void OnRestartClicked()
        {
            gm?.RestartMatch();
        }

        string BuildTable(List<PlayerData> players, int maxRounds)
        {
            string s = "";
            s += "Player | R1  R2  R3  R4  R5 | Total\n";
            s += "-----------------------------------\n";

            s += LineFor("You", players[0]) + "\n";
            s += LineFor("P1 ", players[1]) + "\n";
            s += LineFor("P2 ", players[2]) + "\n";
            s += LineFor("P3 ", players[3]) + "\n";

            return s;
        }

        string LineFor(string name, PlayerData p)
        {
            string r1 = Format(p.roundScores[0]);
            string r2 = Format(p.roundScores[1]);
            string r3 = Format(p.roundScores[2]);
            string r4 = Format(p.roundScores[3]);
            string r5 = Format(p.roundScores[4]);

            return $"{name.PadRight(3)}   | {r1} {r2} {r3} {r4} {r5} | {p.totalScore}";
        }

        string Format(int v)
        {
            if (v > 0) return $"+{v}".PadLeft(3);
            if (v < 0) return $"{v}".PadLeft(3);
            return " 0".PadLeft(3);
        }

        string BuildRanking(List<PlayerData> players)
        {
            List<int> order = new List<int> { 0, 1, 2, 3 };
            order.Sort((a, b) => players[b].totalScore.CompareTo(players[a].totalScore));

            string Name(int i) => i == 0 ? "You" : $"P{i}";

            return
                $"1st: {Name(order[0])} ({players[order[0]].totalScore})\n" +
                $"2nd: {Name(order[1])} ({players[order[1]].totalScore})\n" +
                $"3rd: {Name(order[2])} ({players[order[2]].totalScore})\n" +
                $"4th: {Name(order[3])} ({players[order[3]].totalScore})";
        }
    }
}
