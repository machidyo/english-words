using System.Collections.Generic;
using System.Linq;
using EnglishWords.Runtime.Word;

namespace EnglishWords.Runtime.Review
{
    // 取りこぼした単語から、復習で出し直すものを選ぶ
    public static class ReviewWordSelector
    {
        // 取りこぼした回数の多い順に選ぶ。同数なら先に取りこぼしたものから出す
        public static IReadOnlyList<WordEntry> SelectMostMissed(IReadOnlyList<WordEntry> missedWords, int count)
        {
            return missedWords
                .GroupBy(word => word.English)
                .OrderByDescending(sameWords => sameWords.Count())
                .Take(count)
                .Select(sameWords => sameWords.First())
                .ToList();
        }
    }
}
