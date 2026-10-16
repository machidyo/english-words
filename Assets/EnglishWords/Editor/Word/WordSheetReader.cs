using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using EnglishWords.Runtime.Word;

namespace EnglishWords.Editor.Word
{
    // スプレッドシートから書き出したCSVを、ゲームが読める形かを確かめながら読む。
    // 実行時のパーサーは不正な行を黙って飛ばすので、取り込みの時点で止める
    public static class WordSheetReader
    {
        // 空白・ハイフンを含む語は初版のスコープ外
        private static readonly Regex ENGLISH_PATTERN = new("^[A-Za-z]+$");

        public static WordSheetReadResult Read(string csv)
        {
            var entries = new List<WordEntry>();
            var errors = new List<string>();
            var lines = csv.Replace("\r\n", "\n").Split('\n');

            // 共有されていないシートや誤ったgidでは、ログイン画面などのHTMLが返ってくる
            if (lines[0].Trim().TrimStart('﻿') != WordCsvParser.HEADER)
            {
                errors.Add($"1行目が見出し（{WordCsvParser.HEADER}）になっていません。シートの共有設定とgidを確認してください");
                return new WordSheetReadResult(entries, errors);
            }

            var spellings = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (var i = 1; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                // 書式だけ残った行は、セルが空のまま,,,として書き出される
                if (line.Replace(",", "").Trim().Length == 0)
                    continue;

                var row = i + 1;
                var entry = ReadRow(line, row, errors);
                if (entry == null)
                    continue;

                if (spellings.Add(entry.English))
                    entries.Add(entry);
                else
                    errors.Add($"{row}行目: 「{entry.English}」が重複しています");
            }

            if (entries.Count == 0 && errors.Count == 0)
                errors.Add("単語が1つもありません");
            return new WordSheetReadResult(entries, errors);
        }

        private static WordEntry ReadRow(string line, int row, List<string> errors)
        {
            // ""にエスケープされた"や、セル内の改行で割れた行は、実行時のパーサーが扱えない
            if (line.Contains("\"\"") || line.Count(character => character == '"') % 2 != 0)
            {
                errors.Add($"{row}行目: セルに\"や改行を入れないでください");
                return null;
            }

            var fields = WordCsvParser.SplitFields(line);
            if (fields.Count != WordCsvParser.FIELD_COUNT)
            {
                errors.Add($"{row}行目: 列が{WordCsvParser.FIELD_COUNT}つではありません（{fields.Count}つ）");
                return null;
            }

            var english = fields[0].Trim();
            if (!ENGLISH_PATTERN.IsMatch(english))
            {
                errors.Add($"{row}行目: 英単語「{english}」が空か、英字以外を含んでいます");
                return null;
            }

            var japaneseWords = fields[1].Split(',')
                .Select(word => word.Trim())
                .Where(word => word.Length > 0)
                .ToArray();
            if (japaneseWords.Length == 0)
            {
                errors.Add($"{row}行目: 「{english}」の日本語が空です");
                return null;
            }

            if (!int.TryParse(fields[2].Trim(), out var difficulty))
            {
                errors.Add($"{row}行目: 「{english}」の難易度「{fields[2]}」が整数ではありません");
                return null;
            }

            // 書き出すときにカテゴリーは""で囲まないので、,が入ると列がずれる
            var category = fields[3].Trim();
            if (category.Contains(','))
            {
                errors.Add($"{row}行目: 「{english}」のカテゴリーに,を入れないでください");
                return null;
            }

            return new WordEntry(english, japaneseWords, difficulty, category);
        }
    }
}
