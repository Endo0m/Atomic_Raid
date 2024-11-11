using UnityEngine;
using System.Collections.Generic;

public class InputHandler : MonoBehaviour
{
    public static InputHandler Instance { get; private set; }

    // Словарь для хранения назначенных клавиш
    private Dictionary<string, KeyCode> keyBindings = new Dictionary<string, KeyCode>();

    // Значения по умолчанию
    private Dictionary<string, KeyCode> defaultKeyBindings = new Dictionary<string, KeyCode>
    {
        {"MoveUp", KeyCode.W},
        {"MoveDown", KeyCode.S},
        {"MoveLeft", KeyCode.A},
        {"MoveRight", KeyCode.D},
        {"Shoot", KeyCode.Mouse0},
        {"Pause", KeyCode.Escape}
    };

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadKeyBindings();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public Vector2 GetMovementInput()
    {
        float horizontal = 0f;
        float vertical = 0f;

        if (Input.GetKey(keyBindings["MoveLeft"])) horizontal -= 1f;
        if (Input.GetKey(keyBindings["MoveRight"])) horizontal += 1f;
        if (Input.GetKey(keyBindings["MoveDown"])) vertical -= 1f;
        if (Input.GetKey(keyBindings["MoveUp"])) vertical += 1f;

        return new Vector2(horizontal, vertical).normalized;
    }

    public bool GetShootInput()
    {
        return Input.GetKey(keyBindings["Shoot"]);
    }

    public bool GetPauseInput()
    {
        return Input.GetKeyDown(keyBindings["Pause"]);
    }

    public void SetKeyBinding(string action, KeyCode key)
    {
        keyBindings[action] = key;
        SaveKeyBindings();
    }

    public KeyCode GetKeyBinding(string action)
    {
        return keyBindings[action];
    }

    public void ResetToDefaults()
    {
        keyBindings = new Dictionary<string, KeyCode>(defaultKeyBindings);
        SaveKeyBindings();
    }

    private void SaveKeyBindings()
    {
        foreach (var binding in keyBindings)
        {
            PlayerPrefs.SetInt(binding.Key, (int)binding.Value);
        }
        PlayerPrefs.Save();
    }

    private void LoadKeyBindings()
    {
        keyBindings = new Dictionary<string, KeyCode>();
        foreach (var binding in defaultKeyBindings)
        {
            keyBindings[binding.Key] = (KeyCode)PlayerPrefs.GetInt(binding.Key, (int)binding.Value);
        }
    }
}