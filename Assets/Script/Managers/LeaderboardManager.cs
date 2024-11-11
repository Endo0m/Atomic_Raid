
using System.Collections;
using TMPro;
using UnityEngine.UI;
using Dan.Main;
using Dan.Models;
using System.Linq;
using UnityEngine; // Добавлено для использования LINQ

public class LeaderboardManager : MonoBehaviour
{
    [SerializeField] private GameObject leaderboardPanel;
    [SerializeField] private Transform leaderboardContent;
    [SerializeField] private GameObject entryDisplayPrefab;
    [SerializeField] private TextMeshProUGUI leaderboardText;
    [SerializeField] private Button previousPageButton;
    [SerializeField] private Button nextPageButton;
    [SerializeField] private TMP_InputField pageInputField;
    [SerializeField] private CanvasGroup loadingPanel;

    private const string LeaderboardPublicKey = "9346e84b22b1b196eaa7dba41106371ffeb7943073fb8859d8803f795235948d";
    private const int EntriesPerPage = 20;
    private int currentPage = 1;
    private Entry[] currentEntries; // Новое поле для хранения текущих записей таблицы лидеров

    private void Start()
    {
        InitializeComponents();
        LocalizationManager.Instance.OnLanguageChanged += OnLanguageChanged;
    }

    private void OnDestroy()
    {
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.OnLanguageChanged -= OnLanguageChanged;
        }
    }

    private void InitializeComponents()
    {
        previousPageButton.onClick.AddListener(() => ChangePageBy(-1));
        nextPageButton.onClick.AddListener(() => ChangePageBy(1));
        pageInputField.onEndEdit.AddListener(OnPageInputChanged);
    }

    private void OnLanguageChanged()
    {
        LoadLeaderboard();
    }

    public void ShowLeaderboard()
    {
        if (leaderboardPanel != null)
        {
            leaderboardPanel.SetActive(true);
            LoadLeaderboard();
        }
        else
        {
            Debug.LogError("Панель таблицы лидеров отсутствует!");
        }
    }

    public void HideLeaderboard()
    {
        leaderboardPanel.SetActive(false);
    }

    // Обновленный метод AddScore, теперь использующий корутину
    public void AddScore(string playerName, int score)
    {
        StartCoroutine(AddScoreCoroutine(playerName, score));
    }

    // Новый метод-корутина для добавления счета
    private IEnumerator AddScoreCoroutine(string playerName, int score)
    {
        // Сначала загружаем текущую таблицу лидеров
        bool leaderboardLoaded = false;
        LeaderboardCreator.GetLeaderboard(LeaderboardPublicKey, new LeaderboardSearchQuery(), entries =>
        {
            currentEntries = entries;
            leaderboardLoaded = true;
        }, error =>
        {
            Debug.LogError($"Не удалось загрузить таблицу лидеров: {error}");
            leaderboardLoaded = true;
        });

        yield return new WaitUntil(() => leaderboardLoaded);

        // Проверяем, существует ли уже имя игрока
        string uniquePlayerName = playerName;
        int suffix = 2;
        while (currentEntries.Any(e => e.Username == uniquePlayerName))
        {
            uniquePlayerName = $"{playerName}{suffix}";
            suffix++;
        }

        // Загружаем счет с уникальным именем игрока
        LeaderboardCreator.UploadNewEntry(LeaderboardPublicKey, uniquePlayerName, score, success =>
        {
            if (success)
            {
                Debug.Log($"Счет успешно загружен для {uniquePlayerName}");
                LoadLeaderboard();
            }
            else
            {
                Debug.LogError($"Не удалось загрузить счет для {uniquePlayerName}");
            }
        });
    }

    private void LoadLeaderboard()
    {
        ToggleLoadingPanel(true);

        var searchQuery = new LeaderboardSearchQuery
        {
            Skip = (currentPage - 1) * EntriesPerPage,
            Take = EntriesPerPage,
            TimePeriod = Dan.Enums.TimePeriodType.AllTime
        };

        LeaderboardCreator.GetLeaderboard(LeaderboardPublicKey, searchQuery, OnLeaderboardLoaded, ErrorCallback);
    }

    private void OnLeaderboardLoaded(Entry[] entries)
    {
        foreach (Transform child in leaderboardContent)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < entries.Length; i++)
        {
            GameObject entryObject = Instantiate(entryDisplayPrefab, leaderboardContent);
            EntryDisplay entryDisplay = entryObject.GetComponent<EntryDisplay>();
            entryDisplay.SetEntry(entries[i]);
        }

        UpdatePageDisplay();
        ToggleLoadingPanel(false);
    }

    private void ChangePageBy(int amount)
    {
        currentPage += amount;
        if (currentPage < 1) currentPage = 1;
        LoadLeaderboard();
    }

    private void OnPageInputChanged(string input)
    {
        if (int.TryParse(input, out int page) && page > 0)
        {
            currentPage = page;
            LoadLeaderboard();
        }
        else
        {
            UpdatePageDisplay();
        }
    }

    private void UpdatePageDisplay()
    {
        pageInputField.text = currentPage.ToString();
    }

    private void ToggleLoadingPanel(bool isOn)
    {
        if (loadingPanel != null)
        {
            loadingPanel.alpha = isOn ? 1f : 0f;
            loadingPanel.interactable = isOn;
            loadingPanel.blocksRaycasts = isOn;
        }
    }

    private void ErrorCallback(string error)
    {
        Debug.LogError($"Ошибка таблицы лидеров: {error}");
        ToggleLoadingPanel(false);
    }

    public void CheckForNewScore()
    {
        if (PlayerPrefs.HasKey("LastPlayerName") && PlayerPrefs.HasKey("LastScore"))
        {
            string playerName = PlayerPrefs.GetString("LastPlayerName");
            int score = PlayerPrefs.GetInt("LastScore");
            AddScore(playerName, score);

            PlayerPrefs.DeleteKey("LastPlayerName");
            PlayerPrefs.DeleteKey("LastScore");
            PlayerPrefs.Save();
        }
    }
}