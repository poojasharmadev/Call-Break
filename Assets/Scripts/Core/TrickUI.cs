using UnityEngine;
using TMPro;

namespace Core
{
    public class TrickUI : MonoBehaviour
    {
        [Header("Table Slots (CENTER)")]
        public TMP_Text slot0; // player0 table position
        public TMP_Text slot1; // player1 table position
        public TMP_Text slot2; // player2 table position
        public TMP_Text slot3; // player3 table position

        public void Clear()
        {
            if (slot0) slot0.text = "";
            if (slot1) slot1.text = "";
            if (slot2) slot2.text = "";
            if (slot3) slot3.text = "";
        }

        public void SetCardForPlayer(int playerIndex, CardData card)
        {
            TMP_Text t = GetSlotText(playerIndex);
            if (!t) return;

            t.text = GetShortCard(card);

            // ♦♥ red, ♠♣ black
            if (card.suit == Suit.Hearts || card.suit == Suit.Diamonds)
                t.color = Color.red;
            else
                t.color = Color.black;
        }

        TMP_Text GetSlotText(int playerIndex)
        {
            switch (playerIndex)
            {
                case 0: return slot0;
                case 1: return slot1;
                case 2: return slot2;
                case 3: return slot3;
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