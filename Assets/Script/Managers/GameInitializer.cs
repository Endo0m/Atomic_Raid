using UnityEngine;
using UnityEngine.SceneManagement;

public class GameInitializer : MonoBehaviour
{
    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Game") // �������� �� ��� ����� ������� �����
        {
            ResetGameState();
        }
    }

    private void ResetGameState()
    {
        Time.timeScale = 1f;

        CameraController cameraController = FindObjectOfType<CameraController>();
        if (cameraController != null)
        {
            cameraController.ResetState();
        }

        PlayerAimController playerAimController = FindObjectOfType<PlayerAimController>();
        if (playerAimController != null)
        {
            playerAimController.ResetState();
        }

        // �������� ����� ��������� ��� ������ ������ �����������, ���� ����������
    }
}