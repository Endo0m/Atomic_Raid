using UnityEngine;
using System.Collections;

public class BossFragmentation : MonoBehaviour
{
    [SerializeField] private Rigidbody[] bodyParts;
    [SerializeField] private float explosionForce = 500f;
    [SerializeField] private float explosionRadius = 5f;
    [SerializeField] private float upwardsModifier = 3f;

    public IEnumerator FragmentBody()
    {
        yield return new WaitForSeconds(0.1f); // Small delay for visual effect

        Vector3 explosionPos = transform.position;
        foreach (Rigidbody rb in bodyParts)
        {
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
                Vector3 randomDir = Random.insideUnitSphere;
                rb.AddForce(randomDir * explosionForce, ForceMode.Impulse);
                rb.AddExplosionForce(explosionForce, explosionPos, explosionRadius, upwardsModifier);
            }
        }

        // Central part falls down
        if (bodyParts.Length > 0 && bodyParts[0] != null)
        {
            bodyParts[0].AddForce(Vector3.down * explosionForce);
        }

        Destroy(gameObject, 5f); // Destroy the boss object after 5 seconds
    }
}