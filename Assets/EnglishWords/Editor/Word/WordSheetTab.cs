using System;
using UnityEngine;

namespace EnglishWords.Editor.Word
{
    [Serializable]
    public sealed class WordSheetTab
    {
        [Tooltip("Resources/Words/に書き出すCSVの名前。拡張子は付けない")]
        [SerializeField] private string _fileName;
        [Tooltip("タブを開いたときのURLにある gid= の値")]
        [SerializeField] private string _gid;

        public string FileName => _fileName;
        public string Gid => _gid;
    }
}
