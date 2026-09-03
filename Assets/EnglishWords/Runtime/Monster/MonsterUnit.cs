using System;
using TMPro;
using UnityEngine;
using EnglishWords.Runtime.Word;

namespace EnglishWords.Runtime.Monster
{
    public sealed class MonsterUnit : MonoBehaviour
    {
        private const float SECONDS_TO_REACH = 5f;

        [SerializeField] private Transform _model;
        [SerializeField] private TMP_Text _wordLabel;
        [SerializeField] private Transform _hitPoint;
        [SerializeField] private Collider _bodyCollider;

        public event Action<MonsterUnit> OnVanished;

        public bool IsTargetable => _state == MonsterState.Approaching;
        public Vector3 HitPoint => _hitPoint.position;
        public float RemainingDistance => Vector3.Distance(transform.position, _destination);

        private MonsterState _state;
        private Vector3 _destination;
        private float _speed;
        private string _english;

        public void Setup(WordEntry word, Vector3 destination)
        {
            _english = word.English;

            // 訳が複数あっても頭上に出すのは先頭のひとつだけ
            _wordLabel.text = word.JapaneseWords[0];
            _destination = destination;
            _speed = Vector3.Distance(transform.position, destination) / SECONDS_TO_REACH;

            // 頭上の日本語はカメラに正対させたいので、向きを変えるのはモデルだけ
            _model.forward = destination - transform.position;
        }

        public bool Matches(string input) => string.Equals(_english, input, StringComparison.OrdinalIgnoreCase);

        // 被弾したものは当たり判定と入力対象から外れる。消滅は着弾時のVanishで行う
        public void MarkHit()
        {
            _state = MonsterState.Hit;
            _bodyCollider.enabled = false;
        }

        public void Vanish()
        {
            OnVanished?.Invoke(this);
            Destroy(gameObject);
        }

        private void Update()
        {
            transform.position = Vector3.MoveTowards(transform.position, _destination, _speed * Time.deltaTime);
        }
    }
}
