using System.Collections.Generic;
using EnglishWords.Runtime.Word;

namespace EnglishWords.Editor.Word
{
    public sealed class WordSheetReadResult
    {
        public IReadOnlyList<WordEntry> Entries { get; }
        public IReadOnlyList<string> Errors { get; }

        public WordSheetReadResult(IReadOnlyList<WordEntry> entries, IReadOnlyList<string> errors)
        {
            Entries = entries;
            Errors = errors;
        }
    }
}
