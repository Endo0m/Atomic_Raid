using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private Button showLeaderboardButton;
    [SerializeField] private LeaderboardManager leaderboardManager;

    private void Start()
    {
        showLeaderboardButton.onClick.AddListener(ShowLeaderboard);

        // ��������� ������� ����� ����� ��� ������� ����
        leaderboardManager.CheckForNewScore();
    }

    private void ShowLeaderboard()
    {
        leaderboardManager.ShowLeaderboard();
    }
}