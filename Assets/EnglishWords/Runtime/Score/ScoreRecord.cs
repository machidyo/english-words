using System.Collections.Generic;

namespace EnglishWords.Runtime.Score
{
    public sealed class ScoreRecord
    {
        private const int POINT_PER_DEFEAT = 100;

        public int Point { get; private set; }
        public int DefeatCount { get; private set; }
        public int MaxCombo { get; private set; }
        public IReadOnlyList<string> MissedInputs => _missedInputs;
        public float Accuracy => _submitCount == 0 ? 1f : (float)DefeatCount / _submitCount;

        private readonly List<string> _missedInputs = new();
        private int _submitCount;
        private int _combo;

        public void AddDefeat()
        {
            _submitCount++;
            DefeatCount++;
            Point += POINT_PER_DEFEAT;
            _combo++;
            if (_combo > MaxCombo)
                MaxCombo = _combo;
        }

        public void AddMiss(string input)
        {
            _submitCount++;
            _combo = 0;
            _missedInputs.Add(input);
        }
    }
}
