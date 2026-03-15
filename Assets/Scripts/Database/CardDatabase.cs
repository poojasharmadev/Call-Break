using System;
using System.Collections.Generic;
using Core;
using UnityEngine;

namespace Database
{
    [CreateAssetMenu(fileName = "CardDatabase", menuName = "Cards/Card Database")]
    public class CardDatabase : ScriptableObject
    {
        [Header("All 52 Card Front Sprites")]
        [SerializeField] private List<CardSpriteEntry> cardEntries = new List<CardSpriteEntry>();

        [Header("Common Card Back")]
        [SerializeField] private Sprite cardBackSprite;

        private Dictionary<string, Sprite> spriteLookup;

        private void OnEnable()
        {
            BuildLookup();
        }

        private void BuildLookup()
        {
            spriteLookup = new Dictionary<string, Sprite>();

            for (int i = 0; i < cardEntries.Count; i++)
            {
                CardSpriteEntry entry = cardEntries[i];

                if (entry == null || entry.sprite == null)
                    continue;

                string key = GetKey(entry.suit, entry.rank);

                if (!spriteLookup.ContainsKey(key))
                {
                    spriteLookup.Add(key, entry.sprite);
                }
                else
                {
                    Debug.LogWarning($"Duplicate card entry found: {key}", this);
                }
            }
        }

        public Sprite GetFrontSprite(CardData card)
        {
            return GetFrontSprite(card.suit, card.rank);
        }

        public Sprite GetFrontSprite(Suit suit, Rank rank)
        {
            EnsureLookup();

            string key = GetKey(suit, rank);

            if (spriteLookup.TryGetValue(key, out Sprite sprite))
                return sprite;

            Debug.LogWarning($"Front sprite not found for card: {key}", this);
            return null;
        }

        public bool TryGetFrontSprite(CardData card, out Sprite sprite)
        {
            return TryGetFrontSprite(card.suit, card.rank, out sprite);
        }

        public bool TryGetFrontSprite(Suit suit, Rank rank, out Sprite sprite)
        {
            EnsureLookup();
            return spriteLookup.TryGetValue(GetKey(suit, rank), out sprite);
        }

        public Sprite GetBackSprite()
        {
            return cardBackSprite;
        }

        public Sprite GetSpriteByName(string cardName)
        {
            EnsureLookup();

            if (string.IsNullOrWhiteSpace(cardName))
            {
                Debug.LogWarning("Card name is null or empty.", this);
                return null;
            }

            string key = cardName.Trim();

            if (spriteLookup.TryGetValue(key, out Sprite sprite))
                return sprite;

            Debug.LogWarning($"Sprite not found for name: {key}", this);
            return null;
        }

        public bool HasCard(CardData card)
        {
            EnsureLookup();
            return spriteLookup.ContainsKey(card.Key);
        }

        public List<CardData> GetAllCards()
        {
            List<CardData> cards = new List<CardData>();

            foreach (Suit suit in System.Enum.GetValues(typeof(Suit)))
            {
                foreach (Rank rank in System.Enum.GetValues(typeof(Rank)))
                {
                    cards.Add(new CardData(suit, rank));
                }
            }

            return cards;
        }

        public static string GetKey(Suit suit, Rank rank)
        {
            return $"{suit}_{rank}";
        }

        private void EnsureLookup()
        {
            if (spriteLookup == null || spriteLookup.Count == 0)
                BuildLookup();
        }
    }
    [Serializable]
    public class CardSpriteEntry
    {
        public Suit suit;
        public Rank rank;
        public Sprite sprite;

        public string Key => $"{suit}_{rank}";
    }
}