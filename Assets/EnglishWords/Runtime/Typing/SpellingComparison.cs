using System.Collections.Generic;

namespace EnglishWords.Runtime.Typing
{
    // 打った文字列と正解を1文字ずつ突き合わせる。入力欄で直す位置を示すのに使う
    public static class SpellingComparison
    {
        // 入力が単語より長ければ、はみ出した分もすべて食い違いとして数える
        public static List<int> FindMisspelledIndices(string input, string word)
        {
            var misspelledIndices = new List<int>();

            for (var i = 0; i < input.Length; i++)
            {
                if (i >= word.Length || !IsSameLetter(input[i], word[i]))
                    misspelledIndices.Add(i);
            }
            return misspelledIndices;
        }

        public static bool IsSameLetter(char left, char right) => char.ToLowerInvariant(left) == char.ToLowerInvariant(right);
    }
}
