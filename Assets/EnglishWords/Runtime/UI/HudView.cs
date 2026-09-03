using TMPro;
using UnityEngine;

namespace EnglishWords.Runtime.UI
{
    public sealed class HudView : MonoBehaviour
    {
        private const float MISS_MARK_SECONDS = 0.6f;
        private const char LIFE_MARK = '♥';

        [SerializeField] private TMP_Text _pointValue;
        [SerializeField] private TMP_Text _lifeValue;
        [SerializeField] private TMP_Text _timeValue;
        [SerializeField] private TMP_Text _inputText;
        [SerializeField] private CanvasGroupFader _missMark;

        private float _missMarkRemainingSeconds;

        public void SetPoint(int point) => _pointValue.text = point.ToString();

        public void SetLife(int life) => _lifeValue.text = new string(LIFE_MARK, Mathf.Max(life, 0));

        public void SetElapsedSeconds(float seconds) => _timeValue.text = seconds.ToString("0.0");

        public void SetInputText(string text) => _inputText.text = text;

        public void ShowMiss()
        {
            _missMark.Show();
            _missMarkRemainingSeconds = MISS_MARK_SECONDS;
            enabled = true;
        }

        private void Awake() => enabled = false;

        private void Update()
        {
            _missMarkRemainingSeconds -= Time.deltaTime;
            if (_missMarkRemainingSeconds > 0f)
                return;

            _missMark.Hide();
            enabled = false;
        }
    }
}
