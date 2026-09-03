namespace EnglishWords.Runtime.GameFlow
{
    public sealed class GameSession
    {
        private readonly float _spawnDurationSeconds;

        public float ElapsedSeconds { get; private set; }
        public int RemainingLife { get; private set; }
        public bool IsSpawning => ElapsedSeconds < _spawnDurationSeconds;
        public bool IsDefeated => RemainingLife <= 0;

        public GameSession(int life, float spawnDurationSeconds)
        {
            RemainingLife = life;
            _spawnDurationSeconds = spawnDurationSeconds;
        }

        public void Advance(float deltaSeconds) => ElapsedSeconds += deltaSeconds;

        public void LoseLife() => RemainingLife--;

        // 出現が終わり、残りを倒し切ったらクリア
        public bool IsCleared(int aliveMonsterCount) => !IsSpawning && aliveMonsterCount == 0;
    }
}
