using UnityEngine;

public class WorldSpaceCanvasScaler : MonoBehaviour
{
    public Vector2 referenceResolution = new Vector2(3840, 2160);
    public Camera uiCamera;

    private Canvas canvas;
    private RectTransform canvasRect;

    private void Start()
    {
        canvas = GetComponent<Canvas>();
        canvasRect = canvas.GetComponent<RectTransform>();
        uiCamera = canvas.worldCamera;

        if (uiCamera == null)
        {
            Debug.LogError("World Space Canvas needs a camera reference!");
            return;
        }

        ScaleCanvas();
    }

    private void Update()
    {
        ScaleCanvas();
    }

    private void ScaleCanvas()
    {
        if (uiCamera == null) return;

        float targetAspect = referenceResolution.x / referenceResolution.y;
        float windowAspect = (float)Screen.width / (float)Screen.height;
        float scaleHeight = windowAspect / targetAspect;

        if (scaleHeight < 1.0f)
        {
            float scaleFactor = 1.0f / scaleHeight;
            canvasRect.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
        }
        else
        {
            canvasRect.localScale = Vector3.one;
        }

        PositionCanvas();
    }

    private void PositionCanvas()
    {
        Vector3 cameraForward = uiCamera.transform.forward;
        float distanceFromCamera = 10f; // Adjust this value to change the distance of the canvas from the camera
        canvasRect.position = uiCamera.transform.position + cameraForward * distanceFromCamera;
        canvasRect.forward = cameraForward;
    }
}