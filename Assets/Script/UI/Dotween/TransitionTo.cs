using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Playables;
using Cinemachine;
using DG.Tweening;

public class TransitionTo : MonoBehaviour
{
    [SerializeField] private PanelTransitionManager transitionManager;
    [SerializeField] private SettingsSubsectionManager settingsSubsectionManager;
    [SerializeField] private CinemachineVirtualCamera mainCamera;
    [SerializeField] private Transform mainMenuCameraPosition;
    [SerializeField] private Transform submenuCameraPosition;
    [SerializeField] private float cameraMoveTime = 1f;
    [SerializeField] private Ease cameraEase = Ease.InOutQuad;
    [SerializeField] private PlayableDirector gameStartDirector;

    private bool isInSubmenu = false;
    private bool isTransitioning = false;

    public void StartGame()
    {
        if (isTransitioning) return;
        isTransitioning = true;
        FindObjectOfType<FirstFieldDetector>().ResetFirstFieldDetected();
        PlayTransitionTimeline();
    }

    public void TransitionToMenu()
    {
        if (isTransitioning) return;
        isTransitioning = true;
        if (isInSubmenu)
        {
            MoveCameraToPosition(mainMenuCameraPosition, "Screen_Menu");
            isInSubmenu = false;
        }
        else
        {
            transitionManager.TransitionToScreen("Screen_Menu");
            isTransitioning = false;
        }
    }

    public void TransitionToSettings()
    {
        if (isTransitioning) return;
        isTransitioning = true;
        MoveCameraToPosition(submenuCameraPosition, "Screen_Settings");
        isInSubmenu = true;
        // Добавляем вызов TransitionToSettingsGame() после перехода к экрану настроек
        TransitionToSettingsGame();
    }

    public void TransitionToLeaderboard()
    {
        if (isTransitioning) return;
        isTransitioning = true;
        MoveCameraToPosition(submenuCameraPosition, "Screen_Leaderboard");
        isInSubmenu = true;
    }

    public void TransitionToAuthors()
    {
        if (isTransitioning) return;
        isTransitioning = true;
        MoveCameraToPosition(submenuCameraPosition, "Screen_Authors");
        isInSubmenu = true;
    }

    public void ExitGame()
    {
        Application.Quit();
        Debug.Log("EXIT");
    }

    // Методы для переключения подразделов настроек
    public void TransitionToSettingsGame()
    {
        if (settingsSubsectionManager != null)
        {
            settingsSubsectionManager.TransitionToSubsection("SettingsScreenPanel_Game");
        }
    }

    public void TransitionToSettingsSound()
    {
        if (settingsSubsectionManager != null)
        {
            settingsSubsectionManager.TransitionToSubsection("SettingsScreenPanel_Sound");
        }
    }

    public void TransitionToSettingsVideo()
    {
        if (settingsSubsectionManager != null)
        {
            settingsSubsectionManager.TransitionToSubsection("SettingsScreenPanel_Video");
        }
    }

    public void TransitionToSettingsManagement()
    {
        if (settingsSubsectionManager != null)
        {
            settingsSubsectionManager.TransitionToSubsection("SettingsScreenPanel_Management");
        }
    }

    private void MoveCameraToPosition(Transform targetPosition, string targetScreen)
    {
        Sequence sequence = DOTween.Sequence();
        sequence.Append(mainCamera.transform.DOMove(targetPosition.position, cameraMoveTime).SetEase(cameraEase));
        sequence.Join(mainCamera.transform.DORotate(targetPosition.rotation.eulerAngles, cameraMoveTime).SetEase(cameraEase));
        sequence.OnComplete(() => {
            transitionManager.TransitionToScreen(targetScreen);
            isTransitioning = false;
        });
    }

    private void PlayTransitionTimeline()
    {
        if (gameStartDirector != null)
        {
            gameStartDirector.Play();
            gameStartDirector.stopped += OnTimelineFinished;
        }
        else
        {
            Debug.LogError("GameStartDirector not found!");
            OnTimelineFinished(null);
        }
    }

    private void OnTimelineFinished(PlayableDirector director)
    {
        if (director != null)
        {
            director.stopped -= OnTimelineFinished;
        }
        SceneManager.LoadScene("Cutscene");
        isTransitioning = false;
    }
}