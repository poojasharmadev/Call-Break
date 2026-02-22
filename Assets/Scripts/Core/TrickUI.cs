using UnityEngine;
using TMPro;

namespace Core
{
    public class TrickUI : MonoBehaviour
    {
        public RectTransform bottomSlot;
        public RectTransform leftSlot;
        public RectTransform topSlot;
        public RectTransform rightSlot;

        public TMP_Text bottomText;
        public TMP_Text leftText;
        public TMP_Text topText;
        public TMP_Text rightText;

        public void Clear()
        {
            bottomText.text = "";
            leftText.text = "";
            topText.text = "";
            rightText.text = "";
        }

        public RectTransform GetSlot(int playerIndex)
        {
            switch (playerIndex)
            {
                case 0: return bottomSlot;
                case 1: return leftSlot;
                case 2: return topSlot;
                case 3: return rightSlot;
            }
            return bottomSlot;
        }

        public void SetCardForPlayer(int playerIndex, CardData card)
        {
            TMP_Text target = GetSeatText(playerIndex);
            target.text = GetShortCard(card);

            if (card.suit == Suit.Hearts || card.suit == Suit.Diamonds)
                target.color = Color.red;
            else
                target.color = Color.black;
        }

        TMP_Text GetSeatText(int playerIndex)
        {
            switch (playerIndex)
            {
                case 0: return bottomText;
                case 1: return leftText;
                case 2: return topText;
                case 3: return rightText;
            }
            return null;
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