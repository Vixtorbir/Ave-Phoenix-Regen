namespace PhoenixReborn.Core
{
    /// <summary>
    /// Contract for any object that can receive damage (Enemies, Boss parts, Destructible barriers).
    /// </summary>
    public interface IDamageable
    {
        void TakeDamage(int damage);
    }
}
