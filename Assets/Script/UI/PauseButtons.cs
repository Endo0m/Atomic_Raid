using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseButtons : MonoBehaviour
{
    public void MainMenu()
    {
        Time.timeScale = 1f; // Восстановить нормальное течение времени
        SceneManager.LoadScene("Menu");
    }
}
