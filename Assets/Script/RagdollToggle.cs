using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollToggle : MonoBehaviour
{
    private Animator animator;
    private Rigidbody[] allRigidbodies;

    void Start()
    {
        animator = GetComponent<Animator>();
        allRigidbodies = GetComponentsInChildren<Rigidbody>();
        SetRagdollState(false);
    }

    void SetRagdollState(bool state)
    {
        animator.enabled = !state;
        foreach (Rigidbody rb in allRigidbodies)
        {
            rb.isKinematic = !state;
        }
    }

    public void ActivateRagdoll()
    {
        SetRagdollState(true);
    }

    public void DeactivateRagdoll()
    {
        SetRagdollState(false);
    }
}