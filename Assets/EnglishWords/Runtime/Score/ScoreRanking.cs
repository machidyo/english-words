using System;
using System.Collections.Generic;
using System.Linq;

namespace EnglishWords.Runtime.Score
{
    public static class ScoreRanking
    {
        // 高い順に選ぶ。同点なら古い記録を上にする
        public static IReadOnlyList<ScoreEntry> SelectTop<TKey>(
            IEnumerable<ScoreEntry> entries,
            Func<ScoreEntry, TKey> keySelector,
            int count)
        {
            return entries
                .OrderByDescending(keySelector)
                .ThenBy(entry => entry.PlayedAt)
                .Take(count)
                .ToList();
        }
    }
}
