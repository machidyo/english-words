using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using EnglishWords.Runtime.Review;

namespace EnglishWords.Runtime.UI
{
    public sealed class ReviewView : MonoBehaviour
    {
        private const string TRACING_GUIDE = "答えを見ながら打とう";
        private const string RECALLING_GUIDE = "こんどは思い出して打とう";
        private const string FINISHING_GUIDE = "仕上げ。ヒントなしで打とう";

        [SerializeField] private CanvasGroupFader _fader;
        [SerializeField] private TMP_Text _progress;
        [SerializeField] private TMP_Text _guide;
        [SerializeField] private CanvasGroupFader _giveUpFader;
        [SerializeField] private Button _giveUpButton;

        public event Action OnGiveUpRequested;

        public void Show() => _fader.Show();

        public void Hide()
        {
            _giveUpFader.Hide();
            _fader.Hide();
        }

        public void SetProgress(int current, int total) => _progress.text = $"{current} / {total}";

        public void SetGuide(ReviewStage stage) => _guide.text = stage switch
        {
            ReviewStage.Tracing => TRACING_GUIDE,
            ReviewStage.Recalling => RECALLING_GUIDE,
            _ => FINISHING_GUIDE,
        };

        public void SetGiveUpVisible(bool isVisible)
        {
            if (isVisible)
                _giveUpFader.Show();
            else
                _giveUpFader.Hide();
        }

        private void OnEnable() => _giveUpButton.onClick.AddListener(RaiseGiveUpRequested);

        private void OnDisable() => _giveUpButton.onClick.RemoveListener(RaiseGiveUpRequested);

        private void RaiseGiveUpRequested() => OnGiveUpRequested?.Invoke();
    }
}
