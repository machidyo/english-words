using System;
using System.Collections.Generic;

namespace EnglishWords.Runtime.Word
{
    public sealed class WordProvider
    {
        private readonly IReadOnlyList<WordEntry> _entries;
        private readonly Random _random = new();

        public WordProvider(IReadOnlyList<WordEntry> entries)
        {
            _entries = entries;
        }

        public WordEntry Next() => _entries[_random.Next(_entries.Count)];
    }
}
