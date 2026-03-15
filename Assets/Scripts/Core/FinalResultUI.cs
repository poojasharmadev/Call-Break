using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

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

        [Header("Buttons")]
        public GameObject restartButton;
        public GameObject homeButton;

        GameManager gm;

        public void Show(GameManager gameManager, int maxRounds, List<PlayerData> players)
        {
            gm = gameManager;

            if (gameUIRoot) gameUIRoot.SetActive(false);
            if (panel) panel.SetActive(true);

            if (titleText)
                titleText.text = "Final Result";

            if (tableText)
            {
                tableText.enableWordWrapping = false;
                tableText.text = BuildFinalTable(players);
            }

            if (restartButton) restartButton.SetActive(true);
            if (homeButton) homeButton.SetActive(true);
        }

        string BuildFinalTable(List<PlayerData> players)
        {
            string s = "";

            // Header
            s += string.Format("{0,-8}{1,-12}{2,-12}{3,-12}{4,-12}\n",
                "", "You", "P1", "P2", "P3");

            s += "-------------------------------------------------\n";

            // Round rows
            for (int r = 0; r < 5; r++)
            {
                s += string.Format("{0,-8}{1,-12}{2,-12}{3,-12}{4,-12}\n",
                    "R" + (r + 1),
                    Format(players[0], r),
                    Format(players[1], r),
                    Format(players[2], r),
                    Format(players[3], r));
            }
            

            s += "--------------------------------------------------\n";

            // Total
            s += string.Format("{0,-8}{1,-12}{2,-12}{3,-12}{4,-12}\n",
                "Total",
                players[0].totalScore.ToString("0.0"),
                players[1].totalScore.ToString("0.0"),
                players[2].totalScore.ToString("0.0"),
                players[3].totalScore.ToString("0.0"));

            // Rank
            string[] ranks = GetRanks(players);

            s += string.Format("{0,-8}{1,-12}{2,-12}{3,-12}{4,-12}",
                "Rank",
                ranks[0],
                ranks[1],
                ranks[2],
                ranks[3]);

            return s;
        }

        string Format(PlayerData p, int round)
        {
            float score = p.roundScores[round];

            // if failed bid
            if (score < 0)
            {
                return CircleBid(p.bid);
            }

            return score.ToString("0.0");
        }

        string[] GetRanks(List<PlayerData> players)
        {
            string[] result = new string[4];

            List<int> order = new List<int> { 0, 1, 2, 3 };
            order.Sort((a, b) => players[b].totalScore.CompareTo(players[a].totalScore));

            result[order[0]] = "1st";
            result[order[1]] = "2nd";
            result[order[2]] = "3rd";
            result[order[3]] = "4th";

            return result;
        }
        
        string CircleBid(int bid)
        {
            return "(" + bid + ")";
        }

        public void OnRestartClicked()
        {
            if (panel) panel.SetActive(false);
            if (gameUIRoot) gameUIRoot.SetActive(true);

            if (gm) gm.RestartMatch();
        }

        public void OnHomeClicked()
        {
            SceneManager.LoadScene("MainMenu");
        }
    }
}