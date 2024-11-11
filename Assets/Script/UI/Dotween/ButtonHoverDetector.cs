using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonHoverDetector : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IDragHandler
{
    public ArrowButtonSelector selector;
    public int index;
    public bool isSlider;

    private Slider slider;

    private void Start()
    {
        if (isSlider)
        {
            slider = GetComponent<Slider>();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        selector.OnElementHover(index, isSlider);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        selector.OnElementUnhover();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        selector.OnElementClick(index);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isSlider && slider != null)
        {
            selector.UpdateSliderValue(index, slider.value);
        }
    }
}