using TMPro;
using UnityEngine;
using EnglishWords.Runtime.GameFlow;
using EnglishWords.Runtime.Score;

namespace EnglishWords.Runtime.UI
{
    public sealed class ResultView : MonoBehaviour
    {
        private const string HINT = "Enter: ランキングへ";

        [SerializeField] private CanvasGroupFader _fader;
        [SerializeField] private TMP_Text _headline;
        [SerializeField] private TMP_Text _pointValue;
        [SerializeField] private TMP_Text _defeatCountValue;
        [SerializeField] private TMP_Text _accuracyValue;
        [SerializeField] private TMP_Text _maxComboValue;
        [SerializeField] private TMP_Text _hint;

        public void Show(GameResult result, ScoreRecord score)
        {
            _headline.text = result == GameResult.Cleared ? "CLEAR" : "GAME OVER";
            _pointValue.text = score.Point.ToString();
            _defeatCountValue.text = score.DefeatCount.ToString();
            _accuracyValue.text = $"{score.Accuracy * 100f:0}%";
            _maxComboValue.text = score.MaxCombo.ToString();
            _hint.text = HINT;
            _fader.Show();
        }

        public void Hide() => _fader.Hide();
    }
}
