using UnityEngine;

namespace PhoenixReborn.Systems
{
    [DefaultExecutionOrder(-1000)]
    [RequireComponent(typeof(Camera))]
    public class CameraWidthFitter : MonoBehaviour
    {
        private const float ReferenceOrthographicSize = 5f;
        private const float ReferenceAspect = 9f / 16f;

        public static float VerticalExtra { get; private set; }

        private Camera _camera;
        private float _fittedAspect;

        private void Awake()
        {
            Fit();
        }

        private void Update()
        {
            if (!Mathf.Approximately(_camera.aspect, _fittedAspect))
            {
                Fit();
            }
        }

        private void Fit()
        {
            if (_camera == null) _camera = GetComponent<Camera>();

            _fittedAspect = _camera.aspect;
            float size = ReferenceOrthographicSize * Mathf.Max(1f, ReferenceAspect / _fittedAspect);

            _camera.orthographicSize = size;
            VerticalExtra = size - ReferenceOrthographicSize;
        }
    }
}
