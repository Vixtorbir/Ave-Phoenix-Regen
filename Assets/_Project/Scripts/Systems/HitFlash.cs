using System.Collections;
using UnityEngine;

namespace PhoenixReborn.Systems
{
    /// <summary>
    /// Flash component for instant visual hit feedback on enemies, boss, or player.
    /// Temporarily replaces sprite color with a flash color upon damage.
    /// </summary>
    public class HitFlash : MonoBehaviour
    {
        [SerializeField] private Color flashColor = new Color(1f, 0.2f, 0.2f, 1f);
        [SerializeField] private float flashDuration = 0.08f;

        private SpriteRenderer _spriteRenderer;
        private Color _originalColor;
        private Coroutine _flashCoroutine;

        private void Awake()
        {
            CacheRenderer();
        }

        private void CacheRenderer()
        {
            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponent<SpriteRenderer>();
                if (_spriteRenderer == null) _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
                if (_spriteRenderer == null) _spriteRenderer = GetComponentInParent<SpriteRenderer>();
            }

            if (_spriteRenderer != null && _originalColor.a <= 0.01f)
            {
                _originalColor = _spriteRenderer.color;
                if (_originalColor.a <= 0.01f)
                {
                    _originalColor = Color.white;
                }
            }
        }

        private void OnEnable()
        {
            ResetColor();
        }

        private void OnDisable()
        {
            ResetColor();
        }

        public void SetFlashColor(Color color)
        {
            flashColor = color;
        }

        public void Flash()
        {
            CacheRenderer();
            if (_spriteRenderer == null) return;

            if (_flashCoroutine != null)
            {
                StopCoroutine(_flashCoroutine);
            }
            _flashCoroutine = StartCoroutine(FlashRoutine());
        }

        public void ResetColor()
        {
            if (_flashCoroutine != null)
            {
                StopCoroutine(_flashCoroutine);
                _flashCoroutine = null;
            }

            if (_spriteRenderer != null && _originalColor != default)
            {
                _spriteRenderer.color = _originalColor;
            }
        }

        private IEnumerator FlashRoutine()
        {
            Color activeFlash = flashColor;

            if (Mathf.Abs(activeFlash.r - _originalColor.r) < 0.1f &&
                Mathf.Abs(activeFlash.g - _originalColor.g) < 0.1f &&
                Mathf.Abs(activeFlash.b - _originalColor.b) < 0.1f)
            {
                activeFlash = new Color(1f, 0.2f, 0.2f, 1f);
            }

            _spriteRenderer.color = activeFlash;
            yield return new WaitForSeconds(flashDuration);

            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = _originalColor;
            }
            _flashCoroutine = null;
        }
    }
}
