using UnityEngine;
using UnityEngine.UI;

public class BossHealthUI : MonoBehaviour
{
    [SerializeField] private Slider[] healthBars;
    [SerializeField] private GameObject bossHealthPanel;

    private static BossHealthUI instance;
    public static BossHealthUI Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<BossHealthUI>();
                if (instance == null)
                {
                    Debug.LogError("BossHealthUI not found in the scene!");
                }
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void Initialize(int maxHealth)
    {
        ShowHealthUI(true);
        UpdateHealth(maxHealth, maxHealth); // Исправлено: передаем maxHealth дважды
    }

    public void UpdateHealth(int currentHealth, int maxHealth)
    {
        float healthPercentage = (float)currentHealth / maxHealth;

        if (healthPercentage > 0.5f)
        {
            SetBarValue(0, (healthPercentage - 0.5f) * 2);
            SetBarValue(1, 1);
            SetBarValue(2, 1);
        }
        else if (healthPercentage > 0.25f)
        {
            SetBarValue(0, 0);
            SetBarValue(1, (healthPercentage - 0.25f) * 4);
            SetBarValue(2, 1);
        }
        else
        {
            SetBarValue(0, 0);
            SetBarValue(1, 0);
            SetBarValue(2, healthPercentage * 4);
        }
    }

    private void SetBarValue(int index, float value)
    {
        if (index < healthBars.Length)
        {
            healthBars[index].value = value;
        }
    }

    public void ShowHealthUI(bool show)
    {
        bossHealthPanel.SetActive(show);
    }
}