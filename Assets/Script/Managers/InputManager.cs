using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    private PlayerInput _playerInput;
    private InputActionRebindingExtensions.RebindingOperation _rebindingOperation;

    // ������� ��� �������� �������� ��������
    private Dictionary<string, string> _defaultBindings = new Dictionary<string, string>();

    // ������� ��� ����������� UI �� ��������� ��������
    public delegate void BindingsUpdatedHandler();
    public event BindingsUpdatedHandler OnBindingsUpdated;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        _playerInput = new PlayerInput();

        // ���������� �������� ��������
        SaveDefaultBindings();

        // �������� ����������� ��������
        LoadBindings();
    }

    private void OnEnable()
    {
        _playerInput.Enable();
    }

    private void OnDisable()
    {
        _playerInput.Disable();
    }

    // ����� ��� ������ �������������� ������
    public void StartRebind(string actionName, int bindingIndex)
    {
        InputAction action = _playerInput.asset.FindAction(actionName);
        if (action != null && _rebindingOperation == null)
        {
            // ��������� �������� ����� ���������������
            action.Disable();

            _rebindingOperation = action.PerformInteractiveRebinding(bindingIndex)
                .WithControlsExcluding("Mouse")
                .OnMatchWaitForAnother(0.1f)
                .OnComplete(operation => CompleteRebind(operation, actionName, bindingIndex))
                .OnCancel(operation => CancelRebind(operation, actionName))
                .Start();
        }
    }

    // ����� ��� ���������� �������������� ������
    private void CompleteRebind(InputActionRebindingExtensions.RebindingOperation operation, string actionName, int bindingIndex)
    {
        _rebindingOperation = null;
        operation.Dispose();

        InputAction action = _playerInput.asset.FindAction(actionName);
        // �������� �������� ����� ��������������
        action.Enable();

        SaveBindings();
        OnBindingsUpdated?.Invoke();
    }

    // ����� ��� ������ �������������� ������
    private void CancelRebind(InputActionRebindingExtensions.RebindingOperation operation, string actionName)
    {
        _rebindingOperation = null;
        operation.Dispose();

        InputAction action = _playerInput.asset.FindAction(actionName);
        // �������� �������� ����� ������ ��������������
        action.Enable();

        OnBindingsUpdated?.Invoke();
    }

    // ����� ��� ������ ���� �������� �� ��������
    public void ResetToDefaults()
    {
        foreach (var binding in _defaultBindings)
        {
            string[] parts = binding.Key.Split('/');
            if (parts.Length == 2)
            {
                InputAction action = _playerInput.asset.FindAction(parts[0]);
                if (action != null)
                {
                    int bindingIndex = int.Parse(parts[1]);
                    action.ApplyBindingOverride(bindingIndex, binding.Value);
                }
            }
        }
        SaveBindings();
        OnBindingsUpdated?.Invoke();
    }

    // ����� ��� ���������� ������� ��������
    private void SaveBindings()
    {
        string rebinds = _playerInput.asset.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString("InputBindings", rebinds);
        PlayerPrefs.Save();
    }

    // ����� ��� �������� ����������� ��������
    private void LoadBindings()
    {
        string rebinds = PlayerPrefs.GetString("InputBindings");
        if (!string.IsNullOrEmpty(rebinds))
        {
            _playerInput.asset.LoadBindingOverridesFromJson(rebinds);
        }
    }

    // ����� ��� ���������� �������� ��������
    private void SaveDefaultBindings()
    {
        foreach (var map in _playerInput.asset.actionMaps)
        {
            foreach (var action in map.actions)
            {
                for (int i = 0; i < action.bindings.Count; i++)
                {
                    if (!action.bindings[i].isComposite)
                    {
                        _defaultBindings[$"{action.name}/{i}"] = action.bindings[i].effectivePath;
                    }
                }
            }
        }
    }

    // ����� ��� ��������� ������� �������� ��������
    public string GetBindingName(string actionName, int bindingIndex)
    {
        InputAction action = _playerInput.asset.FindAction(actionName);
        if (action != null && bindingIndex < action.bindings.Count)
        {
            InputBinding binding = action.bindings[bindingIndex];
            if (binding.isPartOfComposite)
            {
                return GetKeyName(binding.effectivePath);
            }
            return action.GetBindingDisplayString(bindingIndex);
        }
        return string.Empty;
    }

    private string GetKeyName(string bindingPath)
    {
        // ������� ��� ����������� ������
        Dictionary<string, string> specialKeys = new Dictionary<string, string>
        {
            {"<Keyboard>/escape", "Esc"},
            {"<Mouse>/leftButton", "LMB"},
            {"<Keyboard>/leftAlt", "Left Alt"},
            {"<Keyboard>/rightAlt", "Right Alt"},
            {"<Keyboard>/leftCtrl", "Left Ctrl"},
            {"<Keyboard>/rightCtrl", "Right Ctrl"},
            {"<Keyboard>/leftShift", "Left Shift"},
            {"<Keyboard>/rightShift", "Right Shift"},
            {"<Keyboard>/space", "Space"},
            // �������� ������ ����������� ������� �� �������������
        };

        // ���������, ���� �� ������� � ������� ����������� ������
        if (specialKeys.TryGetValue(bindingPath, out string specialKeyName))
        {
            return specialKeyName;
        }

        // ��� ������� ������ ������� ������� "<Keyboard>/"
        if (bindingPath.StartsWith("<Keyboard>/"))
        {
            return bindingPath.Substring(11).ToUpper();
        }

        // ���� ��� �� ����������� ������� � �� ������� ����������, ���������� ���� ��� ����
        return bindingPath;
    }
}