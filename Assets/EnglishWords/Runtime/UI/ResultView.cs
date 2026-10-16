using TMPro;
using UnityEngine;
using EnglishWords.Runtime.GameFlow;
using EnglishWords.Runtime.Score;

namespace EnglishWords.Runtime.UI
{
    public sealed class ResultView : MonoBehaviour
    {
        private const string REVIEW_HINT = "Enter: 復習する  /  Esc: タイトルへ";
        private const string RETRY_HINT = "Enter: もう一度  /  Esc: タイトルへ";

        [SerializeField] private CanvasGroupFader _fader;
        [SerializeField] private TMP_Text _headline;
        [SerializeField] private TMP_Text _pointValue;
        [SerializeField] private TMP_Text _defeatCountValue;
        [SerializeField] private TMP_Text _accuracyValue;
        [SerializeField] private TMP_Text _maxComboValue;
        [SerializeField] private TMP_Text _hint;

        public void Show(GameResult result, ScoreRecord score, bool hasWordsToReview)
        {
            _headline.text = result == GameResult.Cleared ? "CLEAR" : "GAME OVER";
            _pointValue.text = score.Point.ToString();
            _defeatCountValue.text = score.DefeatCount.ToString();
            _accuracyValue.text = $"{score.Accuracy * 100f:0}%";
            _maxComboValue.text = score.MaxCombo.ToString();
            _hint.text = hasWordsToReview ? REVIEW_HINT : RETRY_HINT;
            _fader.Show();
        }

        // 復習のあいだは引っ込め、終わったら同じ内容で出し直す
        public void Hide() => _fader.Hide();
    }
}
