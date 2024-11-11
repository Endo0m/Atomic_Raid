using UnityEngine;

public class CursorManager : MonoBehaviour
{
    public static CursorManager Instance { get; private set; }

    private ImageMouse imageMouse;

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
        }
    }

    private void Start()
    {
        FindImageMouse();
    }

    private void FindImageMouse()
    {
        imageMouse = FindObjectOfType<ImageMouse>();
    }

    public void SetGameplayCursor()
    {
        if (imageMouse == null)
        {
            FindImageMouse();
        }

        if (imageMouse != null)
        {
            imageMouse.ShowCrosshair(true);
        }
    }

    public void SetDefaultCursor()
    {
        if (imageMouse == null)
        {
            FindImageMouse();
        }

        if (imageMouse != null)
        {
            imageMouse.ShowCrosshair(false);
        }
        else
        {
            Cursor.visible = true;
        }
    }
}