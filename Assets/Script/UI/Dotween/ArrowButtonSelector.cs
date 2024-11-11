using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

public class ArrowButtonSelector : MonoBehaviour
{
    [SerializeField] private RectTransform arrowIndicator;
    [SerializeField] private float selectionTransitionDuration = 0.3f;
    [SerializeField] private List<RectTransform> targets;
    [SerializeField] private List<Slider> sliders;
    [SerializeField] private Button[] buttons;
    [SerializeField] private bool handleInputInternally = true;
    [SerializeField] private bool includeSliders = true;
    [SerializeField] private ButtonFX buttonFX;
    private int currentSelection = 0;

    private void Start()
    {
        SelectElement(0);
        SetupHoverDetectors();
    }

    private void SetupHoverDetectors()
    {
        int index = 0;
        foreach (var button in buttons)
        {
            ButtonHoverDetector detector = button.gameObject.AddComponent<ButtonHoverDetector>();
            detector.selector = this;
            detector.index = index++;
            detector.isSlider = false;
        }

        if (includeSliders)
        {
            foreach (var slider in sliders)
            {
                ButtonHoverDetector detector = slider.gameObject.AddComponent<ButtonHoverDetector>();
                detector.selector = this;
                detector.index = index++;
                detector.isSlider = true;
            }
        }
    }

    private void Update()
    {
        if (handleInputInternally)
        {
            HandleInput();
        }
    }

    public void HandleInput()
    {
        if (!gameObject.activeSelf) return;

        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            ChangeSelection(-1);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            ChangeSelection(1);
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            AdjustCurrentSlider(-0.1f);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            AdjustCurrentSlider(0.1f);
        }
        else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            PressSelectedButton();
        }
    }

    public void ChangeSelection(int direction)
    {
        int totalElements = buttons.Length + (includeSliders ? sliders.Count : 0);
        currentSelection = (currentSelection + direction + totalElements) % totalElements;
        SelectElement(currentSelection);
    }

    public void SelectElement(int index)
    {
        currentSelection = index;
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].interactable = true;
        }

        arrowIndicator.DOMove(targets[index].transform.position, selectionTransitionDuration)
            .SetEase(Ease.OutQuad);
        if (buttonFX != null)
        {
            buttonFX.HoverSound();
        }
    }

    public void OnElementHover(int index, bool isSlider)
    {
        SelectElement(index);
    }

    public void OnElementUnhover()
    {
        // Можно оставить пустым или добавить дополнительную логику при необходимости
    }

    public void OnElementClick(int index)
    {
        if (index < buttons.Length)
        {
            buttons[index].onClick.Invoke();
        }
        else if (index - buttons.Length < sliders.Count)
        {
            // Обработка клика на слайдер, если нужно
        }
    }

    public void PressSelectedButton()
    {
        if (currentSelection < buttons.Length)
        {
            buttons[currentSelection].onClick.Invoke();
        }
        if (buttonFX != null)
        {
            buttonFX.PressedFX();
        }
    }

    public void UpdateSliderValue(int index, float value)
    {
        if (index >= buttons.Length && index - buttons.Length < sliders.Count)
        {
            int sliderIndex = index - buttons.Length;
            sliders[sliderIndex].value = value;
            // Здесь можно добавить дополнительную логику обработки изменения значения слайдера
        }
    }

    private void AdjustCurrentSlider(float amount)
    {
        if (currentSelection >= buttons.Length && currentSelection - buttons.Length < sliders.Count)
        {
            int sliderIndex = currentSelection - buttons.Length;
            Slider currentSlider = sliders[sliderIndex];
            currentSlider.value = Mathf.Clamp(currentSlider.value + amount, currentSlider.minValue, currentSlider.maxValue);
        }
    }
}