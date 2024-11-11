using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private Button showLeaderboardButton;
    [SerializeField] private LeaderboardManager leaderboardManager;

    private void Start()
    {
        showLeaderboardButton.onClick.AddListener(ShowLeaderboard);

        // ѕровер€ем наличие новых очков при запуске меню
        leaderboardManager.CheckForNewScore();
    }

    private void ShowLeaderboard()
    {
        leaderboardManager.ShowLeaderboard();
    }
}