using System.Collections.Generic;
using System.Text;
using EnglishWords.Runtime.Word;

namespace EnglishWords.Editor.Word
{
    public static class WordCsvWriter
    {
        // 日本語は訳が1つでも""で囲み、手で作っていたときの書式に揃える。gitの差分を読みやすくするため
        public static string Write(IReadOnlyList<WordEntry> entries)
        {
            var csv = new StringBuilder();
            csv.Append(WordCsvParser.HEADER).Append('\n');
            foreach (var entry in entries)
            {
                var japanese = string.Join(",", entry.JapaneseWords);
                csv.Append($"{entry.English},\"{japanese}\",{entry.Difficulty},{entry.Category}\n");
            }
            return csv.ToString();
        }
    }
}
