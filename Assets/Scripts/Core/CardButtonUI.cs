using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Core
{
    public class CardButtonUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text label;
        [SerializeField] private Button button;

        CardData card;
        System.Action<CardData> onClick;

        public void Setup(CardData card, System.Action<CardData> onClick)
        {
            this.card = card;
            this.onClick = onClick;

            if (button == null)
                button = GetComponent<Button>();

            if (label != null)
            {
                label.text = GetShortCard(card);

                // ✅ Make Hearts & Diamonds RED
                if (card.suit == Suit.Hearts || card.suit == Suit.Diamonds)
                    label.color = Color.red;
                else
                    label.color = Color.black;
            }
        }

        public void SetInteractable(bool value)
        {
            if (button == null)
                button = GetComponent<Button>();

            button.interactable = value;
        }

        public void Click()
        {
            onClick?.Invoke(card);
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