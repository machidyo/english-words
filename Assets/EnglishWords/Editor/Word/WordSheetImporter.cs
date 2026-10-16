using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using EnglishWords.Runtime.Word;

namespace EnglishWords.Editor.Word
{
    // 全タブを取得して検証し終えてから書き出す。1つでも失敗したら何も書き換えない。
    // 途中まで書き換わると、どのタブが新しいのか分からなくなるため
    public static class WordSheetImporter
    {
        private const string OUTPUT_FOLDER = "Assets/EnglishWords/Resources/Words";
        private const string PROGRESS_TITLE = "単語の取り込み";

        [MenuItem("EnglishWords/Import Words From Spreadsheet")]
        private static async void Import()
        {
            var settings = FindSettings();
            if (settings == null)
                return;

            var errors = ValidateSettings(settings);
            var sheets = new Dictionary<string, IReadOnlyList<WordEntry>>(StringComparer.OrdinalIgnoreCase);
            try
            {
                for (var i = 0; errors.Count == 0 && i < settings.Tabs.Count; i++)
                {
                    var tab = settings.Tabs[i];
                    EditorUtility.DisplayProgressBar(PROGRESS_TITLE, tab.FileName, (float)i / settings.Tabs.Count);
                    var entries = await ReadTab(settings.SpreadsheetId, tab, errors);
                    if (entries != null)
                        sheets.Add(tab.FileName, entries);
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            if (errors.Count > 0)
            {
                Debug.LogError($"単語の取り込みを中止しました。ファイルは変更していません\n{string.Join("\n", errors)}");
                return;
            }

            WarnDuplicatesAcrossTabs(sheets);
            Write(sheets);
        }

        private static WordSheetSettings FindSettings()
        {
            var guids = AssetDatabase.FindAssets($"t:{nameof(WordSheetSettings)}");
            if (guids.Length != 1)
            {
                Debug.LogError($"{nameof(WordSheetSettings)}のアセットがちょうど1つ必要です（{guids.Length}個あります）");
                return null;
            }
            return AssetDatabase.LoadAssetAtPath<WordSheetSettings>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }

        private static List<string> ValidateSettings(WordSheetSettings settings)
        {
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(settings.SpreadsheetId))
                errors.Add("スプレッドシートのIDが空です");
            if (settings.Tabs.Count == 0)
                errors.Add("タブが1つも登録されていません");

            var fileNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var tab in settings.Tabs)
            {
                if (string.IsNullOrWhiteSpace(tab.FileName) || tab.FileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                    errors.Add($"ファイル名「{tab.FileName}」は使えません");
                else if (!fileNames.Add(tab.FileName))
                    errors.Add($"ファイル名「{tab.FileName}」が重複しています");
                if (string.IsNullOrWhiteSpace(tab.Gid))
                    errors.Add($"「{tab.FileName}」のgidが空です");
            }
            return errors;
        }

        // シートを「リンクを知っている全員が閲覧可」にしておけば、認証なしでCSVとして書き出せる
        private static async Awaitable<IReadOnlyList<WordEntry>> ReadTab(
            string spreadsheetId,
            WordSheetTab tab,
            List<string> errors)
        {
            var url = $"https://docs.google.com/spreadsheets/d/{spreadsheetId}/export?format=csv&gid={tab.Gid}";
            using var request = UnityWebRequest.Get(url);
            await request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                errors.Add($"[{tab.FileName}] 取得できませんでした: {request.error}");
                return null;
            }

            var result = WordSheetReader.Read(request.downloadHandler.text);
            errors.AddRange(result.Errors.Select(error => $"[{tab.FileName}] {error}"));
            return result.Errors.Count == 0 ? result.Entries : null;
        }

        // 複数のタブに同じ単語があると、そのぶん出題されやすくなる。止めるほどではないので知らせるだけ
        private static void WarnDuplicatesAcrossTabs(Dictionary<string, IReadOnlyList<WordEntry>> sheets)
        {
            var duplicates = sheets
                .SelectMany(sheet => sheet.Value.Select(entry => (FileName: sheet.Key, entry.English)))
                .GroupBy(word => word.English, StringComparer.OrdinalIgnoreCase)
                .Where(sameWords => sameWords.Count() > 1);
            foreach (var sameWords in duplicates)
                Debug.LogWarning($"「{sameWords.Key}」が複数のタブにあります（{string.Join("、", sameWords.Select(word => word.FileName))}）");
        }

        // 出力先はシートの写しなので、設定から外したタブのCSVは消す
        private static void Write(Dictionary<string, IReadOnlyList<WordEntry>> sheets)
        {
            Directory.CreateDirectory(OUTPUT_FOLDER);
            foreach (var sheet in sheets)
                File.WriteAllText($"{OUTPUT_FOLDER}/{sheet.Key}.csv", WordCsvWriter.Write(sheet.Value));

            var stalePaths = Directory.GetFiles(OUTPUT_FOLDER, "*.csv")
                .Select(path => path.Replace('\\', '/'))
                .Where(path => !sheets.ContainsKey(Path.GetFileNameWithoutExtension(path)))
                .ToList();
            foreach (var path in stalePaths)
                AssetDatabase.DeleteAsset(path);

            AssetDatabase.Refresh();

            var summary = string.Join("、", sheets.Select(sheet => $"{sheet.Key} {sheet.Value.Count}語"));
            var deleted = stalePaths.Count > 0 ? $"\n削除: {string.Join("、", stalePaths.Select(Path.GetFileName))}" : "";
            Debug.Log($"単語を取り込みました: {summary}{deleted}");
        }
    }
}
