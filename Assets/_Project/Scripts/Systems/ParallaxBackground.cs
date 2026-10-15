using System.Collections.Generic;
using UnityEngine;

namespace PhoenixReborn.Systems
{
    /// <summary>
    /// Vertical parallax background.
    /// Supports smooth material UV scrolling or dual transform tiling.
    /// The starfield texture is generated procedurally (see StarfieldTexture).
    /// </summary>
    public class ParallaxBackground : MonoBehaviour
    {
        [Header("Scroll Settings")]
        [SerializeField] private float scrollSpeed = 0.12f;
        [SerializeField] private Vector2 textureTiling = new Vector2(3f, 4f);

        [Header("Dual Quad Tiling (Optional)")]
        [SerializeField] private Transform cloneTransform;
        [SerializeField] private float quadHeight = 36f;

        [Header("Procedural Starfield")]
        [SerializeField] private bool generateStarfield = true;
        [SerializeField] private StarfieldTexture.Settings starfield = StarfieldTexture.Settings.Default;

        // Both background quads ask for the same seed, so they share one texture.
        private static readonly Dictionary<int, Texture2D> GeneratedStarfields = new Dictionary<int, Texture2D>();

        private Renderer _renderer;
        private Material _material;
        private Vector2 _uvOffset;

        private void Awake()
        {
            _renderer = GetComponent<Renderer>();
            if (_renderer != null)
            {
                _material = _renderer.material;
                if (_material != null)
                {
                    if (generateStarfield) ApplyStarfield(_material);
                    _material.mainTextureScale = textureTiling;
                    if (_material.HasProperty("_BaseMap")) _material.SetTextureScale("_BaseMap", textureTiling);
                    if (_material.HasProperty("_MainTex")) _material.SetTextureScale("_MainTex", textureTiling);
                }
            }
        }

        private void Update()
        {
            float dtSpeed = scrollSpeed * Time.deltaTime;

            if (cloneTransform != null)
            {
                transform.position -= new Vector3(0f, dtSpeed * 10f, 0f);
                cloneTransform.position -= new Vector3(0f, dtSpeed * 10f, 0f);

                if (transform.position.y <= -quadHeight)
                {
                    transform.position = new Vector3(transform.position.x, cloneTransform.position.y + quadHeight, transform.position.z);
                }

                if (cloneTransform.position.y <= -quadHeight)
                {
                    cloneTransform.position = new Vector3(cloneTransform.position.x, transform.position.y + quadHeight, cloneTransform.position.z);
                }
            }

            if (_material != null)
            {
                _uvOffset.y += dtSpeed;
                _material.mainTextureOffset = _uvOffset;

                if (_material.HasProperty("_BaseMap"))
                {
                    _material.SetTextureOffset("_BaseMap", _uvOffset);
                }
                if (_material.HasProperty("_MainTex"))
                {
                    _material.SetTextureOffset("_MainTex", _uvOffset);
                }
            }
        }

        private void ApplyStarfield(Material material)
        {
            if (!GeneratedStarfields.TryGetValue(starfield.seed, out Texture2D texture) || texture == null)
            {
                texture = StarfieldTexture.Generate(starfield);
                GeneratedStarfields[starfield.seed] = texture;
            }

            material.mainTexture = texture;
            if (material.HasProperty("_BaseMap")) material.SetTexture("_BaseMap", texture);
            if (material.HasProperty("_MainTex")) material.SetTexture("_MainTex", texture);
        }
    }
}
