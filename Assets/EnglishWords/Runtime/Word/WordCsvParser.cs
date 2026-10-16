using System.Collections.Generic;
using System.Text;

namespace EnglishWords.Runtime.Word
{
    public static class WordCsvParser
    {
        public const string HEADER = "english,japanese,difficulty,category";
        public const int FIELD_COUNT = 4;

        public static IReadOnlyList<WordEntry> Parse(string csv)
        {
            var entries = new List<WordEntry>();
            var lines = csv.Split('\n');

            // 1行目は見出し
            for (var i = 1; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                if (line.Length == 0)
                    continue;

                var fields = SplitFields(line);
                if (fields.Count < FIELD_COUNT)
                    continue;

                entries.Add(new WordEntry(fields[0], fields[1].Split(','), int.Parse(fields[2]), fields[3]));
            }
            return entries;
        }

        // 日本語は複数の訳を1フィールドに持つため、""で囲んだ中の,は区切りとして扱わない
        public static List<string> SplitFields(string line)
        {
            var fields = new List<string>();
            var field = new StringBuilder();
            var isQuoted = false;

            foreach (var character in line)
            {
                if (character == '"')
                    isQuoted = !isQuoted;
                else if (character == ',' && !isQuoted)
                {
                    fields.Add(field.ToString());
                    field.Clear();
                }
                else
                    field.Append(character);
            }
            fields.Add(field.ToString());
            return fields;
        }
    }
}
