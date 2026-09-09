using System;
using System.Collections.Generic;

namespace EnglishWords.Runtime.Word
{
    // 打った文字列が出題されうる単語かどうかを答える。画面に出ていない単語を打ち切ったときのミス判定に使う
    public sealed class WordSpellings
    {
        private readonly HashSet<string> _spellings = new(StringComparer.OrdinalIgnoreCase);

        public WordSpellings(IReadOnlyList<WordEntry> entries)
        {
            foreach (var entry in entries)
                _spellings.Add(entry.English);
        }

        public bool Contains(string spelling) => _spellings.Contains(spelling);
    }
}
