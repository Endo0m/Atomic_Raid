using System;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Rigidbody rb;
    private float lifetime;
    private BulletPoolManager poolManager;

    public bool returnToPoolByOwnerScript;

    [HideInInspector, NonSerialized]
    public BulletChainLightning chainlightning;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        chainlightning = GetComponent<BulletChainLightning>();
        poolManager = FindObjectOfType<BulletPoolManager>();
    }

    public void Initialize(Vector3 direction, float speed, float lifetime)
    {
        this.lifetime = lifetime;
        rb.velocity = direction * speed;
        Invoke("ReturnToPool", lifetime);
    }

    public void ReturnToPool()
    {
        CancelInvoke("ReturnToPool");
        transform.localScale = Vector3.one;
        poolManager.ReturnBullet(this);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Handle collision
        if (!returnToPoolByOwnerScript)
            ReturnToPool();
    }

    public void Fire()
    {
        if (chainlightning != null)
            chainlightning.Fire();
    }
}