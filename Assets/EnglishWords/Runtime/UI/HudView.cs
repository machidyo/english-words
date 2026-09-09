using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;

namespace EnglishWords.Runtime.UI
{
    public sealed class HudView : MonoBehaviour
    {
        private const float MISS_MARK_SECONDS = 0.6f;
        private const char LIFE_MARK = '♥';
        private const string MISSPELLED_OPEN = "<color=#FF4B4B><size=150%>";
        private const string MISSPELLED_CLOSE = "</size></color>";

        [SerializeField] private TMP_Text _pointValue;
        [SerializeField] private TMP_Text _lifeValue;
        [SerializeField] private TMP_Text _timeValue;
        [SerializeField] private TMP_Text _inputText;
        [SerializeField] private CanvasGroupFader _missMark;

        private float _missMarkRemainingSeconds;

        public void SetPoint(int point) => _pointValue.text = point.ToString();

        public void SetLife(int life) => _lifeValue.text = new string(LIFE_MARK, Mathf.Max(life, 0));

        public void SetElapsedSeconds(float seconds) => _timeValue.text = seconds.ToString("0.0");

        private readonly StringBuilder _inputMarkup = new();

        // 綴りが食い違っている文字だけ赤く大きくして、Backspaceで直す位置を示す
        public void SetInputText(string text, IReadOnlyList<int> misspelledIndices)
        {
            if (misspelledIndices.Count == 0)
            {
                _inputText.text = text;
                return;
            }

            _inputMarkup.Clear();
            for (var i = 0; i < text.Length; i++)
            {
                if (misspelledIndices.Contains(i))
                    _inputMarkup.Append(MISSPELLED_OPEN).Append(text[i]).Append(MISSPELLED_CLOSE);
                else
                    _inputMarkup.Append(text[i]);
            }
            _inputText.text = _inputMarkup.ToString();
        }

        public void ClearInputText() => _inputText.text = string.Empty;

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
