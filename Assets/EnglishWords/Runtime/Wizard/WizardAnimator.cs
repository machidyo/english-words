using UnityEngine;

namespace EnglishWords.Runtime.Wizard
{
    public sealed class WizardAnimator : MonoBehaviour
    {
        private const string CAST_TRIGGER = "Cast";

        [SerializeField] private Animator _animator;

        public void PlayCast() => _animator.SetTrigger(CAST_TRIGGER);
    }
}
