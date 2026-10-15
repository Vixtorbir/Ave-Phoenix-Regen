using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace PhoenixReborn.Systems
{
    /// <summary>
    /// Sets up URP post-processing.
    /// Configures URP Bloom glow, Vignette focal depth, and Color Grading vibrancy.
    /// </summary>
    public class PostProcessingSetup : MonoBehaviour
    {
        [Header("Glow & Lighting Parameters")]
        [SerializeField] private float bloomIntensity = 1.6f;
        [SerializeField] private float bloomThreshold = 0.85f;
        [SerializeField] private float saturation = 28f;
        [SerializeField] private float contrast = 18f;
        [SerializeField] private float vignetteIntensity = 0.35f;

        [Header("Scene Volume Reference (Optional)")]
        [SerializeField] private Volume globalVolume;

        private VolumeProfile _profile;

        private void Start()
        {
            SetupGlobalVolume();
        }

        private void SetupGlobalVolume()
        {
            if (globalVolume == null)
            {
                globalVolume = FindFirstObjectByType<Volume>();
            }

            if (globalVolume != null)
            {
                _profile = globalVolume.profile;
            }

            if (_profile == null)
            {
                _profile = ScriptableObject.CreateInstance<VolumeProfile>();
                if (globalVolume != null) globalVolume.profile = _profile;
            }

            if (_profile != null)
            {
                if (!_profile.TryGet(out Bloom bloom))
                {
                    bloom = _profile.Add<Bloom>(true);
                }
                bloom.intensity.Override(bloomIntensity);
                bloom.threshold.Override(bloomThreshold);
                bloom.scatter.Override(0.68f);
                bloom.tint.Override(new Color(0.9f, 0.95f, 1f));

                if (!_profile.TryGet(out ColorAdjustments colorAdjustments))
                {
                    colorAdjustments = _profile.Add<ColorAdjustments>(true);
                }
                colorAdjustments.saturation.Override(saturation);
                colorAdjustments.contrast.Override(contrast);
                colorAdjustments.postExposure.Override(0.2f);

                if (!_profile.TryGet(out Vignette vignette))
                {
                    vignette.intensity.Override(vignetteIntensity);
                }
                vignette.intensity.Override(vignetteIntensity);
                vignette.smoothness.Override(0.45f);
                vignette.color.Override(new Color(0.02f, 0.01f, 0.05f));
            }

            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                UniversalAdditionalCameraData camData = mainCam.GetComponent<UniversalAdditionalCameraData>();
                if (camData != null)
                {
                    camData.renderPostProcessing = true;
                }
            }
        }
    }
}
