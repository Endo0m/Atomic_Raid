using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance;
    public float globalGameTime = 0f;
    private bool isPaused = false;

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

    private void Update()
    {
        if (!isPaused)
        {
            globalGameTime += Time.unscaledDeltaTime; // Используем unscaledDeltaTime вместо deltaTime
        }
    }

    public void PauseTime()
    {
        isPaused = true;
    }

    public void ResumeTime()
    {
        isPaused = false;
    }
}