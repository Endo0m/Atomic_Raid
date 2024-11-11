public interface IScoreCounter
{
    int Score { get; }
    void AddScore(int points);
    void ResetScore();
}