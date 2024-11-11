using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HS_ProjectileMover : MonoBehaviour
{
    [SerializeField] protected float speed = 15f;
    [SerializeField] protected float hitOffset = 0f;
    [SerializeField] protected bool UseFirePointRotation;
    [SerializeField] protected Vector3 rotationOffset = new Vector3(0, 0, 0);
    [SerializeField] protected GameObject hit;
    [SerializeField] protected ParticleSystem hitPS;
    [SerializeField] protected GameObject flash;
    [SerializeField] protected Rigidbody rb;
    [SerializeField] protected Collider col;
    [SerializeField] protected Light lightSourse;
    [SerializeField] protected GameObject[] Detached;
    [SerializeField] protected ParticleSystem projectilePS;
    private bool startChecker = false;
    [SerializeField] protected bool notDestroy = false;

    private Bullet bullet;

    [SerializeField] protected float explosionRadius;
    [SerializeField] protected LayerMask attackMask;
    [SerializeField] protected int damage;
    [SerializeField] protected bool killBullets;

    [SerializeField] protected LayerMask bulletsLayer;
    [SerializeField] protected float maxDistance;
    
    private void Awake()
    {
        bullet = GetComponent<Bullet>();
    }

    private DamageDealer ddealer = null;
    
    protected virtual void Start()
    {
        if (ddealer == null)
            ddealer = GetComponent<DamageDealer>();

        ddealer.SetDamageAmount(damage);
        
        if (!startChecker)
        {
            /*lightSourse = GetComponent<Light>();
            rb = GetComponent<Rigidbody>();
            col = GetComponent<Collider>();
            if (hit != null)
                hitPS = hit.GetComponent<ParticleSystem>();*/
            if (flash != null)
            {
                flash.transform.parent = null;
            }
        }
        
        if (notDestroy)
            StartCoroutine(DisableTimer(5));
        else
            Destroy(gameObject, 5);
        
        startChecker = true;
    }

    protected virtual IEnumerator DisableTimer(float time)
    {
        yield return new WaitForSeconds(time);
        
        if(gameObject.activeSelf)
            gameObject.SetActive(false);
        
        if (bullet != null)
            bullet.ReturnToPool();
        
        yield break;
    }

    protected virtual void OnEnable()
    {
        if (startChecker)
        {
            if (flash != null)
            {
                flash.transform.parent = null;
            }
            if (lightSourse != null)
                lightSourse.enabled = true;
            col.enabled = true;
            rb.constraints = RigidbodyConstraints.None;
        }
    }

    private void OnDisable()
    {
        if (flash != null)
        {
            flash.transform.parent = transform.GetChild(0);
            flash.transform.localPosition = Vector3.zero;
        }
    }

    private void FixedUpdate()
    {
        if (col.enabled)
        {
            if (Vector3.Distance(transform.position, PlayerShoot.instance.WeaponTransform.position) >= maxDistance)
            {
                Vector3 direction = (PlayerShoot.instance.WeaponTransform.position - transform.position).normalized;
                Collide(transform.position, Quaternion.LookRotation(direction));
            }

            if (killBullets)
            {
                Collider[] coll = Physics.OverlapSphere(transform.position, explosionRadius, bulletsLayer);
                foreach (Collider coll_ in coll)
                {
                    Bullet bullet = coll_.GetComponent<Bullet>();
                    if (bullet != null)
                        bullet.ReturnToPool();
                }
            }
        }
        //}
    ///*protected virtual void FixedUpdate()
    //{
        //if (speed != 0)
        //{
        //    rb.velocity = transform.forward * speed;      
        //}
    }//*/

    private void Collide(Vector3 pos, Quaternion rot)
    {
        //Lock all axes movement and rotation
        rb.constraints = RigidbodyConstraints.FreezeAll;
        //speed = 0;
        if (lightSourse != null)
            lightSourse.enabled = false;
        col.enabled = false;
        projectilePS.Stop();
        projectilePS.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        //Spawn hit effect on collision
        if (hit != null)
        {
            hit.transform.rotation = rot;
            hit.transform.position = pos;
            if (UseFirePointRotation) { hit.transform.rotation = gameObject.transform.rotation * Quaternion.Euler(0, 180f, 0); }
            else if (rotationOffset != Vector3.zero) { hit.transform.rotation = Quaternion.Euler(rotationOffset); }
            //else { hit.transform.LookAt(pos); }
            hitPS.Play();
        }

        bool enemies = false;
        Collider[] colliders = Physics.OverlapSphere(pos, explosionRadius, attackMask);
        foreach (Collider collider in colliders)
            //if (collider != collision.collider)
            {
                IDamageable damagable = collider.GetComponent<IDamageable>();
                if (damagable != null)
                {
                    damagable.TakeDamage(damage);
                    enemies = true;
                }
            }

        if (!enemies)
        {
            Collider[] colliders_ = Physics.OverlapSphere(pos, explosionRadius);
            if (colliders_.Length > 0)
                if (hit != null)
                    hit.transform.parent = colliders_[0].transform;
        }

        //Removing trail from the projectile on cillision enter or smooth removing. Detached elements must have "AutoDestroying script"
        foreach (var detachedPrefab in Detached)
        {
            if (detachedPrefab != null)
            {
                ParticleSystem detachedPS = detachedPrefab.GetComponent<ParticleSystem>();
                detachedPS.Stop();
            }
        }
        
        if (notDestroy)
            StartCoroutine(DisableTimer(hitPS.main.duration));
        else
        {
            if (hitPS != null)
            {
                Destroy(gameObject, hitPS.main.duration);
            }
            else
                Destroy(gameObject, 1);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Vector3 direction = (PlayerShoot.instance.WeaponTransform.position - transform.position).normalized;
        Collide(transform.position, Quaternion.LookRotation(direction));
    }

    private void OnCollisionEnter(Collision collision)
    {
        Vector3 direction = (PlayerShoot.instance.WeaponTransform.position - transform.position).normalized;
        Collide(transform.position, Quaternion.LookRotation(direction));
    }

    //https ://docs.unity3d.com/ScriptReference/Rigidbody.OnCollisionEnter.html
    /*protected virtual void OnCollisionEnter(Collision collision)
    {
        ContactPoint contact = collision.contacts[0];
        Quaternion rot = Quaternion.FromToRotation(Vector3.up, contact.normal);
        Vector3 pos = contact.point + contact.normal * hitOffset;

        Collide(pos, rot);
    }*/
}