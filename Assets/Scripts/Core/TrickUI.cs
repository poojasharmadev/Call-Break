using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Database;
using System.Collections.Generic;

namespace Core
{
    public class TrickUI : MonoBehaviour
    {
        [Header("Table Slots (CENTER)")]
        public TMP_Text slot0; // player0 table position
        public TMP_Text slot1; // player1 table position
        public TMP_Text slot2; // player2 table position
        public TMP_Text slot3; // player3 table position
        
        readonly Dictionary<Image, Sprite> defaultSprites = new Dictionary<Image, Sprite>();
        readonly Dictionary<Image, Color> defaultColors = new Dictionary<Image, Color>();

        public void Clear()
        {
            ClearSlot(slot0);
            ClearSlot(slot1);
            ClearSlot(slot2);
            ClearSlot(slot3);
        }

        public void SetCardForPlayer(int playerIndex, CardData card)
        {
            TMP_Text t = GetSlotText(playerIndex);
            if (!t) return;

            bool spriteAssigned = TrySetSlotSprite(t, card);
            t.text = spriteAssigned ? string.Empty : GetShortCard(card);

            // ♦♥ red, ♠♣ black
            if (card.suit == Suit.Hearts || card.suit == Suit.Diamonds)
                t.color = Color.red;
            else
                t.color = Color.black;
        }

        void ClearSlot(TMP_Text textSlot)
        {
            if (textSlot != null)
                textSlot.text = string.Empty;

            Image img = GetSlotImage(textSlot);
            if (img == null)
                return;

            if (defaultSprites.TryGetValue(img, out Sprite defaultSprite))
                img.sprite = defaultSprite;
            else
                img.sprite = null;

            if (defaultColors.TryGetValue(img, out Color defaultColor))
                img.color = defaultColor;
            else
                img.color = Color.white;
        }

        bool TrySetSlotSprite(TMP_Text textSlot, CardData card)
        {
            Image img = GetSlotImage(textSlot);
            if (img == null || CardDatabaseHolder.Instance == null)
                return false;

            Sprite sprite = CardDatabaseHolder.Instance.GetFront(card);
            if (sprite == null)
                return false;

            img.sprite = sprite;
            img.preserveAspect = true;
            img.color = Color.white;
            return true;
        }

        Image GetSlotImage(TMP_Text textSlot)
        {
            if (textSlot == null || textSlot.transform.parent == null)
                return null;

            Image img = textSlot.transform.parent.GetComponent<Image>();
            if (img == null)
                return null;

            CacheDefaultSlotVisual(img);
            return img;
        }

        void CacheDefaultSlotVisual(Image img)
        {
            if (img == null || defaultSprites.ContainsKey(img))
                return;

            defaultSprites[img] = img.sprite;
            defaultColors[img] = img.color;
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
