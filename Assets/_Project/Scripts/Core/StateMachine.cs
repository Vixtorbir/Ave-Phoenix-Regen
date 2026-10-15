namespace PhoenixReborn.Core
{
    /// <summary>
    /// Generic finite state machine. Owns the current state and
    /// handles Enter/Exit transitions. Used by the game flow and
    /// by the player's shield.
    /// </summary>
    public class StateMachine
    {
        public IState CurrentState { get; private set; }

        public void ChangeState(IState newState)
        {
            CurrentState?.Exit();
            CurrentState = newState;
            CurrentState?.Enter();
        }

        public void Tick(float deltaTime)
        {
            CurrentState?.Tick(deltaTime);
        }
    }
}
