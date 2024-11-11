using UnityEngine;
using System.Collections;

public class DelayedPositionSetter : MonoBehaviour
{
    [SerializeField] private float delay = 1f;
    [SerializeField] private bool resetRotation = false;

    private void Start()
    {
        StartCoroutine(SetPositionWithDelay());
    }

    private IEnumerator SetPositionWithDelay()
    {
        Vector3 initialLocalPosition = transform.localPosition;
        Vector3 initialWorldPosition = transform.position;
        Quaternion initialRotation = transform.rotation;

        Debug.Log($"Initial - Local: {initialLocalPosition}, World: {initialWorldPosition}, Rotation: {initialRotation.eulerAngles}");

        yield return new WaitForSeconds(delay);

        // ��������� ������� �������
        Vector3 worldPosition = transform.position;

        // ������������� ��������� ������� � (0, 0, 0)
        transform.localPosition = Vector3.zero;

        // ���� ����� �������� �������
        if (resetRotation)
        {
            transform.localRotation = Quaternion.identity;
        }

        // ��������������� ������� �������
        transform.position = worldPosition;

        yield return null; // ���� ���� ���� ��� ���������� �������������

        Vector3 finalLocalPosition = transform.localPosition;
        Vector3 finalWorldPosition = transform.position;
        Quaternion finalRotation = transform.rotation;

        Debug.Log($"After setting - Local: {finalLocalPosition}, World: {finalWorldPosition}, Rotation: {finalRotation.eulerAngles}");

        if (finalLocalPosition != Vector3.zero)
        {
            Debug.LogWarning($"LocalPosition is not exactly zero. Actual: {finalLocalPosition}");
        }

        if (initialWorldPosition != finalWorldPosition)
        {
            Debug.LogWarning($"World position changed. Initial: {initialWorldPosition}, Final: {finalWorldPosition}");
        }

        CheckForPositionAffectingComponents();
    }

    private void CheckForPositionAffectingComponents()
    {
        if (GetComponent<Rigidbody>() != null)
        {
            Debug.LogWarning("Object has a Rigidbody, which might affect its position.");
        }

        if (GetComponent<Collider>() != null)
        {
            Debug.LogWarning("Object has a Collider, which might affect its position if physics are involved.");
        }

        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour script in scripts)
        {
            if (script != this)
            {
                Debug.LogWarning($"Object has another script: {script.GetType().Name}, which might affect its position.");
            }
        }
    }

    public void SetDelay(float newDelay)
    {
        delay = newDelay;
    }
}