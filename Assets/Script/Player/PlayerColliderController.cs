using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerColliderController : MonoBehaviour
{
    public static PlayerColliderController instance = null;

    [SerializeField] public CapsuleCollider playerCollider;
    [SerializeField] private Animator playerAnimator;

    [Header("Standing Collider Settings")]
    [SerializeField] private float standingHeight = 2f;
    [SerializeField] private float standingRadius = 0.5f;
    [SerializeField] private float standingYOffset = 0f;

    [Header("Sliding Collider Settings")]
    [SerializeField] private float slidingHeight = 0.5f;
    [SerializeField] private float slidingRadius = 0.5f;
    [SerializeField] private float slidingYOffset = -0.75f;

    private int isSlidingHash;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        if (playerCollider == null)
            playerCollider = GetComponent<CapsuleCollider>();

        if (playerAnimator == null)
            playerAnimator = GetComponentInChildren<Animator>();

        isSlidingHash = Animator.StringToHash("IsSliding");
    }

    private void Update()
    {
        bool isSliding = playerAnimator.GetBool(isSlidingHash);
        UpdateColliderShape(isSliding);
    }

    private void UpdateColliderShape(bool isSliding)
    {
        if (isSliding)
        {
            playerCollider.height = slidingHeight;
            playerCollider.radius = slidingRadius;
            playerCollider.center = new Vector3(0, slidingYOffset, 0);
            playerCollider.direction = 2; // Установка направления капсулы по оси Z
        }
        else
        {
            playerCollider.height = standingHeight;
            playerCollider.radius = standingRadius;
            playerCollider.center = new Vector3(0, standingYOffset, 0);
            playerCollider.direction = 1; // Установка направления капсулы по оси Y
        }
    }
}
