using System;
using UnityEngine;
using EnglishWords.Runtime.Monster;

namespace EnglishWords.Runtime.Magic
{
    public sealed class MagicLauncher : MonoBehaviour
    {
        [SerializeField] private MagicBolt _magicBoltPrefab;
        [SerializeField] private Transform _staffTip;

        // 魔法は撃つたびに作られて消えるので、着弾の知らせは発射役が中継して外へ渡す
        public event Action OnImpact;

        public void Launch(MonsterUnit target)
        {
            var bolt = Instantiate(_magicBoltPrefab, _staffTip.position, _staffTip.rotation);
            bolt.OnHit += RaiseImpact;
            bolt.Launch(target);
        }

        private void RaiseImpact() => OnImpact?.Invoke();
    }
}
