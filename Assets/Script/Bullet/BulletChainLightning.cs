using System;
using System.Collections;
using System.Collections.Generic;
using DigitalRuby.ThunderAndLightning;
using UnityEngine;

public class BulletChainLightning : MonoBehaviour
{
    public int initialDamage, onlineDamage;
    public float shotRadius, chainRadius;
    public LayerMask attackMask;
    public float maxDistance;
    public float maxTime;
    public Bullet curBullet;
    public LightningBoltPrefabScript curLightning;
    public GameObject flash, hit;
    public ParticleSystem hitParticle;

    private List<Bullet> bullets = new List<Bullet>();
    private List<BulletChainLightning> chainlightnings = new List<BulletChainLightning>();
    private List<LightningBoltPrefabScript> lightnings = new List<LightningBoltPrefabScript>();
    private List<GameObject> objects = new List<GameObject>();
    private List<IDamageable> damagables = new List<IDamageable>();
    private List<Transform> transforms = new List<Transform>();
    private List<Vector3> transformsDelta = new List<Vector3>();
    
    public void Fire()
    {
        Vector3 direction = (PlayerShoot.instance.GetAimPoint() - PlayerShoot.instance.WeaponTransform.position).normalized;
        RaycastHit[] hits = Physics.SphereCastAll(PlayerShoot.instance.MainCamera.ScreenPointToRay(Input.mousePosition), shotRadius, maxDistance, attackMask);

        if (hits.Length > 0)
        {
            int startDamage = initialDamage;
            
            IDamageable damagable = null;
            Transform hitTransform = null;
            RaycastHit hitCollider = new RaycastHit();
           /* foreach (RaycastHit hit in hits)
            {
                damagable = hit.collider.GetComponent<IDamageable>();
                if (damagable != null)
                {
                    hitCollider = hit;
                    //damagable.TakeDamage(startDamage);
                    damagables.Add(damagable);
                    startDamage--;
                    hitTransform = hit.collider.transform;
                    break;
                }
            }*/
           //Arthur
            foreach (RaycastHit hit in hits)
            {
                damagable = hit.collider.GetComponent<IDamageable>();
                if (damagable != null)
                {
                    hitCollider = hit;
                    // Проверяем, что объект еще не уничтожен перед добавлением
                    if (damagable != null && !objects.Contains(hit.collider.gameObject))
                    {
                        damagables.Add(damagable);
                        objects.Add(hit.collider.gameObject); // Добавляем объект для проверки на уникальность
                    }
                    startDamage--;
                    hitTransform = hit.collider.transform;
                    break;
                }
            }
            //Arthur
            if (damagable == null)
            {
                hitTransform = hits[0].collider.transform;
                hitCollider = hits[0];
            }

            if (curLightning.Camera == null)
                curLightning.Camera = PlayerShoot.instance.MainCamera;

            Vector3 centerPoint = Vector3.zero;
            if (hitCollider.collider is MeshCollider) centerPoint = (hitCollider.collider as MeshCollider).bounds.center;
            else if (hitCollider.collider is BoxCollider) centerPoint = (hitCollider.collider as BoxCollider).center;
            else if (hitCollider.collider is SphereCollider) centerPoint = (hitCollider.collider as SphereCollider).center;
            else if (hitCollider.collider is CapsuleCollider) centerPoint = (hitCollider.collider as CapsuleCollider).center;
            
            if (centerPoint == Vector3.zero) transformsDelta.Add(Vector3.zero);
            else
            {
                centerPoint += hitTransform.position;
                hits = Physics.RaycastAll(PlayerShoot.instance.WeaponTransform.position, centerPoint - PlayerShoot.instance.WeaponTransform.position, Vector3.Distance(centerPoint, PlayerShoot.instance.WeaponTransform.position), attackMask);
                
                bool found = false;
                foreach (RaycastHit hit in hits)
                    if (hit.collider == hitCollider.collider)
                    {
                        found = true;
                        transformsDelta.Add(hit.point - hitTransform.position);
                        break;
                    }
                
                if (!found)
                    transformsDelta.Add(Vector3.zero);
            }

            //Arthur
            if (hitTransform == null) // Проверка на null
            {
                Debug.LogWarning("Hit transform is missing.");
                return;
            }
            //Arthur


            //curLightning.Destination.transform.parent = hitTransform;
            //curLightning.Destination.transform.localPosition = Vector3.zero;
            curLightning.Destination.transform.position = hitTransform.position + transformsDelta[transformsDelta.Count - 1];
            transforms.Add(hitTransform);

            curLightning.Source.transform.parent = PlayerShoot.instance.WeaponTransform;
            curLightning.Source.transform.localPosition = Vector3.zero;

            bullets.Add(curBullet);
            lightnings.Add(curLightning);
            chainlightnings.Add(this);

            //flash.transform.parent = PlayerShoot.instance.WeaponTransform;
            //flash.transform.localPosition = Vector3.zero;
            flash.transform.rotation = Quaternion.LookRotation(direction);
            flash.SetActive(true);

            //hit.transform.parent = hitTransform;
            //hit.transform.localPosition = Vector3.zero;
            //hit.transform.position = hitTransform.position;
            hit.SetActive(true);

            if (damagable != null)
            {
                objects.Add(hitTransform.gameObject);

                while (startDamage > 0)
                {
                    Collider[] around = Physics.OverlapSphere(hitTransform.position, chainRadius, attackMask);
                    bool found = false;
                    foreach (Collider collider in around)
                        if (objects.Find(r => r == collider.gameObject) == null)
                        {
                            damagable = collider.GetComponent<IDamageable>();
                            if (damagable != null)
                            {
                                found = true;

                                Bullet bullet = BulletPoolManager.instance.GetChainlightningBullet();
                                BulletChainLightning chainlightning = bullet.chainlightning;                                  //.GetComponent<BulletChainLightning>();
                                LightningBoltPrefabScript lightning = chainlightning.curLightning;                            //bullet.GetComponent<LightningBoltPrefabScript>();

                                if (lightning.Camera == null)
                                    lightning.Camera = PlayerShoot.instance.MainCamera;
                                
                                //lightning.Source.transform.parent = hitTransform;
                                //lightning.Source.transform.localPosition = Vector3.zero;
                                lightning.Source.transform.position = hitTransform.position + transformsDelta[transformsDelta.Count - 1];

                                Vector3 prevPosition = hitTransform.position + transformsDelta[transformsDelta.Count - 1];
                                
                                hitTransform = collider.transform;
                                transforms.Add(hitTransform);
                                
                                centerPoint = Vector3.zero;
                                if (collider is MeshCollider) centerPoint = (collider as MeshCollider).bounds.center;
                                else if (collider is BoxCollider) centerPoint = (collider as BoxCollider).center;
                                else if (collider is SphereCollider) centerPoint = (collider as SphereCollider).center;
                                else if (collider is CapsuleCollider) centerPoint = (collider as CapsuleCollider).center;
            
                                if (centerPoint == Vector3.zero) transformsDelta.Add(Vector3.zero);
                                else
                                {
                                    centerPoint += hitTransform.position;
                                    hits = Physics.RaycastAll(prevPosition, centerPoint - prevPosition, Vector3.Distance(centerPoint, prevPosition), attackMask);
                
                                    bool found_ = false;
                                    foreach (RaycastHit hit in hits)
                                        if (hit.collider == collider)
                                        {
                                            found_ = true;
                                            transformsDelta.Add(hit.point - hitTransform.position);
                                            break;
                                        }
                
                                    if (!found_)
                                        transformsDelta.Add(Vector3.zero);
                                }

                                //lightning.Destination.transform.parent = hitTransform;
                                //lightning.Destination.transform.localPosition = Vector3.zero;
                                lightning.Destination.transform.position = hitTransform.position + transformsDelta[transformsDelta.Count - 1];

                                bullets.Add(bullet);
                                lightnings.Add(lightning);
                                chainlightnings.Add(chainlightning);

                                //hit.transform.parent = hitTransform;
                                //hit.transform.localPosition = Vector3.zero;
                                //chainlightning.hit.transform.position = hitTransform.position;
                                chainlightning.hit.SetActive(true);

                                objects.Add(collider.gameObject);
                                
                                //damagable.TakeDamage(startDamage);
                                damagables.Add(damagable);
                                break;
                            }
                        }

                    if (!found) break;
                    startDamage--;
                }
            }
        }
        else
        {
            Vector3 hitpoint = PlayerShoot.instance.WeaponTransform.position + direction * maxDistance;

            Bullet bullet = curBullet;
            if (bullet != null)
            {
                LightningBoltPrefabScript lightning = curLightning;
                if (lightning != null)
                {
                    if (lightning.Camera == null)
                        lightning.Camera = PlayerShoot.instance.MainCamera;

                    lightning.Source.transform.parent = PlayerShoot.instance.WeaponTransform;
                    lightning.Source.transform.localPosition = Vector3.zero;

                    lightning.Destination.transform.position = hitpoint;
                    
                    bullets.Add(bullet);
                    lightnings.Add(lightning);
                    chainlightnings.Add(this);

                    //flash.transform.parent = PlayerShoot.instance.WeaponTransform;
                    //flash.transform.localPosition = Vector3.zero;
                    flash.transform.rotation = Quaternion.LookRotation(direction);
                    flash.SetActive(true);

                    //hit.transform.position = hitpoint;
                    hit.SetActive(true);
                }
            }
        }

        if (!isStarted)
            StartCoroutine(Lightnings());
    }

