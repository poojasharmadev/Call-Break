using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Core
{
    public class FlyingCardUI : MonoBehaviour
    {
        public TMP_Text label;
        public Image bg;

        public void Set(CardData card)
        {
            if (label) label.text = ShortName(card);

            if (label)
            {
                if (card.suit == Suit.Hearts || card.suit == Suit.Diamonds)
                    label.color = Color.red;
                else
                    label.color = Color.black;
            }
        }

        string ShortName(CardData c)
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