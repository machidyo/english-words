using UnityEngine;

namespace EnglishWords.Runtime.Effect
{
    public sealed class CameraShake : MonoBehaviour
    {
        private const float DURATION_SECONDS = 0.25f;
        private const float STRENGTH = 0.25f;

        private Vector3 _basePosition;
        private float _remainingSeconds;

        public void Play()
        {
            _remainingSeconds = DURATION_SECONDS;
            enabled = true;
        }

        private void Awake()
        {
            _basePosition = transform.localPosition;
            enabled = false;
        }

        private void Update()
        {
            _remainingSeconds -= Time.deltaTime;
            if (_remainingSeconds <= 0f)
            {
                transform.localPosition = _basePosition;
                enabled = false;
                return;
            }

            var decay = _remainingSeconds / DURATION_SECONDS;
            transform.localPosition = _basePosition + Random.insideUnitSphere * (STRENGTH * decay);
        }
    }
}
