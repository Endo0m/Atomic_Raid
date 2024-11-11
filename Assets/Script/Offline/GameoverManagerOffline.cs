using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameoverManagerOffline : MonoBehaviour
{
    public static GameoverManagerOffline Instance { get; private set; }

    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject choicePanel;
    [SerializeField] private LeaderboardOffline leaderboardManager;
    private bool isGameOver = false;

    private TMP_InputField nameInputField;
    private Button submitButton;
    [SerializeField]
    private TextMeshProUGUI warningText;
    private Button mainMenuButton;
    private Button restartLevelButton;
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
        Debug.Log("AutoEnterName started");
        string lastPlayerName = PlayerPrefs.GetString("LastPlayerName", "");
        Debug.Log($"Last player name: {lastPlayerName}");
        if (!string.IsNullOrEmpty(lastPlayerName))
        {
            nameInputField.text = lastPlayerName;
            ValidateInput(lastPlayerName);
        }
        nameInputField.Select();
        nameInputField.ActivateInputField();
        Debug.Log("AutoEnterName completed");
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




    public void Massage()
    {
        Debug.Log("WARNING");
    }



    private IEnumerator HideWarningText()
    {
        yield return new WaitForSeconds(5f);
        warningText.text = "";
    }


    public void ShowGameOver()
    {
        Debug.Log("ShowGameOver called");
        if (!isGameOver)
        {
            Debug.Log("Game is now over");
            isGameOver = true;

            // ¬ременно закомментируйте эту строку дл€ теста
            // Time.timeScale = 0;

            Debug.Log("Activating game over panel");
            gameOverPanel.SetActive(true);
            Debug.Log("Deactivating choice panel");
            choicePanel.SetActive(false);

            Debug.Log("Setting default cursor");
            CursorManager.Instance.SetDefaultCursor();

            Debug.Log("Disabling player controller");
            PlayerMove playerController = FindObjectOfType<PlayerMove>();
            if (playerController != null)
            {
                playerController.enabled = false;
            }

            Debug.Log("Stopping enemy generation");
            EnemyWaveGenerator enemyWaveGenerator = FindObjectOfType<EnemyWaveGenerator>();
            if (enemyWaveGenerator != null)
            {
                enemyWaveGenerator.StopEnemyGeneration();
            }

            Debug.Log("Stopping environment movement");
            EnvironmentStaticGenerator environmentGenerator = FindObjectOfType<EnvironmentStaticGenerator>();
            if (environmentGenerator != null)
            {
                environmentGenerator.StopMovement();
            }

            Debug.Log("Setting final score");
            SetFinalScore();

            Debug.Log("Calling AutoEnterName");
            AutoEnterName();

            Debug.Log("ShowGameOver completed");
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
                int score = ScoreManager.Instance.GetScore();
                PlayerPrefs.SetString("LastPlayerName", playerName);
                PlayerPrefs.SetInt("LastScore", score);
                PlayerPrefs.Save();

                Debug.Log("Score submitted successfully");

                if (leaderboardManager != null)
                {
                    leaderboardManager.AddScore(playerName, score);
                    Debug.Log("Score added to leaderboard");
                }
                else
                {
                    Debug.LogWarning("LeaderboardManager is null");
                }

                gameOverPanel.SetActive(false);
                ShowChoicePanel();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error in SubmitScore: " + e.Message);
        }
    }

    private void ShowChoicePanel()
    {
        choicePanel.SetActive(true);
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
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        StartCoroutine(ReinitializeAudioSettings());
    }

    private IEnumerator ReinitializeAudioSettings()
    {
        yield return null; // ∆дем один кадр, чтобы сцена успела загрузитьс€
        FindObjectOfType<VolumeInit>()?.InitializeVolumes();
        FindObjectOfType<VolumeController>()?.Start();
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
        Debug.Log($"ValidateInput called with input: {input}");

        // »зменим регул€рное выражение, чтобы оно включало кириллицу
        string validatedInput = Regex.Replace(input, @"[^a-zA-Z0-9а-€ј-я-]", "");

        validatedInput = validatedInput.Substring(0, Mathf.Min(validatedInput.Length, 8));

        Debug.Log($"Validated input: {validatedInput}");

        nameInputField.text = validatedInput;
        submitButton.interactable = validatedInput.Length >= 3;

        Debug.Log($"Submit button interactable: {submitButton.interactable}");
    }
    private bool ValidateName(string name)
    {
        Debug.Log($"ValidateName called with name: {name}");

        if (name.Length < 3)
        {
            ShowWarning("»м€ должно содержать минимум 3 символа");
            return false;
        }

        // »зменим регул€рное выражение, чтобы оно включало кириллицу
        if (!Regex.IsMatch(name, @"^[a-zA-Z0-9а-€ј-я-]+$"))
        {
            ShowWarning("»м€ может содержать только буквы, цифры и дефис");
            return false;
        }

        Debug.Log("Name is valid");
        return true;
    }

    private void ShowWarning(string message)
    {
        warningText.text = message;
        HideWarningText();
    }
}
