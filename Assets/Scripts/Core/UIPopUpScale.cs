using System.Collections;
using UnityEngine;

namespace Core
{
    public class UIPopUpScale : MonoBehaviour
    {
        [Header("Animation")]
        public float duration = 0.18f;
        public float startScale = 0.7f;

        [Header("Optional")]
        public bool playOnEnable = true;

        RectTransform rt;
        Coroutine co;

        void Awake()
        {
            rt = transform as RectTransform;
        }

        void OnEnable()
        {
            if (playOnEnable) Play();
        }

        public void Play()
        {
            if (!rt) rt = transform as RectTransform;

            if (co != null) StopCoroutine(co);
            co = StartCoroutine(Popup());
        }

        IEnumerator Popup()
        {
            float t = 0f;

            rt.localScale = Vector3.one * startScale;

            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / duration; // ✅ works even if Time.timeScale=0
                float s = Mathf.SmoothStep(startScale, 1f, t);
                rt.localScale = Vector3.one * s;
                yield return null;
            }

            rt.localScale = Vector3.one;
            co = null;
        }
    }
}