using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

namespace EnglishWords.Runtime.Score
{
    // 記録は日ごとのCSVに追記し、読むときは全ファイルをまとめて返す
    public static class ScoreStorage
    {
        private const string FOLDER_NAME = "scores";
        private const string FILE_PREFIX = "score_";
        private const string FILE_DATE_FORMAT = "yyyyMMdd";
        private const string PLAYED_AT_FORMAT = "yyyy-MM-dd HH:mm:ss";
        private const string HEADER = "played_at,point,defeat_count,accuracy,max_combo";
        private const int FIELD_COUNT = 5;

        private static string FolderPath => Path.Combine(Application.persistentDataPath, FOLDER_NAME);

        public static void Save(ScoreEntry entry)
        {
            Directory.CreateDirectory(FolderPath);
            var path = Path.Combine(FolderPath, $"{FILE_PREFIX}{entry.PlayedAt.ToString(FILE_DATE_FORMAT, CultureInfo.InvariantCulture)}.csv");
            if (!File.Exists(path))
                File.WriteAllText(path, HEADER + "\n");

            File.AppendAllText(path, ToLine(entry) + "\n");
        }

        private static string ToLine(ScoreEntry entry)
        {
            var invariant = CultureInfo.InvariantCulture;
            return string.Join(",",
                entry.PlayedAt.ToString(PLAYED_AT_FORMAT, invariant),
                entry.Point.ToString(invariant),
                entry.DefeatCount.ToString(invariant),
                entry.Accuracy.ToString("R", invariant),
                entry.MaxCombo.ToString(invariant));
        }

        public static IReadOnlyList<ScoreEntry> LoadAll()
        {
            var entries = new List<ScoreEntry>();
            if (!Directory.Exists(FolderPath))
                return entries;

            foreach (var path in Directory.GetFiles(FolderPath, $"{FILE_PREFIX}*.csv"))
                entries.AddRange(Parse(File.ReadAllText(path)));
            return entries;
        }

        private static IEnumerable<ScoreEntry> Parse(string csv)
        {
            var invariant = CultureInfo.InvariantCulture;
            var lines = csv.Split('\n');

            // 1行目は見出し
            for (var i = 1; i < lines.Length; i++)
            {
                var fields = lines[i].Trim().Split(',');
                if (fields.Length < FIELD_COUNT)
                    continue;

                yield return new ScoreEntry(
                    DateTime.ParseExact(fields[0], PLAYED_AT_FORMAT, invariant),
                    int.Parse(fields[1], invariant),
                    int.Parse(fields[2], invariant),
                    float.Parse(fields[3], invariant),
                    int.Parse(fields[4], invariant));
            }
        }
    }
}
