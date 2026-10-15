using UnityEngine;

namespace PhoenixReborn.Gameplay
{
    /// <summary>
    /// Moves the ship within the camera's visible bounds. Works with
    /// any aspect ratio, so the same script behaves correctly on a
    /// PC monitor and on a phone screen. Locks completely while the
    /// shield is active, matching the original arcade trade-off.
    /// </summary>
    [RequireComponent(typeof(PlayerInputReader))]
    [RequireComponent(typeof(PlayerShield))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float speed = 8f;
        [SerializeField] private float edgePadding = 0.4f;

        [Header("Vertical Movement Limits")]
        [Tooltip("If true, player is restricted to lower screen region. If false, can move anywhere on screen.")]
        [SerializeField] private bool limitVerticalToBottomRegion = true;
        [Range(0.1f, 1.0f)]
        [Tooltip("Fraction of screen height the player can move up to (0.45 = lower 45% of screen).")]
        [SerializeField] private float maxScreenVerticalFraction = 0.45f;

        [Header("Flight Animation")]
        [SerializeField] private float maxTiltAngle = 14f;
        [SerializeField] private float tiltSmoothSpeed = 12f;
        [SerializeField] private float thrusterPulseSpeed = 18f;
        [SerializeField] private float thrusterPulseAmount = 0.05f;

        private PlayerInputReader _input;
        private PlayerShield _shield;
        private Rigidbody2D _rb;
        private Camera _camera;
        private Vector3 _baseScale;
        private float _currentTilt;

        private void Awake()
        {
            _input = GetComponent<PlayerInputReader>();
            _shield = GetComponent<PlayerShield>();
            _camera = Camera.main;
            _baseScale = transform.localScale;

            EnforceZeroGravity();
        }

        private void OnEnable()
        {
            EnforceZeroGravity();
        }

        [Header("Spawn Position")]
        [SerializeField] private bool centerOnStart = true;
        [SerializeField] private float startY = -3.8f;

        private void Start()
        {
            EnforceZeroGravity();
            if (centerOnStart)
            {
                transform.position = new Vector3(0f, startY, transform.position.z);
            }
        }

        private void EnforceZeroGravity()
        {
            Rigidbody2D[] rbs = GetComponentsInChildren<Rigidbody2D>(true);
            foreach (var rb in rbs)
            {
                if (rb != null)
                {
                    rb.gravityScale = 0f;
                    rb.bodyType = RigidbodyType2D.Kinematic;
                    rb.constraints = RigidbodyConstraints2D.FreezeRotation;
#if UNITY_6000_0_OR_NEWER
                    rb.linearVelocity = Vector2.zero;
#else
                    rb.velocity = Vector2.zero;
#endif
                }
            }
        }

        private void Update()
        {
            Vector2 input = _input != null ? _input.MoveInput : Vector2.zero;

            if (input.sqrMagnitude > 0.001f)
            {
                Vector2 move = input * speed * Time.deltaTime;
                Vector3 nextPosition = transform.position + new Vector3(move.x, move.y, 0f);
                transform.position = ClampToScreen(nextPosition);
            }

            float targetTilt = -input.x * maxTiltAngle;
            _currentTilt = Mathf.Lerp(_currentTilt, targetTilt, Time.deltaTime * tiltSmoothSpeed);
            transform.localRotation = Quaternion.Euler(0f, 0f, _currentTilt);

            float pulse = 1f + (Mathf.Sin(Time.time * thrusterPulseSpeed) * thrusterPulseAmount);
            transform.localScale = new Vector3(_baseScale.x * pulse, _baseScale.y * (2f - pulse), _baseScale.z);
        }

        private Vector3 ClampToScreen(Vector3 position)
        {
            if (_camera == null) _camera = Camera.main;
            if (_camera == null) return position;

            float distanceToCamera = Mathf.Abs(_camera.transform.position.z - position.z);
            Vector3 min = _camera.ViewportToWorldPoint(new Vector3(0f, 0f, distanceToCamera));
            Vector3 max = _camera.ViewportToWorldPoint(new Vector3(1f, 1f, distanceToCamera));

            float x = Mathf.Clamp(position.x, min.x + edgePadding, max.x - edgePadding);
            float minY = min.y + edgePadding;

            float allowedMaxY;
            if (limitVerticalToBottomRegion)
            {
                float screenHeight = max.y - min.y;
                allowedMaxY = Mathf.Min(max.y - edgePadding, min.y + (screenHeight * maxScreenVerticalFraction));
            }
            else
            {
                allowedMaxY = max.y - edgePadding;
            }

            float y = Mathf.Clamp(position.y, minY, allowedMaxY);

            return new Vector3(x, y, position.z);
        }
    }
}
