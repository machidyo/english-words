using System.Collections.Generic;
using UnityEngine;
using EnglishWords.Runtime.Word;

namespace EnglishWords.Runtime.Monster
{
    public sealed class MonsterSpawner : MonoBehaviour
    {
        private const float SPAWN_INTERVAL_SECONDS = 2f;
        private const int MAX_ALIVE_COUNT = 10;
        private const float REVIEW_SPEED_SCALE = 0.5f;

        [SerializeField] private MonsterUnit _monsterPrefab;
        [SerializeField] private Transform[] _spawnPoints;
        [SerializeField] private Transform _destination;

        public IReadOnlyList<MonsterUnit> ActiveMonsters => _activeMonsters;

        private readonly List<MonsterUnit> _activeMonsters = new();
        private WordProvider _wordProvider;
        private float _secondsSinceSpawn;

        public void Initialize(WordProvider wordProvider)
        {
            _wordProvider = wordProvider;

            // 開始直後に1体目を出す
            _secondsSinceSpawn = SPAWN_INTERVAL_SECONDS;
        }

        // 出現を止めるのは進行の判断なので、GameDirectorが呼ぶ間だけ動かす
        public void Tick(float deltaSeconds)
        {
            _secondsSinceSpawn += deltaSeconds;
            if (_secondsSinceSpawn < SPAWN_INTERVAL_SECONDS)
                return;
            if (_activeMonsters.Count >= MAX_ALIVE_COUNT)
                return;

            _secondsSinceSpawn = 0f;
            Spawn(_wordProvider.Next());
        }

        // 復習は歩き出しを打鍵に合わせるので、止めた状態でゆっくり歩く1体を出す
        public MonsterUnit SpawnForReview(WordEntry word)
        {
            var monster = Spawn(word);
            monster.SetSpeedScale(REVIEW_SPEED_SCALE);
            monster.Halt();
            return monster;
        }

        private MonsterUnit Spawn(WordEntry word)
        {
            var spawnPoint = _spawnPoints[Random.Range(0, _spawnPoints.Length)];
            var monster = Instantiate(_monsterPrefab, spawnPoint.position, Quaternion.identity, transform);
            monster.Setup(word, _destination.position);
            monster.OnVanished += Remove;
            _activeMonsters.Add(monster);
            return monster;
        }

        // リザルトの背後でモンスターを歩き続けさせない
        public void HaltMonsters()
        {
            foreach (var monster in _activeMonsters)
                monster.Halt();
        }

        // 復習は1体ずつ向き合わせたいので、残っているモンスターを消してから始める
        public void ClearMonsters()
        {
            for (var i = _activeMonsters.Count - 1; i >= 0; i--)
                _activeMonsters[i].Vanish();
        }

        private void Remove(MonsterUnit monster)
        {
            monster.OnVanished -= Remove;
            _activeMonsters.Remove(monster);
        }
    }
}
