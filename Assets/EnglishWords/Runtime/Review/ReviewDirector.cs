using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using EnglishWords.Runtime.Monster;
using EnglishWords.Runtime.UI;
using EnglishWords.Runtime.Word;

namespace EnglishWords.Runtime.Review
{
    // リザルトのあと、取りこぼした単語を1体ずつ出し直して打ち直させる
    public sealed class ReviewDirector : MonoBehaviour
    {
        // ギブアップしたあと、答えを見せてから次の単語へ移るまで
        private const float ANSWER_REVEAL_SECONDS = 3f;

        private static readonly IReadOnlyList<int> NO_INDICES = Array.Empty<int>();

        [SerializeField] private MonsterSpawner _spawner;
        [SerializeField] private HudView _hud;
        [SerializeField] private ReviewView _view;

        public event Action OnTyped;
        public event Action<MonsterUnit> OnDefeated;
        public event Action OnFinished;

        private readonly Queue<WordEntry> _remainingWords = new();
        private ReviewSession _session;
        private MonsterUnit _monster;
        private int _wordCount;
        private float _revealRemainingSeconds;

        private bool IsWaitingForNextWord => _session == null;

        public void Begin(IReadOnlyList<WordEntry> words)
        {
            _remainingWords.Clear();
            foreach (var word in words)
                _remainingWords.Enqueue(word);
            _wordCount = _remainingWords.Count;

            // 打鍵を受け取り始めるのはここから。復習に入るまでUpdateもキー入力も動かさない
            enabled = true;
            _view.Show();
            StartNextWord();
        }

        private void Awake() => enabled = false;

        private void OnEnable()
        {
            Keyboard.current.onTextInput += Type;
            _view.OnGiveUpRequested += GiveUp;
        }

        private void OnDisable()
        {
            Keyboard.current.onTextInput -= Type;
            _view.OnGiveUpRequested -= GiveUp;
        }

        // 復習は覚え直す場なので、打ち間違いをミスとして数えない。赤く見せて直させるだけにする
        private void Type(char character)
        {
            if (IsWaitingForNextWord || !_session.TryType(character))
                return;

            OnTyped?.Invoke();

            // 答えを読むあいだは止めておき、打ち始めてから歩かせる
            _monster.Resume();
            ShowInput();

            if (_session.IsCompleted)
                CompleteStage();
        }

        private void CompleteStage()
        {
            // ヒントが出る前に思い出せたなら、仕上げの入力は省いてそのまま倒す
            var isRecalled = _session.Stage == ReviewStage.Recalling && !_session.HasShownHint;
            if (_session.Stage == ReviewStage.Finishing || isRecalled)
            {
                Defeat();
                return;
            }

            _monster.Knockback();
            _session.NextStage();
            ShowStage();
        }

        private void Update()
        {
            if (IsWaitingForNextWord)
            {
                WaitForNextWord();
                return;
            }

            var keyboard = Keyboard.current;
            if (keyboard.escapeKey.wasPressedThisFrame)
            {
                GiveUp();
                return;
            }
            if (keyboard.backspaceKey.wasPressedThisFrame && _session.TryErase())
                ShowInput();
            if (_session.Advance(Time.deltaTime))
                ShowInput();
        }

        // 倒したモンスターが消えて、答えを見せ終えてから次の単語を出す
        private void WaitForNextWord()
        {
            _revealRemainingSeconds -= Time.deltaTime;
            if (_revealRemainingSeconds > 0f || _monster != null)
                return;

            StartNextWord();
        }

        // 仕上げでどうしても出てこないときの逃げ道。答えを見せてから次の単語へ進む
        private void GiveUp()
        {
            if (IsWaitingForNextWord || _session.Stage != ReviewStage.Finishing)
                return;

            _hud.SetInputText(_session.Answer, NO_INDICES, string.Empty);
            Defeat();
            _revealRemainingSeconds = ANSWER_REVEAL_SECONDS;
        }

        // 撃破の確定だけを行う。魔法と演出は購読側が受け持つ
        private void Defeat()
        {
            _monster.MarkHit();
            OnDefeated?.Invoke(_monster);
            _view.SetGiveUpVisible(false);
            _session = null;
        }

        private void StartNextWord()
        {
            if (_remainingWords.Count == 0)
            {
                Finish();
                return;
            }

            var word = _remainingWords.Dequeue();
            _monster = _spawner.SpawnForReview(word);
            _session = new ReviewSession(word.English);
            _view.SetProgress(_wordCount - _remainingWords.Count, _wordCount);
            ShowStage();
        }

        private void ShowStage()
        {
            _view.SetGuide(_session.Stage);
            _view.SetGiveUpVisible(_session.Stage == ReviewStage.Finishing);
            ShowInput();
        }

        private void ShowInput() => _hud.SetInputText(_session.Typed, _session.MisspelledIndices, _session.Hint);

        private void Finish()
        {
            _hud.ClearInputText();
            _view.Hide();
            enabled = false;
            OnFinished?.Invoke();
        }
    }
}
