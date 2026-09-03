using System.Collections.Generic;

namespace EnglishWords.Runtime.Word
{
    public sealed class WordEntry
    {
        public string English { get; }
        public IReadOnlyList<string> JapaneseWords { get; }
        public int Difficulty { get; }
        public string Category { get; }

        public WordEntry(string english, IReadOnlyList<string> japaneseWords, int difficulty, string category)
        {
            English = english;
            JapaneseWords = japaneseWords;
            Difficulty = difficulty;
            Category = category;
        }
    }
}
