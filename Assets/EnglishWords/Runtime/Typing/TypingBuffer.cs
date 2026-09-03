using System.Text;

namespace EnglishWords.Runtime.Typing
{
    public sealed class TypingBuffer
    {
        public string Text => _text.ToString();
        public bool IsEmpty => _text.Length == 0;

        private readonly StringBuilder _text = new();

        public bool TryAppend(char character)
        {
            if (character is not ((>= 'a' and <= 'z') or (>= 'A' and <= 'Z')))
                return false;

            _text.Append(char.ToLowerInvariant(character));
            return true;
        }

        public bool TryRemoveLast()
        {
            if (IsEmpty)
                return false;

            _text.Length--;
            return true;
        }

        public string Flush()
        {
            var text = _text.ToString();
            _text.Clear();
            return text;
        }
    }
}
