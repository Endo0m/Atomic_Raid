
using UnityEngine;
public class FlashlightController : MonoBehaviour
{
    [SerializeField] private Light flashlight;

    private void Start()
    {
        if (flashlight == null)
        {
            flashlight = GetComponentInChildren<Light>();
        }

        if (flashlight == null)
        {
            Debug.LogError("Flashlight component not found!");
        }
    }

    public void TurnOn()
    {
        if (flashlight != null)
        {
            flashlight.enabled = true;
        }
    }

    public void TurnOff()
    {
        if (flashlight != null)
        {
            flashlight.enabled = false;
        }
    }
}