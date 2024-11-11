public interface IHealth
{
    int CurrentHealth { get; }
    void TakeDamage(int damage);
}