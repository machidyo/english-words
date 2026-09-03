using System.Collections.Generic;
using UnityEngine;
using EnglishWords.Runtime.Word;

namespace EnglishWords.Runtime.Monster
{
    public sealed class MonsterSpawner : MonoBehaviour
    {
        private const float SPAWN_INTERVAL_SECONDS = 3f;
        private const int MAX_ALIVE_COUNT = 5;

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
            Spawn();
        }

        private void Spawn()
        {
            var spawnPoint = _spawnPoints[Random.Range(0, _spawnPoints.Length)];
            var monster = Instantiate(_monsterPrefab, spawnPoint.position, Quaternion.identity, transform);
            monster.Setup(_wordProvider.Next(), _destination.position);
            monster.OnVanished += Remove;
            _activeMonsters.Add(monster);
        }

        private void Remove(MonsterUnit monster)
        {
            monster.OnVanished -= Remove;
            _activeMonsters.Remove(monster);
        }
    }
}
