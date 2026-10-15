using UnityEngine;
using PhoenixReborn.Core;

namespace PhoenixReborn.Gameplay
{
    /// <summary>
    /// Owns a StateMachine driving the shield: Ready, Active (movement
    /// locked, immune to hits) and Cooldown. Hold to raise it, release
    /// to start the cooldown — faithful to the 1980 original.
    /// </summary>
    [RequireComponent(typeof(PlayerInputReader))]
    public class PlayerShield : MonoBehaviour
    {
        [SerializeField] private float activeDuration = 5f;
        [SerializeField] private float cooldownDuration = 3.5f;
        [SerializeField] private GameObject shieldVisual;

        public StateMachine Machine { get; private set; }
        public PlayerInputReader Input { get; private set; }
        public float ActiveDuration => activeDuration;
        public float CooldownDuration => cooldownDuration;
        public bool IsActive => Machine != null && Machine.CurrentState is ShieldActiveState;

        public float CooldownProgress
        {
            get
            {
                if (Machine != null && Machine.CurrentState is ShieldCooldownState cooldownState)
                {
                    return Mathf.Clamp01(cooldownState.Progress);
                }
                return IsActive ? 0f : 1f;
            }
        }

        private void Awake()
        {
            Input = GetComponent<PlayerInputReader>();
            Machine = new StateMachine();

            if (shieldVisual == null)
            {
                Transform childVisual = transform.Find("ShieldVisual");
                if (childVisual != null)
                {
                    shieldVisual = childVisual.gameObject;
                }
            }
        }

        private void Start()
        {
            Machine.ChangeState(new ShieldReadyState(this));
        }

        private void Update()
        {
            Machine.Tick(Time.deltaTime);

            if (IsActive && shieldVisual != null)
            {
                float pulse = 1f + (Mathf.Sin(Time.time * 12f) * 0.05f);
                shieldVisual.transform.localScale = new Vector3(pulse, pulse, 1f);
            }
        }

        public void SetVisual(bool active)
        {
            if (shieldVisual != null)
            {
                shieldVisual.SetActive(active);
                if (active)
                {
                    shieldVisual.transform.localScale = Vector3.one;
                }
            }
        }
    }
}
