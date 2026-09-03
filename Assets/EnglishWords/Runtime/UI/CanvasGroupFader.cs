using UnityEngine;

namespace EnglishWords.Runtime.UI
{
    public sealed class CanvasGroupFader : MonoBehaviour
    {
        private const float FADE_DURATION_SECONDS = 0.1f;

        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private bool _isVisibleOnAwake;

        private float _targetAlpha;

        public void Show() => SetVisible(true);

        public void Hide() => SetVisible(false);

        private void SetVisible(bool isVisible)
        {
            _targetAlpha = isVisible ? 1f : 0f;
            _canvasGroup.blocksRaycasts = isVisible;
            enabled = true;
        }

        private void Awake()
        {
            _targetAlpha = _isVisibleOnAwake ? 1f : 0f;
            _canvasGroup.alpha = _targetAlpha;
            _canvasGroup.blocksRaycasts = _isVisibleOnAwake;
            enabled = false;
        }

        private void Update()
        {
            var alpha = Mathf.MoveTowards(_canvasGroup.alpha, _targetAlpha, Time.deltaTime / FADE_DURATION_SECONDS);
            _canvasGroup.alpha = alpha;
            if (Mathf.Approximately(alpha, _targetAlpha))
                enabled = false;
        }
    }
}
