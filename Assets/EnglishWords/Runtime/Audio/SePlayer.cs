using UnityEngine;

namespace EnglishWords.Runtime.Audio
{
    // 効果音は鳴る場所ではなく出来事で決まるので、1つのAudioSourceにまとめて重ねて鳴らす
    public sealed class SePlayer : MonoBehaviour
    {
        [SerializeField] private AudioSource _audioSource;

        [SerializeField] private AudioClip _keyStroke;
        [SerializeField] private AudioClip _wordMissed;
        [SerializeField] private AudioClip _magicCast;
        [SerializeField] private AudioClip _monsterHit;
        [SerializeField] private AudioClip _wizardDamaged;

        public void PlayKeyStroke() => _audioSource.PlayOneShot(_keyStroke);

        public void PlayWordMissed() => _audioSource.PlayOneShot(_wordMissed);

        public void PlayMagicCast() => _audioSource.PlayOneShot(_magicCast);

        public void PlayMonsterHit() => _audioSource.PlayOneShot(_monsterHit);

        public void PlayWizardDamaged() => _audioSource.PlayOneShot(_wizardDamaged);
    }
}
