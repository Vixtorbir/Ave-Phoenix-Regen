using UnityEngine;
using UnityEngine.InputSystem;

namespace PhoenixReborn.Gameplay
{
    /// <summary>
    /// Single point of contact with the Input System. Movement,
    /// shooting and shield all read from here instead of touching
    /// PlayerControls directly. Also handles touch drag and
    /// auto-fire on mobile.
    /// </summary>
    public class PlayerInputReader : MonoBehaviour
    {
        [Header("Gamepad / Keyboard Settings")]
        [SerializeField] private float deadzone = 0.15f;

        [Header("Mobile / Touch Settings")]
        [SerializeField] private bool enableTouchDrag = true;
        [SerializeField] private bool autoFireOnMobile = true;
        [SerializeField] private float touchSensitivity = 1.6f;

        private PlayerControls _controls;
        private Vector2 _lastTouchPosition;
        private bool _isDraggingTouch;
        private bool _shieldTriggeredFromUI;

        public Vector2 MoveInput { get; private set; }
        public bool FireHeld { get; private set; }
        public bool ShieldHeld { get; private set; }
        public bool ShieldPressed { get; private set; }

        private void Awake()
        {
            if (_controls == null) _controls = new PlayerControls();
        }

        private void OnEnable()
        {
            if (_controls == null) _controls = new PlayerControls();
            _controls.Enable();
        }

        private void OnDisable()
        {
            _controls?.Disable();
        }

        public void TriggerShieldFromUI()
        {
            _shieldTriggeredFromUI = true;
        }

        private void Update()
        {
            Vector2 rawInput = _controls != null ? _controls.Player.Move.ReadValue<Vector2>() : Vector2.zero;

            // Mobile Touch Drag Delta Calculation using New Input System
            Vector2 touchDelta = Vector2.zero;
            if (enableTouchDrag && Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
            {
                var touch = Touchscreen.current.primaryTouch;
                Vector2 currentPos = touch.position.ReadValue();

                if (touch.press.wasPressedThisFrame)
                {
                    _lastTouchPosition = currentPos;
                    _isDraggingTouch = true;
                }
                else if (_isDraggingTouch)
                {
                    touchDelta = (currentPos - _lastTouchPosition) / (Screen.dpi > 0 ? Screen.dpi : 160f) * touchSensitivity;
                    _lastTouchPosition = currentPos;
                }
            }
            else
            {
                _isDraggingTouch = false;
            }

            if (touchDelta.sqrMagnitude > 0.0001f)
            {
                MoveInput = Vector2.ClampMagnitude(touchDelta * 10f, 1f);
            }
            else if (rawInput.sqrMagnitude < deadzone * deadzone)
            {
                MoveInput = Vector2.zero;
            }
            else
            {
                MoveInput = rawInput;
            }

            bool isKeyboardOrGamepadFire = _controls != null && _controls.Player.Fire.IsPressed();
            FireHeld = isKeyboardOrGamepadFire || autoFireOnMobile || (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed);

            bool inputShield = _controls != null && (_controls.Player.Shield.WasPressedThisFrame() || _controls.Player.Shield.IsPressed());
            ShieldPressed = inputShield || _shieldTriggeredFromUI;
            ShieldHeld = (_controls != null && _controls.Player.Shield.IsPressed()) || _shieldTriggeredFromUI;

            // Reset UI trigger frame
            _shieldTriggeredFromUI = false;
        }
    }
}
