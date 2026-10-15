using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;

namespace PhoenixReborn.Systems
{
    /// <summary>
    /// Spawns world-space floating score popups (+100, +500) when enemies die.
    /// Uses spaced character typography, sharp outlines, and HDR Bloom luminescence.
    /// </summary>
    public class FloatingScoreManager : MonoBehaviour
    {
        public static FloatingScoreManager Instance { get; private set; }

        [Header("Score Popup Settings")]
        [SerializeField] private float floatSpeed = 1.4f;
        [SerializeField] private float fadeDuration = 0.60f;
        [SerializeField] private float fontSize = 3.8f;
        [SerializeField] private float hdrGlowMultiplier = 2.4f;

        private ObjectPool<TextMeshPro> _scorePool;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            InitPool();
        }

        private void InitPool()
        {
            if (_scorePool != null) return;

            _scorePool = new ObjectPool<TextMeshPro>(
                createFunc: CreateScoreText,
                actionOnGet: text => text.gameObject.SetActive(true),
                actionOnRelease: text => text.gameObject.SetActive(false),
                actionOnDestroy: text => Destroy(text.gameObject),
                defaultCapacity: 15,
                maxSize: 40
            );
        }

        private TextMeshPro CreateScoreText()
        {
            GameObject obj = new GameObject("FloatingScoreText");
            obj.transform.SetParent(transform);

            TextMeshPro tmp = obj.AddComponent<TextMeshPro>();
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = fontSize;
            tmp.fontStyle = FontStyles.Bold;
            tmp.characterSpacing = 10f;
            tmp.sortingOrder = 100;
            tmp.textWrappingMode = TextWrappingModes.NoWrap;

            if (tmp.fontMaterial != null)
            {
                tmp.fontMaterial.EnableKeyword("OUTLINE_ON");
                tmp.fontMaterial.SetColor(ShaderUtilities.ID_OutlineColor, new Color(0.02f, 0.02f, 0.02f, 0.95f));
                tmp.fontMaterial.SetFloat(ShaderUtilities.ID_OutlineWidth, 0.2f);
            }

            return tmp;
        }

        public void SpawnScore(Vector3 worldPosition, int points, Color color)
        {
            SpawnCustomText(worldPosition, $"+{points}", color, 1.18f, fadeDuration);
        }

        public void SpawnCustomText(Vector3 worldPosition, string message, Color color, float scaleMultiplier = 1.4f, float customDuration = 2.0f)
        {
            if (_scorePool == null) return;

            TextMeshPro text = _scorePool.Get();
            if (text == null) return;

            text.transform.position = worldPosition;
            text.transform.localScale = Vector3.one * scaleMultiplier;
            text.text = message;

            Color hdrColor = new Color(color.r * hdrGlowMultiplier, color.g * hdrGlowMultiplier, color.b * hdrGlowMultiplier, 1f);
            text.color = hdrColor;

            StartCoroutine(AnimateScoreRoutine(text, hdrColor, scaleMultiplier, customDuration));
        }

        public void SpawnScore(Vector3 worldPosition, int points)
        {
            SpawnScore(worldPosition, points, new Color(0.2f, 0.9f, 1f));
        }

        private IEnumerator AnimateScoreRoutine(TextMeshPro text, Color baseColor, float baseScaleMultiplier = 1.18f, float duration = 0.6f)
        {
            Vector3 startPos = text.transform.position;
            Vector3 endPos = startPos + new Vector3(0f, floatSpeed * (duration > 1f ? 1.6f : 1.2f), 0f);

            Color startColor = baseColor;
            Color endColor = new Color(baseColor.r, baseColor.g, baseColor.b, 0f);

            float timer = 0f;
            while (timer < duration)
            {
                timer += Time.deltaTime;
                float t = timer / duration;

                text.transform.position = Vector3.Lerp(startPos, endPos, t);
                text.transform.localScale = Vector3.Lerp(Vector3.one * baseScaleMultiplier, Vector3.one * (baseScaleMultiplier * 0.85f), t);
                text.color = Color.Lerp(startColor, endColor, t);

                yield return null;
            }

            _scorePool.Release(text);
        }
    }
}
