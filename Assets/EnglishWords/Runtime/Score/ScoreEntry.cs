using System;

namespace EnglishWords.Runtime.Score
{
    // 1プレイぶんの記録。ランキングの1件になる
    public sealed class ScoreEntry
    {
        public DateTime PlayedAt { get; }
        public int Point { get; }
        public int DefeatCount { get; }
        public float Accuracy { get; }
        public int MaxCombo { get; }

        public ScoreEntry(DateTime playedAt, int point, int defeatCount, float accuracy, int maxCombo)
        {
            PlayedAt = playedAt;
            Point = point;
            DefeatCount = defeatCount;
            Accuracy = accuracy;
            MaxCombo = maxCombo;
        }

        public static ScoreEntry From(ScoreRecord record, DateTime playedAt)
        {
            return new ScoreEntry(playedAt, record.Point, record.DefeatCount, record.Accuracy, record.MaxCombo);
        }
    }
}
