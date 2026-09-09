using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using EnglishWords.Runtime.Monster;

namespace EnglishWords.Runtime.Typing
{
    public sealed class TypingInput : MonoBehaviour
    {
        private static readonly IReadOnlyList<int> NO_INDICES = Array.Empty<int>();

        [SerializeField] private MonsterSpawner _spawner;

        public event Action OnTyped;
        public event Action<MonsterUnit> OnDefeated;
        public event Action OnMissed;
        public event Action<string, IReadOnlyList<int>> OnTextChanged;

        private readonly TypingBuffer _buffer = new();
        private TypingJudge _judge;

        public void Initialize(TypingJudge judge) => _judge = judge;

        private void OnEnable() => Keyboard.current.onTextInput += Append;

        private void OnDisable() => Keyboard.current.onTextInput -= Append;

        // 打つたびに突き合わせるので、確定のキーは持たない
        private void Append(char character)
        {
            if (!_buffer.TryAppend(character))
                return;

            OnTyped?.Invoke();

            var judgement = _judge.Judge(_buffer.Text, _spawner.ActiveMonsters);
            switch (judgement.Outcome)
            {
                case TypingOutcome.Defeated:
                    Defeat(judgement.Target);
                    break;
                case TypingOutcome.Missed:
                    Miss();
                    break;
                default:
                    OnTextChanged?.Invoke(_buffer.Text, judgement.MisspelledIndices);
                    break;
            }
        }

        private void Defeat(MonsterUnit target)
        {
            _buffer.Flush();
            OnTextChanged?.Invoke(_buffer.Text, NO_INDICES);

            // 入力完了で撃破を確定させる。魔法の飛翔は演出なので、ここで対象から外す
            target.MarkHit();
            OnDefeated?.Invoke(target);
        }

        private void Miss()
        {
            _buffer.Flush();
            OnTextChanged?.Invoke(_buffer.Text, NO_INDICES);
            OnMissed?.Invoke();
        }

        private void Update()
        {
            if (Keyboard.current.backspaceKey.wasPressedThisFrame && _buffer.TryRemoveLast())
                Erase();
        }

        // Backspaceは打ち間違いを直す操作なので、撃破もミスも起こさず綴り違いの表示だけ作り直す
        private void Erase()
        {
            var judgement = _judge.Judge(_buffer.Text, _spawner.ActiveMonsters);
            OnTextChanged?.Invoke(_buffer.Text, judgement.MisspelledIndices);
        }
    }
}
