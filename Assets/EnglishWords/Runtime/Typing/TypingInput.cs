using System;
using UnityEngine;
using UnityEngine.InputSystem;
using EnglishWords.Runtime.Monster;

namespace EnglishWords.Runtime.Typing
{
    public sealed class TypingInput : MonoBehaviour
    {
        [SerializeField] private MonsterSpawner _spawner;

        public event Action<MonsterUnit> OnDefeated;
        public event Action<string> OnMissed;
        public event Action<string> OnTextChanged;

        private readonly TypingBuffer _buffer = new();

        private void OnEnable() => Keyboard.current.onTextInput += Append;

        private void OnDisable() => Keyboard.current.onTextInput -= Append;

        private void Append(char character)
        {
            if (!_buffer.TryAppend(character))
                return;

            OnTextChanged?.Invoke(_buffer.Text);
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard.backspaceKey.wasPressedThisFrame && _buffer.TryRemoveLast())
                OnTextChanged?.Invoke(_buffer.Text);

            if (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame)
                Submit();
        }

        private void Submit()
        {
            if (_buffer.IsEmpty)
                return;

            var input = _buffer.Flush();
            OnTextChanged?.Invoke(_buffer.Text);

            var target = FindTarget(input);
            if (target == null)
            {
                OnMissed?.Invoke(input);
                return;
            }

            // 入力完了で撃破を確定させる。魔法の飛翔は演出なので、ここで対象から外す
            target.MarkHit();
            OnDefeated?.Invoke(target);
        }

        // 同じ単語が複数出ていたら、魔法使いにもっとも近いものを倒す
        private MonsterUnit FindTarget(string input)
        {
            MonsterUnit nearest = null;
            var nearestDistance = float.MaxValue;

            foreach (var monster in _spawner.ActiveMonsters)
            {
                if (!monster.IsTargetable || !monster.Matches(input))
                    continue;
                if (monster.RemainingDistance >= nearestDistance)
                    continue;

                nearest = monster;
                nearestDistance = monster.RemainingDistance;
            }
            return nearest;
        }
    }
}
