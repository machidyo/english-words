using UnityEngine;
using EnglishWords.Runtime.Monster;

namespace EnglishWords.Runtime.Magic
{
    public sealed class MagicLauncher : MonoBehaviour
    {
        [SerializeField] private MagicBolt _magicBoltPrefab;
        [SerializeField] private Transform _staffTip;

        public void Launch(MonsterUnit target)
        {
            var bolt = Instantiate(_magicBoltPrefab, _staffTip.position, _staffTip.rotation);
            bolt.Launch(target);
        }
    }
}
