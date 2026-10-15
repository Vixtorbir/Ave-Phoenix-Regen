using PhoenixReborn.Core;

namespace PhoenixReborn.Gameplay
{

    public class ShieldReadyState : IState
    {
        private readonly PlayerShield _shield;

        public ShieldReadyState(PlayerShield shield) => _shield = shield;

        public void Enter() => _shield.SetVisual(false);

        public void Tick(float deltaTime)
        {
            if (_shield.Input != null && _shield.Input.ShieldPressed)
            {
                _shield.Machine.ChangeState(new ShieldActiveState(_shield));
            }
        }

        public void Exit() { }
    }

    public class ShieldActiveState : IState
    {
        private readonly PlayerShield _shield;
        private float _timer;

        public ShieldActiveState(PlayerShield shield) => _shield = shield;

        public void Enter()
        {
            _timer = _shield.ActiveDuration;
            _shield.SetVisual(true);
            Systems.AudioManager.Instance?.PlayShieldActivate();
        }

        public void Tick(float deltaTime)
        {
            _timer -= deltaTime;
            if (_timer <= 0f)
            {
                _shield.Machine.ChangeState(new ShieldCooldownState(_shield));
            }
        }

        public void Exit() { }
    }

    public class ShieldCooldownState : IState
    {
        private readonly PlayerShield _shield;
        private float _timer;

        public float Progress => _shield.CooldownDuration > 0f ? 1f - (_timer / _shield.CooldownDuration) : 1f;

        public ShieldCooldownState(PlayerShield shield) => _shield = shield;

        public void Enter()
        {
            _timer = _shield.CooldownDuration;
            _shield.SetVisual(false);
        }

        public void Tick(float deltaTime)
        {
            _timer -= deltaTime;
            if (_timer <= 0f)
            {
                _shield.Machine.ChangeState(new ShieldReadyState(_shield));
            }
        }

        public void Exit() { }
    }
}
