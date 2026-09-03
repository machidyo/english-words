using System;
using UnityEngine;
using EnglishWords.Runtime.Monster;

namespace EnglishWords.Runtime.Wizard
{
    public sealed class DefenseLine : MonoBehaviour
    {
        public event Action<MonsterUnit> OnMonsterReached;

        private void OnTriggerEnter(Collider other)
        {
            var monster = other.GetComponentInParent<MonsterUnit>();
            if (monster == null)
                return;

            OnMonsterReached?.Invoke(monster);
        }
    }
}
