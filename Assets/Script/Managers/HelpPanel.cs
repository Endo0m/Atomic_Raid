using System.Collections;
using UnityEngine;

public class HelpPanel : MonoBehaviour
{
    private const string HAS_SEEN_HELP_KEY = "HasSeenHelp";
    public GameObject helpPanelUI;
    public float delayBeforeShow = 5f;
    private bool isPanelActive = false;

    public static HelpPanel Instance { get; private set; }

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

    void Start()
    {
        helpPanelUI.SetActive(false);
        if (!PlayerPrefs.HasKey(HAS_SEEN_HELP_KEY))
        {
            StartCoroutine(ShowPanelAfterDelay());
        }
        else
        {
            Time.timeScale = 1f;
        }
    }

    void Update()
    {
        if (isPanelActive && Input.GetKeyDown(KeyCode.Space))
        {
            ContinueGame();
        }
    }

    IEnumerator ShowPanelAfterDelay()
    {
        yield return new WaitForSeconds(delayBeforeShow);
        ShowPanel();
    }

    void ShowPanel()
    {
        helpPanelUI.SetActive(true);
        isPanelActive = true;
        Time.timeScale = 0f;
    }

    void ContinueGame()
    {
        helpPanelUI.SetActive(false);
        isPanelActive = false;
        Time.timeScale = 1f;
        PlayerPrefs.SetInt(HAS_SEEN_HELP_KEY, 1);
        PlayerPrefs.Save();
    }

    public bool IsPanelActive()
    {
        return isPanelActive;
    }

    public void ResetHasSeenHelp()
    {
        PlayerPrefs.DeleteKey(HAS_SEEN_HELP_KEY);
    }
}