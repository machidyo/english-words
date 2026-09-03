using UnityEngine;
using UnityEngine.InputSystem;
using EnglishWords.Runtime.Effect;
using EnglishWords.Runtime.Magic;
using EnglishWords.Runtime.Monster;
using EnglishWords.Runtime.Score;
using EnglishWords.Runtime.Typing;
using EnglishWords.Runtime.UI;
using EnglishWords.Runtime.Wizard;
using EnglishWords.Runtime.Word;

namespace EnglishWords.Runtime.GameFlow
{
    public sealed class GameDirector : MonoBehaviour
    {
        private const int INITIAL_LIFE = 3;
        private const float SPAWN_DURATION_SECONDS = 60f;
        private const string WORD_RESOURCE_PATH = "Words";

        [SerializeField] private GameFlowAsset _gameFlow;
        [SerializeField] private MonsterSpawner _spawner;
        [SerializeField] private TypingInput _typingInput;
        [SerializeField] private MagicLauncher _magicLauncher;
        [SerializeField] private DefenseLine _defenseLine;
        [SerializeField] private CameraShake _cameraShake;
        [SerializeField] private HudView _hud;
        [SerializeField] private ResultView _resultView;

        private readonly ScoreRecord _score = new();
        private GameSession _session;
        private int _finishedFrame = -1;

        private bool IsFinished => _finishedFrame >= 0;

        private void Awake()
        {
            _session = new GameSession(INITIAL_LIFE, SPAWN_DURATION_SECONDS);

            var csv = Resources.Load<TextAsset>(WORD_RESOURCE_PATH);
            _spawner.Initialize(new WordProvider(WordCsvParser.Parse(csv.text)));

            _hud.SetPoint(_score.Point);
            _hud.SetLife(_session.RemainingLife);
            _hud.SetElapsedSeconds(0f);
            _hud.SetInputText(string.Empty);
        }

        // 入力の解釈から出来事を受け取り、演出・得点・進行の各担当へ配る
        private void OnEnable()
        {
            _typingInput.OnDefeated += _magicLauncher.Launch;
            _typingInput.OnDefeated += AddDefeat;
            _typingInput.OnMissed += AddMiss;
            _typingInput.OnTextChanged += _hud.SetInputText;
            _defenseLine.OnMonsterReached += LoseLife;
        }

        private void OnDisable()
        {
            _typingInput.OnDefeated -= _magicLauncher.Launch;
            _typingInput.OnDefeated -= AddDefeat;
            _typingInput.OnMissed -= AddMiss;
            _typingInput.OnTextChanged -= _hud.SetInputText;
            _defenseLine.OnMonsterReached -= LoseLife;
        }

        private void AddDefeat(MonsterUnit monster)
        {
            _score.AddDefeat();
            _hud.SetPoint(_score.Point);
        }

        private void AddMiss(string input)
        {
            _score.AddMiss(input);
            _hud.ShowMiss();
        }

        private void LoseLife(MonsterUnit monster)
        {
            monster.Vanish();
            _session.LoseLife();
            _hud.SetLife(_session.RemainingLife);
            _cameraShake.Play();

            if (_session.IsDefeated)
                Finish(GameResult.Defeated);
        }

        private void Update()
        {
            if (IsFinished)
            {
                UpdateResultInput();
                return;
            }

            _session.Advance(Time.deltaTime);
            _hud.SetElapsedSeconds(_session.ElapsedSeconds);

            if (_session.IsSpawning)
                _spawner.Tick(Time.deltaTime);
            else if (_session.IsCleared(_spawner.ActiveMonsters.Count))
                Finish(GameResult.Cleared);
        }

        private void UpdateResultInput()
        {
            // 撃破のEnterやゲームオーバーと同じフレームの入力を拾わない
            if (Time.frameCount == _finishedFrame)
                return;

            var keyboard = Keyboard.current;
            if (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame)
                _gameFlow.RequestRetry();
            else if (keyboard.escapeKey.wasPressedThisFrame)
                _gameFlow.RequestReturnToTitle();
        }

        private void Finish(GameResult result)
        {
            _finishedFrame = Time.frameCount;
            _typingInput.enabled = false;
            _resultView.Show(result, _score);
        }
    }
}
