using System.Collections;
using UnityEngine;

namespace PhoenixReborn.Systems
{
    /// <summary>
    /// Screen shake component for juice and impact feedback during explosions and hits.
    /// </summary>
    public class CameraShake : MonoBehaviour
    {
        public static CameraShake Instance { get; private set; }

        private Vector3 _originalPos;
        private Coroutine _shakeCoroutine;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            _originalPos = transform.localPosition;
        }

        public void Shake(float duration = 0.2f, float magnitude = 0.3f)
        {
            if (Time.timeScale <= 0f) return;
            if (_shakeCoroutine != null) StopCoroutine(_shakeCoroutine);
            _shakeCoroutine = StartCoroutine(ShakeRoutine(duration, magnitude));
        }

        public void StopShake()
        {
            if (_shakeCoroutine != null)
            {
                StopCoroutine(_shakeCoroutine);
                _shakeCoroutine = null;
            }
            transform.localPosition = _originalPos;
        }

        private IEnumerator ShakeRoutine(float duration, float magnitude)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                if (Time.timeScale <= 0f)
                {
                    break;
                }

                float x = Random.Range(-1f, 1f) * magnitude;
                float y = Random.Range(-1f, 1f) * magnitude;

                transform.localPosition = new Vector3(_originalPos.x + x, _originalPos.y + y, _originalPos.z);

                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.localPosition = _originalPos;
            _shakeCoroutine = null;
        }
    }
}