    private bool isStarted = false;
    IEnumerator Lightnings()
    {
        if (isStarted) yield break;

        float endTime = Time.time + maxTime;
        isStarted = true;
        float checkTime = Time.time + 0.1f;
        while (lightnings.Count > 0 && Time.time < endTime)
        {
            bool checkedPosition = false;
            if (Time.time >= checkTime)
            {
                checkTime = Time.time + 0.1f;
                foreach (LightningBoltPrefabScript scr in lightnings)
                {
                    if (scr._threadState != null)
                    {
                        checkedPosition = true;
                        for (int i = 0; i < transforms.Count; i++)
                        {
                            if (transforms[i] == null)
                                continue;

                            if (i > 0)
                                lightnings[i].Source.transform.position = lightnings[i - 1].Destination.transform.position + transformsDelta[i - 1];
                            lightnings[i].Destination.transform.position = transforms[i].position + transformsDelta[i];
                            //chainlightnings[i].hit.transform.position = transforms[i].position;
                        }

                        scr.Trigger();
                    }

                    RaycastHit[] hits = Physics.RaycastAll(scr.Source.transform.position, (scr.Destination.transform.position - scr.Source.transform.position).normalized,
                        Vector3.Distance(scr.Source.transform.position, scr.Destination.transform.position), attackMask);
                    foreach (RaycastHit hit in hits)
                        if (objects.Find(r => r == hit.collider.gameObject) == null)
                        {
                            objects.Add(hit.collider.gameObject);

                            IDamageable damagable = hit.collider.GetComponent<IDamageable>();
                            if (damagable != null)
                                damagable.TakeDamage(onlineDamage);
                        }
                }
            }

            if (!checkedPosition)
                for (int i = 0; i < transforms.Count; i++)
                {
                    if (transforms[i] == null)
                        continue;

                    if (i > 0)
                        lightnings[i].Source.transform.position = lightnings[i - 1].Destination.transform.position;
                    lightnings[i].Destination.transform.position = transforms[i].position;
                    //chainlightnings[i].hit.transform.position = transforms[i].position;
                }

            yield return null;
        }
        isStarted = false;

        curLightning.Source.transform.parent = transform;
        
        foreach (BulletChainLightning chainLightning in chainlightnings)
        {
            //flash.transform.parent = transform;
            chainLightning.flash.SetActive(false);

            //hit.transform.parent = transform;
            chainLightning.hit.SetActive(false);
        }

        foreach (BulletChainLightning chainLightning in chainlightnings)
        {
            chainLightning.hitParticle.Stop();
            
            ParticleSystem.Particle[] m_Particles = new ParticleSystem.Particle[chainLightning.hitParticle.main.maxParticles];
            int numParticlesAlive = chainLightning.hitParticle.GetParticles(m_Particles);
            for (int i = 0; i < numParticlesAlive; i++)
                m_Particles[i].remainingLifetime = 0;
            chainLightning.hitParticle.SetParticles(m_Particles, numParticlesAlive);
        }

        /*foreach (LightningBoltPrefabScript scr in lightnings)
        {
            scr.Source.transform.parent = scr.transform;
            scr.Destination.transform.parent = scr.transform;
        }*/
        
        int startDamage = initialDamage;
        foreach (IDamageable damagable in damagables)
        {
            if (damagable != null && startDamage > 0)
            {
                damagable.TakeDamage(startDamage);
                startDamage--;
            }
        }

        yield return new WaitForSeconds(1);

        for (int i = bullets.Count - 1; i >= 0; i--)
            bullets[i].ReturnToPool();
    }

    private void OnDisable()
    {
        bullets.Clear();
        lightnings.Clear();
        objects.Clear();
        chainlightnings.Clear();
        damagables.Clear();
        transforms.Clear();
        transformsDelta.Clear();
    }
}
