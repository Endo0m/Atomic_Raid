using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class KeyBindingUI : MonoBehaviour
{
    [System.Serializable]
    public class KeyBindingButton
    {
        public string action;
        public Button button;
        public TextMeshProUGUI text;
    }

    public KeyBindingButton[] keyBindingButtons;
    public Button resetButton;

    private string currentlyRebinding = null;

    private void Start()
    {
        foreach (var keyBinding in keyBindingButtons)
        {
            UpdateKeyText(keyBinding);
            keyBinding.button.onClick.AddListener(() => StartRebinding(keyBinding.action));
        }

        resetButton.onClick.AddListener(ResetToDefaults);
    }

    private void Update()
    {
        if (currentlyRebinding != null)
        {
            if (Input.anyKeyDown)
            {
                foreach (KeyCode keyCode in System.Enum.GetValues(typeof(KeyCode)))
                {
                    if (Input.GetKeyDown(keyCode))
                    {
                        InputHandler.Instance.SetKeyBinding(currentlyRebinding, keyCode);
                        UpdateKeyText(keyBindingButtons.FirstOrDefault(kb => kb.action == currentlyRebinding));
                        currentlyRebinding = null;
                        break;
                    }
                }
            }
        }
    }

    private void StartRebinding(string action)
    {
        currentlyRebinding = action;
        KeyBindingButton keyBinding = keyBindingButtons.FirstOrDefault(kb => kb.action == action);
        if (keyBinding != null)
        {
            keyBinding.text.text = "Press any key...";
        }
    }

    private void UpdateKeyText(KeyBindingButton keyBinding)
    {
        if (keyBinding != null)
        {
            keyBinding.text.text = InputHandler.Instance.GetKeyBinding(keyBinding.action).ToString();
        }
    }

    private void ResetToDefaults()
    {
        InputHandler.Instance.ResetToDefaults();
        foreach (var keyBinding in keyBindingButtons)
        {
            UpdateKeyText(keyBinding);
        }
    }
}