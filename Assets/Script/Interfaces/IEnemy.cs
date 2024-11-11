public interface IEnemy
{
    void TakeDamage(int damage);
    void Die();
    int ScoreValue { get; }
}