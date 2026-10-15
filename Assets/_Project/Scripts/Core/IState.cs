namespace PhoenixReborn.Core
{
    /// <summary>
    /// Contract for any state managed by a StateMachine.
    /// Used by the game flow and by the player's shield.
    /// </summary>
    public interface IState
    {
        void Enter();
        void Tick(float deltaTime);
        void Exit();
    }
}
