using System.Collections.Generic;
using EnglishWords.Runtime.Word;

namespace EnglishWords.Runtime.Score
{
    public sealed class ScoreRecord
    {
        private const int POINT_PER_DEFEAT = 100;

        public int Point { get; private set; }
        public int DefeatCount { get; private set; }
        public int MaxCombo { get; private set; }
        public IReadOnlyList<WordEntry> MissedWords => _missedWords;
        public float Accuracy => _judgedCount == 0 ? 1f : (float)DefeatCount / _judgedCount;

        private readonly List<WordEntry> _missedWords = new();
        private int _judgedCount;
        private int _combo;

        public void AddDefeat()
        {
            _judgedCount++;
            DefeatCount++;
            Point += POINT_PER_DEFEAT;
            _combo++;
            if (_combo > MaxCombo)
                MaxCombo = _combo;
        }

        public void AddMiss()
        {
            _judgedCount++;
            _combo = 0;
        }

        // 復習に使うのは打ち間違いではなく、取りこぼしてライフを失った単語のほう
        public void AddMissedWord(WordEntry word) => _missedWords.Add(word);
    }
}
