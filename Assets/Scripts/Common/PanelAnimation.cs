using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;
namespace Common
{
    public class PanelAnimation : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] RectTransform target; 

        [Header("Timing")]
        [SerializeField] float duration = 0.35f;

        [Header("Little Bounce Ease")]
        [SerializeField] Ease openEase = Ease.OutBack;
        [SerializeField] float openOvershoot = 0.4f;   // tiny bounce
        [SerializeField] Ease closeEase = Ease.InBack;
        [SerializeField] float closeOvershoot = 0.3f;  // tiny bounce

        [Header("Optional Fade")]
        [SerializeField] bool useFade = true;
        [SerializeField] float fadeDuration = 0.25f;

        public UnityEvent onOpen;
        public UnityEvent onClose;
        public UnityEvent onOpenComplete;
        public UnityEvent onCloseComplete;

        Vector2 _centerPos;
        Vector2 _abovePos;
        RectTransform _canvasRect;
        CanvasGroup cg;

        void Awake()
        {
            if (!target) target = GetComponent<RectTransform>();
            cg = target.GetComponent<CanvasGroup>();

            if (useFade && !cg)
                cg = target.gameObject.AddComponent<CanvasGroup>();

            Canvas c = target.GetComponentInParent<Canvas>();
            _canvasRect = c ? c.rootCanvas.GetComponent<RectTransform>() : null;

            _centerPos = target.anchoredPosition;

            float travel = (_canvasRect ? _canvasRect.rect.height : Screen.height) + target.rect.height;
            _abovePos = _centerPos + Vector2.up * travel;
        }

        [Button]
        public void OpenPanel() => Open();

        Tween Open()
        {
            target.gameObject.SetActive(true);
            target.DOKill();
            target.anchoredPosition = _abovePos;

            if (useFade)
            {
                cg.alpha = 0;
                cg.DOFade(1, fadeDuration).SetUpdate(true);
            }

            onOpen?.Invoke();

            var t = target
                .DOAnchorPos(_centerPos, duration)
                .SetEase(openEase, openOvershoot)  // soft bounce
                .SetUpdate(true)
                .OnComplete(() => onOpenComplete?.Invoke());

            return t;
        }

        [Button]
        public void ClosePanel() => Close();

        Tween Close()
        {
            target.DOKill();
            onClose?.Invoke();

            if (useFade)
                cg.DOFade(0, fadeDuration).SetUpdate(true);

            var t = target
                .DOAnchorPos(_abovePos, duration)
                .SetEase(closeEase, closeOvershoot) // soft bounce
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    target.gameObject.SetActive(false);
                    onCloseComplete?.Invoke();
                });

            return t;
        }

        public void Recalculate()
        {
            _centerPos = target.anchoredPosition;
            float travel = (_canvasRect ? _canvasRect.rect.height : Screen.height) + target.rect.height;
            _abovePos = _centerPos + Vector2.up * travel;
        }

        void OnDisable() => target.DOKill();
    }
}
