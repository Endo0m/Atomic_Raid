using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    public static PauseMenuManager Instance { get; private set; }
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject windowPause;
    [SerializeField] private GameObject windowSettings;
    [SerializeField] private GameOverManager gameOverManager;
    private bool isPaused = false;
    private Button[] buttons;

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
        InitializePauseMenu();
    }

    private void InitializePauseMenu()
    {
        pauseMenu.SetActive(false);
        windowPause.SetActive(true);
        windowSettings.SetActive(false);
        Button resumeButton = windowPause.transform.Find("ResumeButton").GetComponent<Button>();
        Button settingsButton = windowPause.transform.Find("SettingsButton").GetComponent<Button>();
        Button exitButton = windowPause.transform.Find("ExitButton").GetComponent<Button>();
        resumeButton.onClick.AddListener(Resume);
        settingsButton.onClick.AddListener(OpenSettings);
        exitButton.onClick.AddListener(QuitGame);
        Button backButton = windowSettings.transform.Find("BackButton").GetComponent<Button>();
        backButton.onClick.AddListener(CloseSettings);
        buttons = new Button[] { resumeButton, settingsButton, exitButton };
    }

    private void Update()
    {
        if (InputHandler.Instance.GetPauseInput() && !gameOverManager.IsGameOver() && !HelpPanel.Instance.IsPanelActive())
        {
            if (isPaused)
            {
                if (windowSettings.activeSelf)
                {
                    CloseSettings();
                }
                else
                {
                    Resume();
                }
            }
            else
            {
                Pause();
            }
        }
    }

    public void Resume()
    {
        if (!gameOverManager.IsGameOver())
        {
            pauseMenu.SetActive(false);
            Time.timeScale = 1f;
            isPaused = false;
            TimeManager.Instance.ResumeTime();
            CursorManager.Instance.SetGameplayCursor();
        }
    }

    public void Pause()
    {
        if (!gameOverManager.IsGameOver())
        {
            TimeManager.Instance.PauseTime();
            pauseMenu.SetActive(true);
            windowPause.SetActive(true);
            windowSettings.SetActive(false);
            Time.timeScale = 0f;
            isPaused = true;
            CursorManager.Instance.SetDefaultCursor();
        }
    }

    public void OpenSettings()
    {
        windowPause.SetActive(false);
        windowSettings.SetActive(true);
    }

    public void CloseSettings()
    {
        windowSettings.SetActive(false);
        windowPause.SetActive(true);
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        isPaused = false;
        SceneManager.LoadScene("Menu");
    }

    public bool IsPaused()
    {
        return isPaused;
    }
}