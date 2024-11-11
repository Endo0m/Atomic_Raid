using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;
using UnityEngine.SceneManagement;
using System.Collections;
using Dan.Main;
using UnityEngine.Audio;
using System.Linq;
using Dan.Models;

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance { get; private set; }

    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject choicePanel;
    [SerializeField] private LeaderboardManager leaderboardManager;
    private bool isGameOver = false;

    private TMP_InputField nameInputField;
    private Button submitButton;
    [SerializeField] private TextMeshProUGUI warningText;
    private Button mainMenuButton;
    private Button restartLevelButton;

    private const string LeaderboardPublicKey = "9346e84b22b1b196eaa7dba41106371ffeb7943073fb8859d8803f795235948d";
    private Entry[] currentEntries;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        InitializeComponents();
    }


    private void Update()
    {
        if (isGameOver && Time.timeScale != 0)
        {
            Time.timeScale = 0;
        }
    }

    private void AutoEnterName()
    {
        string lastPlayerName = PlayerPrefs.GetString("LastPlayerName", "");
        if (!string.IsNullOrEmpty(lastPlayerName))
        {
            nameInputField.text = lastPlayerName;
            ValidateInput(lastPlayerName);
        }
        nameInputField.Select();
        nameInputField.ActivateInputField();
    }

    private void InitializeComponents()
    {
        nameInputField = gameOverPanel.GetComponentInChildren<TMP_InputField>();
        submitButton = gameOverPanel.GetComponentInChildren<Button>();
        nameInputField.characterLimit = 8;
        mainMenuButton = choicePanel.GetComponentsInChildren<Button>()[0];
        restartLevelButton = choicePanel.GetComponentsInChildren<Button>()[1];

        submitButton.onClick.AddListener(SubmitScore);
        nameInputField.onValueChanged.AddListener(ValidateInput);
        mainMenuButton.onClick.AddListener(GoToMainMenu);
        restartLevelButton.onClick.AddListener(RestartLevel);

        gameOverPanel.SetActive(false);
        choicePanel.SetActive(false);
    }

    public bool IsGameOver()
    {
        return isGameOver;
    }

    private IEnumerator HideWarningText()
    {
        yield return new WaitForSeconds(5f);
        warningText.text = "";
    }

    public void ShowGameOver()
    {
        if (!isGameOver)
        {
            isGameOver = true;
            gameOverPanel.SetActive(true);
            choicePanel.SetActive(false);
            CursorManager.Instance.SetDefaultCursor();

            PlayerMove playerController = FindObjectOfType<PlayerMove>();
            if (playerController != null)
            {
                playerController.enabled = false;
            }

            EnemyWaveGenerator enemyWaveGenerator = FindObjectOfType<EnemyWaveGenerator>();
            if (enemyWaveGenerator != null)
            {
                enemyWaveGenerator.StopEnemyGeneration();
            }

            EnvironmentStaticGenerator environmentGenerator = FindObjectOfType<EnvironmentStaticGenerator>();
            if (environmentGenerator != null)
            {
                environmentGenerator.StopMovement();
            }

            SetFinalScore();
            AutoEnterName();

            Time.timeScale = 0;
        }
    }

    private void SetFinalScore()
    {
        int finalScore = ScoreManager.Instance.GetScore();
    }

    public void SubmitScore()
    {
        try
        {
            string playerName = nameInputField.text;
            if (ValidateName(playerName))
            {
                StartCoroutine(SubmitScoreCoroutine(playerName));
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error in SubmitScore: " + e.Message);
            ShowWarning("An error occurred. Please try again.");
        }
    }

    private IEnumerator SubmitScoreCoroutine(string playerName)
    {
        int score = ScoreManager.Instance.GetScore();

        // Загружаем текущую таблицу лидеров
        bool leaderboardLoaded = false;
        LeaderboardCreator.GetLeaderboard(LeaderboardPublicKey, new Dan.Models.LeaderboardSearchQuery(), entries =>
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

        // Сохраняем уникальное имя
        PlayerPrefs.SetString("LastPlayerName", uniquePlayerName);
        PlayerPrefs.Save();

        // Отправляем счет с уникальным именем
        LeaderboardCreator.UploadNewEntry(LeaderboardPublicKey, uniquePlayerName, score, success =>
        {
            if (success)
            {
                Debug.Log($"Score submitted successfully for {uniquePlayerName}");
                if (leaderboardManager != null)
                {
                    leaderboardManager.ShowLeaderboard();
                }
                gameOverPanel.SetActive(false);
                ShowChoicePanel();
            }
            else
            {
                Debug.LogError($"Failed to submit score for {uniquePlayerName}");
                ShowWarning("Failed to submit score. Please try again.");
            }
        });
    }


    private void ShowChoicePanel()
    {
        choicePanel.SetActive(true);
        CursorManager.Instance.SetDefaultCursor();
        if (leaderboardManager != null)
        {
            leaderboardManager.ShowLeaderboard();
        }
        else
        {
            Debug.LogError("LeaderboardManager is null!");
        }
    }

    private void RestartLevel()
    {
        Time.timeScale = 1;
        isGameOver = false;
        ScoreManager.Instance.ResetScore();
        StartCoroutine(RestartLevelCoroutine());
    }

    private IEnumerator RestartLevelCoroutine()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        yield return new WaitForEndOfFrame();

        VolumeInit volumeInit = FindObjectOfType<VolumeInit>();
        if (volumeInit != null)
        {
            AudioMixer mixer = volumeInit.GetAudioMixer();
            AudioPlayer.Instance.UpdateAudioMixer(mixer);

            float musicVolume = PlayerPrefs.GetFloat("MusicVol", 0f);
            float soundVolume = PlayerPrefs.GetFloat("SoundVol", 0f);
            AudioPlayer.Instance.UpdateVolume("MusicVol", musicVolume);
            AudioPlayer.Instance.UpdateVolume("SoundVol", soundVolume);
        }

        if (AudioPlayer.Instance != null)
        {
            AudioPlayer.Instance.UpdateAudioMixer(FindObjectOfType<VolumeInit>()?.GetAudioMixer());
        }
        else
        {
            Debug.LogError("AudioPlayer.Instance not found after restart!");
        }

        VolumeController volumeController = FindObjectOfType<VolumeController>();
        if (volumeController != null)
        {
            volumeController.Start();
        }
        else
        {
            Debug.LogError("VolumeController not found after restart!");
        }
    }

    private void GoToMainMenu()
    {
        Time.timeScale = 1f;
        isGameOver = false;
        CursorManager.Instance.SetDefaultCursor();
        ScoreManager.Instance.ResetScore();
        SceneManager.LoadScene("Menu");
    }

    private void ValidateInput(string input)
    {
        string validatedInput = Regex.Replace(input, @"[^a-zA-Z0-9а-яА-Я-]", "");
        validatedInput = validatedInput.Substring(0, Mathf.Min(validatedInput.Length, 8));
        nameInputField.text = validatedInput;
        submitButton.interactable = validatedInput.Length >= 3;
    }

    private bool ValidateName(string name)
    {
        if (name.Length < 3)
        {
            ShowWarning("Имя должно содержать минимум 3 символа");
            return false;
        }

        if (!Regex.IsMatch(name, @"^[a-zA-Z0-9а-яА-Я-]+$"))
        {
            ShowWarning("Имя может содержать только буквы, цифры и тире");
            return false;
        }

        return true;
    }

    private void ShowWarning(string message)
    {
        warningText.text = message;
        StartCoroutine(HideWarningText());
    }
}