using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using EnglishWords.Runtime.Score;

namespace EnglishWords.Runtime.UI
{
    public sealed class RankingView : MonoBehaviour
    {
        private const string REVIEW_HINT = "Enter: 復習する  /  Esc: タイトルへ";
        private const string RETRY_HINT = "Enter: もう一度  /  Esc: タイトルへ";
        private const string EMPTY_VALUE = "--";

        [SerializeField] private CanvasGroupFader _fader;
        [SerializeField] private TMP_Text _hint;

        [SerializeField] private TMP_Text[] _pointRows;
        [SerializeField] private TMP_Text[] _defeatCountRows;
        [SerializeField] private TMP_Text[] _accuracyRows;
        [SerializeField] private TMP_Text[] _maxComboRows;

        [SerializeField] private Color _normalColor;
        [SerializeField] private Color _highlightColor;

        // 今回の記録は、過去の記録とは別のインスタンスとして渡して見分ける
        public void Show(IReadOnlyList<ScoreEntry> entries, ScoreEntry current, bool hasWordsToReview)
        {
            SetColumn(_pointRows, entries, current, entry => entry.Point, entry => entry.Point.ToString());
            SetColumn(_defeatCountRows, entries, current, entry => entry.DefeatCount, entry => entry.DefeatCount.ToString());
            SetColumn(_accuracyRows, entries, current, entry => entry.Accuracy, entry => $"{entry.Accuracy * 100f:0}%");
            SetColumn(_maxComboRows, entries, current, entry => entry.MaxCombo, entry => entry.MaxCombo.ToString());
            _hint.text = hasWordsToReview ? REVIEW_HINT : RETRY_HINT;
            _fader.Show();
        }

        private void SetColumn<TKey>(
            TMP_Text[] rows,
            IReadOnlyList<ScoreEntry> entries,
            ScoreEntry current,
            Func<ScoreEntry, TKey> keySelector,
            Func<ScoreEntry, string> formatter)
        {
            var top = ScoreRanking.SelectTop(entries, keySelector, rows.Length);
            for (var i = 0; i < rows.Length; i++)
            {
                var hasEntry = i < top.Count;
                rows[i].text = hasEntry ? formatter(top[i]) : EMPTY_VALUE;
                rows[i].color = hasEntry && top[i] == current ? _highlightColor : _normalColor;
            }
        }

        // 復習のあいだは引っ込め、終わったら同じ内容で出し直す
        public void Hide() => _fader.Hide();
    }
}
