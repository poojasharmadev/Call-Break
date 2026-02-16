using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public class HandUI : MonoBehaviour
    {
        public Transform container;
        public CardButtonUI cardButtonPrefab;

        readonly List<CardButtonUI> spawned = new List<CardButtonUI>();

        public void Clear()
        {
            for (int i = 0; i < spawned.Count; i++)
                Destroy(spawned[i].gameObject);
            spawned.Clear();
        }

        public void Render(List<CardData> hand,
            System.Action<CardData> onCardClicked,
            System.Func<CardData, bool> isPlayable)
        {
            Clear();

            for (int i = 0; i < hand.Count; i++)
            {
                var btn = Instantiate(cardButtonPrefab, container);
                btn.Setup(hand[i], onCardClicked);

                bool playable = (isPlayable == null) ? true : isPlayable(hand[i]);
                btn.SetInteractable(playable);

                spawned.Add(btn);
            }
        }

        public void SetAllInteractable(bool value)
        {
            for (int i = 0; i < spawned.Count; i++)
                spawned[i].SetInteractable(value);
        }
    }
}