using System.Collections.Generic;
using UnityEngine;

namespace EnglishWords.Editor.Word
{
    [CreateAssetMenu(fileName = "WordSheetSettings", menuName = "EnglishWords/Word Sheet Settings")]
    public sealed class WordSheetSettings : ScriptableObject
    {
        [Tooltip("シートのURLの /d/ と /edit の間にある文字列")]
        [SerializeField] private string _spreadsheetId;
        [SerializeField] private List<WordSheetTab> _tabs;

        public string SpreadsheetId => _spreadsheetId;
        public IReadOnlyList<WordSheetTab> Tabs => _tabs;
    }
}
