using UnityEngine;
using UnityEngine.UI;

public class ImageMouse : MonoBehaviour
{
    [SerializeField] private Image crosshairImage;

    private void Start()
    {
        if (crosshairImage == null)
        {
            crosshairImage = GetComponent<Image>();
        }

        Cursor.visible = false;
    }

    private void Update()
    {
        if (crosshairImage != null)
        {
            crosshairImage.rectTransform.position = Input.mousePosition;
        }
    }

    public void ShowCrosshair(bool show)
    {
        if (crosshairImage != null)
        {
            crosshairImage.gameObject.SetActive(show);
        }

        Cursor.visible = !show;
    }
}