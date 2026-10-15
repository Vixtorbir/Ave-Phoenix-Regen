using PhoenixReborn.Gameplay;
using UnityEngine;
using UnityEngine.EventSystems;

namespace PhoenixReborn.UI
{
    /// <summary>
    /// Attached to the HUD Shield Orb so tapping it directly on a touch screen
    /// immediately activates the player's shield.
    /// </summary>
    public class ShieldTouchButton : MonoBehaviour, IPointerDownHandler
    {
        private PlayerInputReader _playerInput;

        private void Start()
        {
            FindPlayerInput();
        }

        private void FindPlayerInput()
        {
            if (_playerInput == null)
            {
                var player = GameObject.FindWithTag("Player");
                if (player != null)
                {
                    _playerInput = player.GetComponent<PlayerInputReader>();
                }
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_playerInput == null) FindPlayerInput();
            _playerInput?.TriggerShieldFromUI();
        }
    }
}
