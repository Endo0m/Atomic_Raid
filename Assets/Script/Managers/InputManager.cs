using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    private PlayerInput _playerInput;
    private InputActionRebindingExtensions.RebindingOperation _rebindingOperation;

    // Словарь для хранения исходных привязок
    private Dictionary<string, string> _defaultBindings = new Dictionary<string, string>();

    // Событие для уведомления UI об изменении привязок
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

        // Сохранение исходных привязок
        SaveDefaultBindings();

        // Загрузка сохраненных привязок
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

    // Метод для начала переназначения кнопки
    public void StartRebind(string actionName, int bindingIndex)
    {
        InputAction action = _playerInput.asset.FindAction(actionName);
        if (action != null && _rebindingOperation == null)
        {
            // Отключаем действие перед переназначением
            action.Disable();

            _rebindingOperation = action.PerformInteractiveRebinding(bindingIndex)
                .WithControlsExcluding("Mouse")
                .OnMatchWaitForAnother(0.1f)
                .OnComplete(operation => CompleteRebind(operation, actionName, bindingIndex))
                .OnCancel(operation => CancelRebind(operation, actionName))
                .Start();
        }
    }

    // Метод для завершения переназначения кнопки
    private void CompleteRebind(InputActionRebindingExtensions.RebindingOperation operation, string actionName, int bindingIndex)
    {
        _rebindingOperation = null;
        operation.Dispose();

        InputAction action = _playerInput.asset.FindAction(actionName);
        // Включаем действие после переназначения
        action.Enable();

        SaveBindings();
        OnBindingsUpdated?.Invoke();
    }

    // Метод для отмены переназначения кнопки
    private void CancelRebind(InputActionRebindingExtensions.RebindingOperation operation, string actionName)
    {
        _rebindingOperation = null;
        operation.Dispose();

        InputAction action = _playerInput.asset.FindAction(actionName);
        // Включаем действие после отмены переназначения
        action.Enable();

        OnBindingsUpdated?.Invoke();
    }

    // Метод для сброса всех привязок на исходные
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

    // Метод для сохранения текущих привязок
    private void SaveBindings()
    {
        string rebinds = _playerInput.asset.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString("InputBindings", rebinds);
        PlayerPrefs.Save();
    }

    // Метод для загрузки сохраненных привязок
    private void LoadBindings()
    {
        string rebinds = PlayerPrefs.GetString("InputBindings");
        if (!string.IsNullOrEmpty(rebinds))
        {
            _playerInput.asset.LoadBindingOverridesFromJson(rebinds);
        }
    }

    // Метод для сохранения исходных привязок
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

    // Метод для получения текущей привязки действия
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
        // Словарь для специальных клавиш
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
            // Добавьте другие специальные клавиши по необходимости
        };

        // Проверяем, есть ли клавиша в словаре специальных клавиш
        if (specialKeys.TryGetValue(bindingPath, out string specialKeyName))
        {
            return specialKeyName;
        }

        // Для обычных клавиш убираем префикс "<Keyboard>/"
        if (bindingPath.StartsWith("<Keyboard>/"))
        {
            return bindingPath.Substring(11).ToUpper();
        }

        // Если это не специальная клавиша и не клавиша клавиатуры, возвращаем путь как есть
        return bindingPath;
    }
}