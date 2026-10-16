using System;
using System.Collections.Generic;
using EnglishWords.Runtime.Typing;

namespace EnglishWords.Runtime.Review
{
    // 1単語ぶんの復習の進行。お手本→思い出し→仕上げの3段階を、打った文字と詰まった時間で進める
    public sealed class ReviewSession
    {
        // 思い出せないとみなすまでの時間と、そのあとヒントを1文字ずつ足す間隔
        private const float FIRST_HINT_SECONDS = 5f;
        private const float NEXT_HINT_SECONDS = 3f;

        public ReviewStage Stage { get; private set; }
        public string Answer => _answer;
        public string Typed => _buffer.Text;
        public IReadOnlyList<int> MisspelledIndices => SpellingComparison.FindMisspelledIndices(_buffer.Text, _answer);
        public bool IsCompleted => string.Equals(_buffer.Text, _answer, StringComparison.OrdinalIgnoreCase);
        public bool HasShownHint => _hintLetterCount > 0;

        // まだ打っていない残りのうち、薄く見せてよい分。お手本の段階は答えを丸ごと見せる
        public string Hint
        {
            get
            {
                var visibleLetterCount = Stage == ReviewStage.Tracing ? _answer.Length : _hintLetterCount;
                return visibleLetterCount <= _buffer.Text.Length
                    ? string.Empty
                    : _answer.Substring(_buffer.Text.Length, visibleLetterCount - _buffer.Text.Length);
            }
        }

        private readonly TypingBuffer _buffer = new();
        private readonly string _answer;
        private float _stuckSeconds;
        private int _hintLetterCount;

        public ReviewSession(string answer)
        {
            _answer = answer;
        }

        public bool TryType(char character)
        {
            if (!_buffer.TryAppend(character))
                return false;

            // 正しく打てているあいだは思い出せているので、詰まった時間を数え直す
            if (_answer.StartsWith(_buffer.Text, StringComparison.OrdinalIgnoreCase))
                _stuckSeconds = 0f;

            return true;
        }

        public bool TryErase() => _buffer.TryRemoveLast();

        // ヒントが増えたらtrueを返し、表示の作り直しを促す
        public bool Advance(float deltaSeconds)
        {
            if (Stage != ReviewStage.Recalling || _hintLetterCount >= _answer.Length)
                return false;

            _stuckSeconds += deltaSeconds;
            if (_stuckSeconds < (HasShownHint ? NEXT_HINT_SECONDS : FIRST_HINT_SECONDS))
                return false;

            // 思い出せなかったので、打ちかけを捨てて1文字目から見せ直す
            if (!HasShownHint)
                _buffer.Flush();

            _stuckSeconds = 0f;
            _hintLetterCount++;
            return true;
        }

        // 打ち切った段階を終える。入力とヒントは次の段階へ持ち越さない
        public void NextStage()
        {
            Stage = Stage == ReviewStage.Tracing ? ReviewStage.Recalling : ReviewStage.Finishing;
            _buffer.Flush();
            _stuckSeconds = 0f;
            _hintLetterCount = 0;
        }
    }
}
