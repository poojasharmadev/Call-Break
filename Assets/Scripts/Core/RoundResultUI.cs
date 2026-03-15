using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Core
{
    public class RoundResultUI : MonoBehaviour
    {
        public GameObject panel;
        public GameObject gameUIRoot;
        public TMP_Text titleText;
        public GameObject nextRoundButton;

        [Header("Header")]
        public TMP_Text youHeader;
        public TMP_Text p1Header;
        public TMP_Text p2Header;
        public TMP_Text p3Header;

        [Header("R1")]
        public TMP_Text r1You;
        public TMP_Text r1P1;
        public TMP_Text r1P2;
        public TMP_Text r1P3;

        [Header("R2")]
        public TMP_Text r2You;
        public TMP_Text r2P1;
        public TMP_Text r2P2;
        public TMP_Text r2P3;

        [Header("R3")]
        public TMP_Text r3You;
        public TMP_Text r3P1;
        public TMP_Text r3P2;
        public TMP_Text r3P3;

        [Header("R4")]
        public TMP_Text r4You;
        public TMP_Text r4P1;
        public TMP_Text r4P2;
        public TMP_Text r4P3;

        [Header("R5")]
        public TMP_Text r5You;
        public TMP_Text r5P1;
        public TMP_Text r5P2;
        public TMP_Text r5P3;

        [Header("Total")]
        public TMP_Text totYou;
        public TMP_Text totP1;
        public TMP_Text totP2;
        public TMP_Text totP3;

        GameManager gm;

        public void Show(GameManager gameManager, int currentRound, int maxRounds, List<PlayerData> players)
        {
            gm = gameManager;

            if (gameUIRoot) gameUIRoot.SetActive(false);
            if (panel) panel.SetActive(true);
            if (titleText) titleText.text = $"Round {currentRound} Result";

            if (youHeader) youHeader.text = "You";
            if (p1Header) p1Header.text = "P1";
            if (p2Header) p2Header.text = "P2";
            if (p3Header) p3Header.text = "P3";

            SetRow(currentRound, 1, players, r1You, r1P1, r1P2, r1P3);
            SetRow(currentRound, 2, players, r2You, r2P1, r2P2, r2P3);
            SetRow(currentRound, 3, players, r3You, r3P1, r3P2, r3P3);
            SetRow(currentRound, 4, players, r4You, r4P1, r4P2, r4P3);
            SetRow(currentRound, 5, players, r5You, r5P1, r5P2, r5P3);

            if (totYou) totYou.text = players[0].totalScore.ToString("0.0");
            if (totP1) totP1.text = players[1].totalScore.ToString("0.0");
            if (totP2) totP2.text = players[2].totalScore.ToString("0.0");
            if (totP3) totP3.text = players[3].totalScore.ToString("0.0");

            if (nextRoundButton)
                nextRoundButton.SetActive(currentRound < maxRounds);
        }

        void SetRow(int currentRound, int rowNumber, List<PlayerData> players,
            TMP_Text a, TMP_Text b, TMP_Text c, TMP_Text d)
        {
            if (rowNumber <= currentRound)
            {
                int i = rowNumber - 1;

                if (a) a.text = FormatRoundValue(players[0], i);
                if (b) b.text = FormatRoundValue(players[1], i);
                if (c) c.text = FormatRoundValue(players[2], i);
                if (d) d.text = FormatRoundValue(players[3], i);
            }
            else
            {
                if (a) a.text = "-";
                if (b) b.text = "-";
                if (c) c.text = "-";
                if (d) d.text = "-";
            }
        }

        string FormatRoundValue(PlayerData player, int roundIndex)
        {
            float score = player.roundScores[roundIndex];

            // failed bid -> show circled bid instead of negative score
            if (score < 0)
                return CircleBid(player.bid);

            return score.ToString("0.0");
        }

        string CircleBid(int bid)
        {
            return "(" + bid + ")";
        }

        public void OnNextRoundClicked()
        {
            if (panel) panel.SetActive(false);
            if (gameUIRoot) gameUIRoot.SetActive(true);
            if (gm) gm.StartNextRoundFromUI();
        }
    }
}