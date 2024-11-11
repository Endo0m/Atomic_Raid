using UnityEngine;

public class FirstFieldDetector : MonoBehaviour
{
    private const string FIRST_FIELD_DETECTED_KEY = "FirstFieldDetected";

    [SerializeField] private float detectionRadius = 10f;
    [SerializeField] private LayerMask fieldLayer;
    [SerializeField] private string fieldTag = "Field";
    [SerializeField] private string helpText = "Это первое поле: постройте здесь башню.";
    [SerializeField] private AudioClip helpSound;
    private bool isFirstFieldDetected = false;


    private void Update()
    {
        if (!isFirstFieldDetected && !PlayerPrefs.HasKey(FIRST_FIELD_DETECTED_KEY))
        {
            Collider[] hitColliders = new Collider[10];
            int numColliders = Physics.OverlapSphereNonAlloc(transform.position, detectionRadius, hitColliders, fieldLayer, QueryTriggerInteraction.Collide);

            for (int i = 0; i < numColliders; i++)
            {
                if (hitColliders[i].CompareTag(fieldTag))
                {
                    Debug.Log("Found matching field!");
                    isFirstFieldDetected = true;
                    PlayerPrefs.SetInt(FIRST_FIELD_DETECTED_KEY, 1);
                    PlayerPrefs.Save();
                    break;
                }
            }
        }
    }
    public void ResetFirstFieldDetected()
    {
        isFirstFieldDetected = false;
        PlayerPrefs.DeleteKey(FIRST_FIELD_DETECTED_KEY);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}