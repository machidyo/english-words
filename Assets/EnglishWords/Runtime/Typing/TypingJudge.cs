using System;
using System.Collections.Generic;
using EnglishWords.Runtime.Monster;
using EnglishWords.Runtime.Word;

namespace EnglishWords.Runtime.Typing
{
    // 打っている文字列を画面上のモンスターと突き合わせ、撃破・打鍵中・綴り違い・ミスに振り分ける
    public sealed class TypingJudge
    {
        // これだけ食い違うと、直せる打ち間違いではなく別の単語を打ったものとして扱う
        private const int MISS_DIFFERENCE_COUNT = 3;

        private readonly WordSpellings _spellings;

        public TypingJudge(WordSpellings spellings)
        {
            _spellings = spellings;
        }

        public TypingJudgement Judge(string input, IReadOnlyList<MonsterUnit> monsters)
        {
            // 同じ文字列で始まる単語が並んでいても短いほうを先に適用するため、打ち切りを前方一致より先に見る
            var target = FindNearestMatch(input, monsters);
            if (target != null)
                return TypingJudgement.Defeated(target);

            if (HasWordStartingWith(input, monsters))
                return TypingJudgement.Continuing();

            var misspelledIndices = FindClosestMisspelling(input, monsters, out var word);
            if (misspelledIndices == null)
                return TypingJudgement.Missed();

            return IsCorrectable(input, word, misspelledIndices)
                ? TypingJudgement.Misspelled(misspelledIndices)
                : TypingJudgement.Missed();
        }

        // 同じ単語のモンスターが複数出ていたら、魔法使いにもっとも近いものを倒す
        private static MonsterUnit FindNearestMatch(string input, IReadOnlyList<MonsterUnit> monsters)
        {
            MonsterUnit nearest = null;
            var nearestDistance = float.MaxValue;

            foreach (var monster in monsters)
            {
                if (!monster.IsTargetable || !string.Equals(monster.English, input, StringComparison.OrdinalIgnoreCase))
                    continue;
                if (monster.RemainingDistance >= nearestDistance)
                    continue;

                nearest = monster;
                nearestDistance = monster.RemainingDistance;
            }
            return nearest;
        }

        private static bool HasWordStartingWith(string input, IReadOnlyList<MonsterUnit> monsters)
        {
            foreach (var monster in monsters)
            {
                if (monster.IsTargetable && monster.English.StartsWith(input, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        // もっとも綴りの近いモンスターを選び、その単語と食い違っている位置を返す。狙えるモンスターがいなければnull
        private static List<int> FindClosestMisspelling(string input, IReadOnlyList<MonsterUnit> monsters, out string word)
        {
            List<int> closest = null;
            word = null;

            foreach (var monster in monsters)
            {
                if (!monster.IsTargetable)
                    continue;

                var misspelledIndices = SpellingComparison.FindMisspelledIndices(input, monster.English);
                if (closest != null && !IsCloser(misspelledIndices, closest))
                    continue;

                closest = misspelledIndices;
                word = monster.English;
            }
            return closest;
        }

        // 食い違いが少ないほうを近いとみなす。同数なら正しく打てている先頭が長いほうを採り、直す位置を打ち手の狙いに寄せる
        private static bool IsCloser(List<int> candidate, List<int> current)
        {
            if (candidate.Count != current.Count)
                return candidate.Count < current.Count;

            return candidate[0] > current[0];
        }

        private bool IsCorrectable(string input, string word, List<int> misspelledIndices)
        {
            if (misspelledIndices.Count >= MISS_DIFFERENCE_COUNT)
                return false;
            if (misspelledIndices.Count == 1)
                return true;
            if (IsTransposed(input, word, misspelledIndices))
                return true;

            // 二文字違いでも、打ち切った文字列が別の登録単語ならミスとして数える
            return !_spellings.Contains(input);
        }

        // 隣り合う二文字が入れ違っているだけなら、修正を促せば直せる
        private static bool IsTransposed(string input, string word, List<int> misspelledIndices)
        {
            var first = misspelledIndices[0];
            var second = misspelledIndices[1];
            if (second != first + 1 || second >= word.Length)
                return false;

            return SpellingComparison.IsSameLetter(input[first], word[second]) && SpellingComparison.IsSameLetter(input[second], word[first]);
        }
    }
}
