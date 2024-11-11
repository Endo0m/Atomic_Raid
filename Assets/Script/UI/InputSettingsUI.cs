using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InputSettingsUI : MonoBehaviour
{
    [SerializeField] private Button _moveUpButton;
    [SerializeField] private Button _moveDownButton;
    [SerializeField] private Button _moveLeftButton;
    [SerializeField] private Button _moveRightButton;
    [SerializeField] private Button _shootButton;
    [SerializeField] private Button _pauseButton;
    [SerializeField] private Button _resetButton;
    [SerializeField] private TextMeshProUGUI _moveUpText;
    [SerializeField] private TextMeshProUGUI _moveDownText;
    [SerializeField] private TextMeshProUGUI _moveLeftText;
    [SerializeField] private TextMeshProUGUI _moveRightText;
    [SerializeField] private TextMeshProUGUI _shootText;
    [SerializeField] private TextMeshProUGUI _pauseText;

    private void Start()
    {
        SetupButtons();
        UpdateBindingTexts();
        InputManager.Instance.OnBindingsUpdated += UpdateBindingTexts;
    }

    private void OnDestroy()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnBindingsUpdated -= UpdateBindingTexts;
        }
    }

    private void SetupButtons()
    {
        _moveUpButton.onClick.AddListener(() => StartRebind("Move", 1));
        _moveDownButton.onClick.AddListener(() => StartRebind("Move", 2));
        _moveLeftButton.onClick.AddListener(() => StartRebind("Move", 3));
        _moveRightButton.onClick.AddListener(() => StartRebind("Move", 4));
        _shootButton.onClick.AddListener(() => StartRebind("Shoot", 0));
        _pauseButton.onClick.AddListener(() => StartRebind("Pause", 0));
        _resetButton.onClick.AddListener(ResetToDefaults);
    }

    private void StartRebind(string actionName, int bindingIndex)
    {
        InputManager.Instance.StartRebind(actionName, bindingIndex);
    }

    private void ResetToDefaults()
    {
        InputManager.Instance.ResetToDefaults();
    }

    private void UpdateBindingTexts()
    {
        _moveUpText.text = InputManager.Instance.GetBindingName("Move", 1);
        _moveDownText.text = InputManager.Instance.GetBindingName("Move", 2);
        _moveLeftText.text = InputManager.Instance.GetBindingName("Move", 3);
        _moveRightText.text = InputManager.Instance.GetBindingName("Move", 4);
        _shootText.text = InputManager.Instance.GetBindingName("Shoot", 0);
        _pauseText.text = InputManager.Instance.GetBindingName("Pause", 0);
    }
}