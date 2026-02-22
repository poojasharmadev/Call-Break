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

        [Header("Buttons")]
        public GameObject restartButton; // assign

        GameManager gm;

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
                rankingText.text = BuildRanking(players);

            if (restartButton) restartButton.SetActive(true);
        }

        public void OnRestartClicked()
        {
            if (panel) panel.SetActive(false);
            if (gameUIRoot) gameUIRoot.SetActive(true);

            if (gm) gm.RestartMatch();
        }

        string BuildTable(List<PlayerData> players, int maxRounds)
        {
            string s = "";
            s += "Player | R1    R2    R3    R4    R5  | Total\n";
            s += "-------------------------------------------\n";

            s += LineFor("You ", players[0]) + "\n";
            s += LineFor("P1  ", players[1]) + "\n";
            s += LineFor("P2  ", players[2]) + "\n";
            s += LineFor("P3  ", players[3]) + "\n";

            return s;
        }

        string LineFor(string name, PlayerData p)
        {
            string r1 = Format(p.roundScores[0]);
            string r2 = Format(p.roundScores[1]);
            string r3 = Format(p.roundScores[2]);
            string r4 = Format(p.roundScores[3]);
            string r5 = Format(p.roundScores[4]);

            return $"{name} | {r1} {r2} {r3} {r4} {r5} | {p.totalScore.ToString("0.0")}";
        }

        string Format(float v)
        {
            string t = v.ToString("0.0");
            if (t.Length < 5) t = t.PadLeft(5);
            return t;
        }

        string BuildRanking(List<PlayerData> players)
        {
            List<int> order = new List<int> { 0, 1, 2, 3 };
            order.Sort((a, b) => players[b].totalScore.CompareTo(players[a].totalScore));

            string Name(int i) => i == 0 ? "You" : $"P{i}";

            return
                $"1st: {Name(order[0])} ({players[order[0]].totalScore:0.0})\n" +
                $"2nd: {Name(order[1])} ({players[order[1]].totalScore:0.0})\n" +
                $"3rd: {Name(order[2])} ({players[order[2]].totalScore:0.0})\n" +
                $"4th: {Name(order[3])} ({players[order[3]].totalScore:0.0})";
        }
    }
}