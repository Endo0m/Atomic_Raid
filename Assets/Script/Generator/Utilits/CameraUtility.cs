using UnityEngine;

public static class CameraUtility
{
    public static bool IsOutOfCameraView(GameObject obj, Camera camera, float offScreenOffset)
    {
        return obj.transform.position.z < camera.transform.position.z - offScreenOffset;
    }
}