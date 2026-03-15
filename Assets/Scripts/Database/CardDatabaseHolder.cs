using Core;
using UnityEngine;

namespace Database
{
    public class CardDatabaseHolder : MonoBehaviour
    {
        public static CardDatabaseHolder Instance { get; private set; }

        [SerializeField] private CardDatabase cardDatabase;

        public CardDatabase Database => cardDatabase;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public Sprite GetFront(CardData card)
        {
            if (cardDatabase == null)
            {
                Debug.LogError("CardDatabase is missing.", this);
                return null;
            }

            return cardDatabase.GetFrontSprite(card);
        }

        public Sprite GetBack()
        {
            if (cardDatabase == null)
            {
                Debug.LogError("CardDatabase is missing.", this);
                return null;
            }

            return cardDatabase.GetBackSprite();
        }
    }
}