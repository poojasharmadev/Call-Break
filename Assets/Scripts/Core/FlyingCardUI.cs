using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Core
{
    public class FlyingCardUI : MonoBehaviour
    {
        [Header("Front")]
        public TMP_Text label;     // A♦
        public Image frontBG;      // optional

        [Header("Back")]
        public Image backImage;    // assign a card-back sprite image here

        public void SetFront(CardData card)
        {
            ShowBack(false);

            if (label) label.text = GetShortCard(card);

            if (label)
            {
                if (card.suit == Suit.Hearts || card.suit == Suit.Diamonds)
                    label.color = Color.red;
                else
                    label.color = Color.black;
            }
        }

        public void SetBack()
        {
            ShowBack(true);
        }

        void ShowBack(bool isBack)
        {
            if (backImage) backImage.enabled = isBack;

            if (label) label.enabled = !isBack;
            if (frontBG) frontBG.enabled = !isBack;
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