using UnityEngine;
using TMPro;

namespace Core
{
    public class TrickUI : MonoBehaviour
    {
        [Header("Seat Texts")]
        public TMP_Text bottomText; // Player 0
        public TMP_Text leftText;   // Player 1
        public TMP_Text topText;    // Player 2
        public TMP_Text rightText;  // Player 3

        public void Clear()
        {
            if (bottomText) bottomText.text = "";
            if (leftText) leftText.text = "";
            if (topText) topText.text = "";
            if (rightText) rightText.text = "";
        }

        public void SetCardForPlayer(int playerIndex, CardData card)
        {
            TMP_Text target = GetSeatText(playerIndex);
            if (target == null) return;

            target.text = GetShortCard(card);

            // color: ♦♥ red, ♠♣ black
            if (card.suit == Suit.Hearts || card.suit == Suit.Diamonds)
                target.color = Color.red;
            else
                target.color = Color.black;
        }

        TMP_Text GetSeatText(int playerIndex)
        {
            switch (playerIndex)
            {
                case 0: return bottomText; // You
                case 1: return leftText;
                case 2: return topText;
                case 3: return rightText;
                default: return null;
            }
        }

        string GetShortCard(CardData c)
        {
            string rank =
                c.rank == Rank.Ace ? "A" :
                c.rank == Rank.King ? "K" :
                c.rank == Rank.Queen ? "Q" :
                c.rank == Rank.Jack ? "J" :
                ((int)c.rank).ToString();

            string suit =
                c.suit == Suit.Spades ? "♠" :
                c.suit == Suit.Hearts ? "♥" :
                c.suit == Suit.Diamonds ? "♦" :
                "♣";

            return rank + suit;
        }
    }
}