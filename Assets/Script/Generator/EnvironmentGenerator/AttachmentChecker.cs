using UnityEngine;

public class AttachmentChecker : MonoBehaviour
{
    private Transform originalParent;
    private float detachTime;

    private void Start()
    {
        originalParent = transform.parent;
    }

    private void Update()
    {
        if (transform.parent != originalParent)
        {
            if (detachTime == 0)
            {
                detachTime = Time.time;
            }
            else if (Time.time - detachTime > 5f)
            {
                Destroy(gameObject);
            }
        }
        else
        {
            detachTime = 0;
        }
    }
}