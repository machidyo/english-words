using UnityEngine;
using EnglishWords.Runtime.Monster;

namespace EnglishWords.Runtime.Magic
{
    public sealed class MagicBolt : MonoBehaviour
    {
        private const float SECONDS_TO_REACH = 1f;
        private const float ARRIVAL_DISTANCE = 0.1f;
        private const float IMPACT_EFFECT_LIFETIME = 3f;

        [SerializeField] private GameObject _impactEffectPrefab;

        private MonsterUnit _target;
        private float _speed;

        public void Launch(MonsterUnit target)
        {
            _target = target;
            _speed = Vector3.Distance(transform.position, target.HitPoint) / SECONDS_TO_REACH;
        }

        // 対象は移動し続けるので毎フレーム狙い直す（誘導）
        private void Update()
        {
            if (_target == null)
            {
                Destroy(gameObject);
                return;
            }

            var destination = _target.HitPoint;
            var toDestination = destination - transform.position;
            if (toDestination.magnitude > ARRIVAL_DISTANCE)
                transform.forward = toDestination;

            // 追い越しを起こさないよう、移動しきってから着弾を判定する
            transform.position = Vector3.MoveTowards(transform.position, destination, _speed * Time.deltaTime);
            if (Vector3.Distance(transform.position, destination) > ARRIVAL_DISTANCE)
                return;

            Hit(destination);
        }

        private void Hit(Vector3 position)
        {
            Destroy(Instantiate(_impactEffectPrefab, position, Quaternion.identity), IMPACT_EFFECT_LIFETIME);
            _target.Vanish();
            Destroy(gameObject);
        }
    }
}
