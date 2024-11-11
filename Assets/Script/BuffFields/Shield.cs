using UnityEngine;

public class Shield : MonoBehaviour
{

    public bool IsActive { get; private set; }

    private void OnEnable()
    {
        IsActive = true;
    }

    private void OnDisable()
    {
        IsActive = false;
    }
}