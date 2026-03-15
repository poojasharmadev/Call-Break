using UnityEngine;
using System.Collections;

namespace Common
{
    public class LoadingUI : MonoBehaviour
    {
        public GameObject loadingPanel;
        CanvasGroup canvasGroup;
        public float fadeOutDuration = 0.5f;

        private void Awake()
        {
            if (loadingPanel != null)
            {
                canvasGroup = loadingPanel.GetComponent<CanvasGroup>();

                if (canvasGroup == null)
                    canvasGroup = loadingPanel.AddComponent<CanvasGroup>();

                canvasGroup.alpha = 0;
                loadingPanel.SetActive(false);
            }
        }

        void Start()
        {
            SceneLoader.Instance.OnSceneLoadRequested += ShowLoadingPanel;
            SceneLoader.Instance.OnSceneLoaded += HideLoadingPanel;
        }

        // INSTANT SHOW
        void ShowLoadingPanel()
        {
            if (loadingPanel == null) return;

            StopAllCoroutines();

            loadingPanel.SetActive(true);
            canvasGroup.alpha = 1f; // instant appear
        }

        // SMOOTH HIDE
        void HideLoadingPanel()
        {
            if (loadingPanel == null) return;

            StopAllCoroutines();
            StartCoroutine(FadeOut());
        }

        IEnumerator FadeOut()
        {
            float t = 0f;
            float startAlpha = 1f;
            float endAlpha = 0f;

            while (t < fadeOutDuration)
            {
                t += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, t / fadeOutDuration);
                yield return null;
            }

            canvasGroup.alpha = 0;
            loadingPanel.SetActive(false);
        }
    }
}
