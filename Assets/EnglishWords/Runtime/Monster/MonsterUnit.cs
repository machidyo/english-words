using System;
using TMPro;
using UnityEngine;
using EnglishWords.Runtime.Word;

namespace EnglishWords.Runtime.Monster
{
    public sealed class MonsterUnit : MonoBehaviour
    {
        private const float SECONDS_TO_REACH = 20f;
        private const float KNOCKBACK_DISTANCE = 1.5f;
        private const float KNOCKBACK_SECONDS = 0.3f;
        private const string HIT_TRIGGER = "Hit";

        [SerializeField] private Transform _model;
        [SerializeField] private Animator _animator;
        [SerializeField] private TMP_Text _wordLabel;
        [SerializeField] private Transform _hitPoint;
        [SerializeField] private Collider _bodyCollider;

        public event Action<MonsterUnit> OnVanished;

        public WordEntry Word => _word;
        public string English => _word.English;
        public bool IsTargetable => _state == MonsterState.Approaching;
        public Vector3 HitPoint => _hitPoint.position;
        public float RemainingDistance => Vector3.Distance(transform.position, _destination);

        private MonsterState _state;
        private Vector3 _destination;
        private Vector3 _approachDirection;
        private float _speed;
        private float _speedScale = 1f;
        private float _knockbackRemainingDistance;
        private WordEntry _word;

        public void Setup(WordEntry word, Vector3 destination)
        {
            _word = word;

            // 訳が複数あっても頭上に出すのは先頭のひとつだけ
            _wordLabel.text = word.JapaneseWords[0];
            _destination = destination;
            _approachDirection = (destination - transform.position).normalized;
            _speed = Vector3.Distance(transform.position, destination) / SECONDS_TO_REACH;

            // 頭上の日本語はカメラに正対させたいので、向きを変えるのはモデルだけ
            _model.forward = _approachDirection;
        }

        // 出現位置によらず所要時間を揃えたうえで、復習だけゆっくり歩かせる
        public void SetSpeedScale(float scale) => _speedScale = scale;

        // 被弾したものは当たり判定と入力対象から外れる。倒れる動きを見せるので前進も止める。消滅は着弾時のVanishで行う
        public void MarkHit()
        {
            _state = MonsterState.Hit;
            _bodyCollider.enabled = false;
            _animator.SetTrigger(HIT_TRIGGER);
            enabled = false;
        }

        public void Vanish()
        {
            OnVanished?.Invoke(this);
            Destroy(gameObject);
        }

        // 前進だけを止める。当たり判定と頭上の日本語はそのまま残す
        public void Halt() => enabled = false;

        public void Resume() => enabled = true;

        // 打ち切った手応えとして少し押し戻す。復習では倒さずに打ち直させるので、状態は変えない
        public void Knockback()
        {
            _knockbackRemainingDistance = KNOCKBACK_DISTANCE;
            enabled = true;
        }

        private void Update()
        {
            if (_knockbackRemainingDistance > 0f)
            {
                PushBack();
                return;
            }

            transform.position = Vector3.MoveTowards(transform.position, _destination, _speed * _speedScale * Time.deltaTime);
        }

        private void PushBack()
        {
            var distance = Mathf.Min(KNOCKBACK_DISTANCE / KNOCKBACK_SECONDS * Time.deltaTime, _knockbackRemainingDistance);
            transform.position -= _approachDirection * distance;
            _knockbackRemainingDistance -= distance;
        }
    }
}
