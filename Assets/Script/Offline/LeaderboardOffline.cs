using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class LeaderboardOffline : MonoBehaviour
{
    [SerializeField] private GameObject leaderboardPanel;
    [SerializeField] private TextMeshProUGUI leaderboardText;
    private List<LeaderboardEntry> leaderboard = new List<LeaderboardEntry>();

    private void Start()
    {
        LoadLeaderboard();

    }



    public void ShowLeaderboard()
    {
        if (leaderboardPanel != null)
        {
            leaderboardPanel.SetActive(true);
            DisplayLeaderboard();
        }
        else
        {
            Debug.LogError("Leaderboard panel is missing!");
        }
    }

    public void HideLeaderboard()
    {
        leaderboardPanel.SetActive(false);
    }

    public void AddScore(string playerName, int score)
    {
        LeaderboardEntry existingEntry = leaderboard.Find(entry => entry.playerName == playerName);

        if (existingEntry != null)
        {
            if (score > existingEntry.score)
            {
                existingEntry.score = score;
            }
        }
        else
        {
            LeaderboardEntry newEntry = new LeaderboardEntry(playerName, score);
            leaderboard.Add(newEntry);
        }

        leaderboard.Sort((a, b) => b.score.CompareTo(a.score));

        if (leaderboard.Count > 10)
        {
            leaderboard.RemoveRange(10, leaderboard.Count - 10);
        }

        SaveLeaderboard();
        DisplayLeaderboard();
    }

    private void DisplayLeaderboard()
    {
        const int totalWidth = 50; // Общая ширина строки
        const int rankWidth = 5;   // Ширина для места
        const int scoreWidth = 10; // Ширина для счета

        StringBuilder leaderboardString = new StringBuilder();

        // Заголовок
        string header = "Место".PadRight(rankWidth) +
                        "Имя".PadLeft((totalWidth - rankWidth - scoreWidth) / 2 + "Имя".Length / 2).PadRight(totalWidth - rankWidth - scoreWidth) +
                        "Счет".PadLeft(scoreWidth);
        leaderboardString.AppendLine(header);

        for (int i = 0; i < leaderboard.Count; i++)
        {
            LeaderboardEntry entry = leaderboard[i];

            string rank = (i + 1).ToString().PadRight(rankWidth);
            string score = entry.score.ToString().PadLeft(scoreWidth);

            int nameSpace = totalWidth - rankWidth - scoreWidth;
            string name = entry.playerName.Length > nameSpace ? entry.playerName.Substring(0, nameSpace) : entry.playerName;
            name = name.PadLeft(nameSpace / 2 + name.Length / 2).PadRight(nameSpace);

            leaderboardString.AppendLine($"{rank}{name}{score}");
        }

        leaderboardText.text = leaderboardString.ToString();
    }
    public void CheckForNewScore()
    {
        if (PlayerPrefs.HasKey("LastPlayerName") && PlayerPrefs.HasKey("LastScore"))
        {
            string playerName = PlayerPrefs.GetString("LastPlayerName");
            int score = PlayerPrefs.GetInt("LastScore");
            AddScore(playerName, score);

            // Очистка сохраненного счета
            PlayerPrefs.DeleteKey("LastPlayerName");
            PlayerPrefs.DeleteKey("LastScore");
            PlayerPrefs.Save();
        }
    }
    private void SaveLeaderboard()
    {
        PlayerPrefs.SetInt("LeaderboardCount", leaderboard.Count);

        for (int i = 0; i < leaderboard.Count; i++)
        {
            LeaderboardEntry entry = leaderboard[i];
            PlayerPrefs.SetString($"LeaderboardName{i}", entry.playerName);
            PlayerPrefs.SetInt($"LeaderboardScore{i}", entry.score);
        }

        PlayerPrefs.Save();
    }

    private void LoadLeaderboard()
    {
        int count = PlayerPrefs.GetInt("LeaderboardCount", 0);

        for (int i = 0; i < count; i++)
        {
            string playerName = PlayerPrefs.GetString($"LeaderboardName{i}");
            int score = PlayerPrefs.GetInt($"LeaderboardScore{i}");

            LeaderboardEntry entry = new LeaderboardEntry(playerName, score);
            leaderboard.Add(entry);
        }

        if (leaderboard.Count == 0)
        {
            AddInitialEntries();
        }
    }

    private void AddInitialEntries()
    {
        List<LeaderboardEntry> initialEntries = new List<LeaderboardEntry>
        {
            new LeaderboardEntry("Goose", 46050),
            new LeaderboardEntry("EnDooM", 43850),
            new LeaderboardEntry("GorillaTropic", 41550),
            new LeaderboardEntry("Jarkin", 38400),
            new LeaderboardEntry("GrOlGe", 33550),
            new LeaderboardEntry("ArtEki", 27450),
            new LeaderboardEntry("Grace", 3200),
            new LeaderboardEntry("Glass", 2800),
            new LeaderboardEntry("Newbee", 2500),
            new LeaderboardEntry("Dambass", 190)
        };

        leaderboard.AddRange(initialEntries);
        leaderboard.Sort((a, b) => b.score.CompareTo(a.score));

        if (leaderboard.Count > 10)
        {
            leaderboard.RemoveRange(10, leaderboard.Count - 10);
        }
    }

    private class LeaderboardEntry
    {
        public string playerName;
        public int score;

        public LeaderboardEntry(string playerName, int score)
        {
            this.playerName = playerName;
            this.score = score;
        }
    }
}
