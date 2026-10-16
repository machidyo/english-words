using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using EnglishWords.Runtime.Audio;
using EnglishWords.Runtime.Effect;
using EnglishWords.Runtime.Magic;
using EnglishWords.Runtime.Monster;
using EnglishWords.Runtime.Review;
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
        private const int REVIEW_WORD_COUNT = 3;
        private const string WORD_RESOURCE_FOLDER = "Words";

        [SerializeField] private GameFlowAsset _gameFlow;
        [SerializeField] private MonsterSpawner _spawner;
        [SerializeField] private TypingInput _typingInput;
        [SerializeField] private ReviewDirector _review;
        [SerializeField] private MagicLauncher _magicLauncher;
        [SerializeField] private WizardAnimator _wizardAnimator;
        [SerializeField] private DefenseLine _defenseLine;
        [SerializeField] private CameraShake _cameraShake;
        [SerializeField] private SePlayer _sePlayer;
        [SerializeField] private HudView _hud;
        [SerializeField] private ResultView _resultView;
        [SerializeField] private RankingView _rankingView;

        private readonly ScoreRecord _score = new();
        private GameSession _session;
        private GameResult _result;
        private IReadOnlyList<WordEntry> _reviewWords;
        private IReadOnlyList<ScoreEntry> _scoreEntries;
        private ScoreEntry _currentScore;
        private int _finishedFrame = -1;
        private bool _isRankingShown;
        private bool _isReviewing;
        private bool _isReviewed;

        private bool IsFinished => _finishedFrame >= 0;

        private void Awake()
        {
            _session = new GameSession(INITIAL_LIFE, SPAWN_DURATION_SECONDS);

            // スプレッドシートのタブごとにCSVが分かれているので、まとめて1つの出題範囲にする
            var words = Resources.LoadAll<TextAsset>(WORD_RESOURCE_FOLDER)
                .SelectMany(csv => WordCsvParser.Parse(csv.text))
                .ToList();
            _spawner.Initialize(new WordProvider(words));
            _typingInput.Initialize(new TypingJudge(new WordSpellings(words)));

            _hud.SetPoint(_score.Point);
            _hud.SetLife(_session.RemainingLife);
            _hud.SetElapsedSeconds(0f);
            _hud.ClearInputText();
        }

        // 入力の解釈から出来事を受け取り、演出・得点・進行の各担当へ配る
        private void OnEnable()
        {
            _typingInput.OnTyped += _sePlayer.PlayKeyStroke;
            _typingInput.OnDefeated += _magicLauncher.Launch;
            _typingInput.OnDefeated += CastMagic;
            _typingInput.OnDefeated += AddDefeat;
            _typingInput.OnMissed += AddMiss;
            _typingInput.OnTextChanged += _hud.SetInputText;
            _magicLauncher.OnImpact += _sePlayer.PlayMonsterHit;
            _defenseLine.OnMonsterReached += LoseLife;

            // 復習は覚え直すための時間なので、演出だけを配って得点には触れない
            _review.OnTyped += _sePlayer.PlayKeyStroke;
            _review.OnDefeated += _magicLauncher.Launch;
            _review.OnDefeated += CastMagic;
            _review.OnFinished += FinishReview;
        }

        private void OnDisable()
        {
            _typingInput.OnTyped -= _sePlayer.PlayKeyStroke;
            _typingInput.OnDefeated -= _magicLauncher.Launch;
            _typingInput.OnDefeated -= CastMagic;
            _typingInput.OnDefeated -= AddDefeat;
            _typingInput.OnMissed -= AddMiss;
            _typingInput.OnTextChanged -= _hud.SetInputText;
            _magicLauncher.OnImpact -= _sePlayer.PlayMonsterHit;
            _defenseLine.OnMonsterReached -= LoseLife;

            _review.OnTyped -= _sePlayer.PlayKeyStroke;
            _review.OnDefeated -= _magicLauncher.Launch;
            _review.OnDefeated -= CastMagic;
            _review.OnFinished -= FinishReview;
        }

        // 詠唱の見た目と音は対象を問わないので、撃破イベントの引数は渡さない
        private void CastMagic(MonsterUnit monster)
        {
            _wizardAnimator.PlayCast();
            _sePlayer.PlayMagicCast();
        }

        private void AddDefeat(MonsterUnit monster)
        {
            _score.AddDefeat();
            _hud.SetPoint(_score.Point);
        }

        private void AddMiss()
        {
            _score.AddMiss();
            _hud.ShowMiss();
            _sePlayer.PlayWordMissed();
        }

        private void LoseLife(MonsterUnit monster)
        {
            // 終了と同じフレームに到達したモンスターでライフを減らさない
            if (IsFinished)
                return;

            _score.AddMissedWord(monster.Word);
            monster.Vanish();
            _session.LoseLife();
            _hud.SetLife(_session.RemainingLife);
            _cameraShake.Play();
            _sePlayer.PlayWizardDamaged();

            if (_session.IsDefeated)
                Finish(GameResult.Defeated);
        }

        private void Update()
        {
            if (IsFinished)
            {
                // 復習の打鍵とEnter・Escがぶつからないよう、リザルトの操作は引っ込める
                if (!_isReviewing)
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
            // クリアやゲームオーバーと同じフレームの入力を拾わない
            if (Time.frameCount == _finishedFrame)
                return;

            var keyboard = Keyboard.current;
            var isEnterPressed = keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame;
            if (!_isRankingShown)
            {
                if (isEnterPressed)
                    ShowRanking();
                return;
            }

            if (isEnterPressed)
                AdvanceFromRanking();
            else if (keyboard.escapeKey.wasPressedThisFrame)
                _gameFlow.RequestReturnToTitle();
        }

        private void ShowRanking()
        {
            _isRankingShown = true;
            _resultView.Hide();
            _rankingView.Show(_scoreEntries, _currentScore, _reviewWords.Count > 0);
        }

        // ランキングのEnterは、取りこぼしが残っていれば復習へ、済んでいればもう一度へ進める
        private void AdvanceFromRanking()
        {
            if (_isReviewed || _reviewWords.Count == 0)
            {
                _gameFlow.RequestRetry();
                return;
            }

            _isReviewing = true;
            _rankingView.Hide();
            _spawner.ClearMonsters();
            _review.Begin(_reviewWords);
        }

        private void FinishReview()
        {
            _isReviewing = false;
            _isReviewed = true;
            _rankingView.Show(_scoreEntries, _currentScore, false);
        }

        // 勝敗が決まったら進行を止め、リザルトのEnter・Escの受付だけを残す
        private void Finish(GameResult result)
        {
            _result = result;
            _finishedFrame = Time.frameCount;
            _typingInput.enabled = false;
            _spawner.HaltMonsters();
            _reviewWords = ReviewWordSelector.SelectMostMissed(_score.MissedWords, REVIEW_WORD_COUNT);
            RecordScore();
            _resultView.Show(result, _score);
        }

        // 今回の記録を見分けて強調できるよう、保存する前の記録に今回のインスタンスを足しておく
        private void RecordScore()
        {
            _currentScore = ScoreEntry.From(_score, DateTime.Now);
            _scoreEntries = ScoreStorage.LoadAll().Append(_currentScore).ToList();
            ScoreStorage.Save(_currentScore);
        }
    }
}
