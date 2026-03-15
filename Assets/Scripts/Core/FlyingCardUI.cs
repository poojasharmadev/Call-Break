using Database;
using UnityEngine;
using UnityEngine.UI;

namespace Core
{
    public class FlyingCardUI : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Image frontImage;
        [SerializeField] private Image backImage;

        public void SetFront(CardData card)
        {
            if (CardDatabaseHolder.Instance == null)
            {
                Debug.LogError("CardDatabaseHolder instance not found in scene.", this);
                return;
            }

            Sprite sprite = CardDatabaseHolder.Instance.GetFront(card);

            if (sprite == null)
                return;

            if (frontImage != null)
                frontImage.sprite = sprite;

            ShowBack(false);
        }

        public void SetBack()
        {
            if (CardDatabaseHolder.Instance == null)
            {
                Debug.LogError("CardDatabaseHolder instance not found in scene.", this);
                return;
            }

            Sprite sprite = CardDatabaseHolder.Instance.GetBack();

            if (backImage != null)
                backImage.sprite = sprite;

            ShowBack(true);
        }

        private void ShowBack(bool showBack)
        {
            if (backImage != null)
                backImage.enabled = showBack;

            if (frontImage != null)
                frontImage.enabled = !showBack;
        }
    }
}