using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//using MyBox;
namespace PilotoStudio
{
    [ExecuteAlways]
    public class BeamEmitter : MonoBehaviour
    {
        //  [Separator("Beam Base Settings")]
        [Space]
        [SerializeField]
        private List<LineRenderer> beams = new List<LineRenderer>();
        [Space]
        [SerializeField]
        private List<ParticleSystem> beamSystems = new List<ParticleSystem>();
        [SerializeField]
        [Space]
        private float beamLifetime;
        [SerializeField]
        private float beamFormationTime;

        //  [Separator("Target Options")]
        [SerializeField]
        public Transform beamTarget;
        [SerializeField]
        private GameObject beamTargetHitFX;
        [SerializeField]
        private List<float> desiredWidth = new List<float>();

        [SerializeField] private float MaxDistance;
        [SerializeField] private int damageOverTime;
        [SerializeField] private float damageTime;
        [SerializeField] private bool isBoss;
        [SerializeField] private float shootBossRadius;
        [SerializeField] private LayerMask attackBossLayers;

        [SerializeField]
        private List<UnityEngine.ParticleSystem.MinMaxCurve> defaultDensity = new List<UnityEngine.ParticleSystem.MinMaxCurve>();

        Vector3 shootBox;
        private void Awake()
        {
            shootBox = new Vector3(shootBossRadius / 2, shootBossRadius / 2, shootBossRadius / 2);
            //shootBox = new Vector3(shootBossRadius, shootBossRadius, shootBossRadius);
        }

        #region Getting Relevant Variables from hierarchy.

        //  [ButtonMethod]
        private void AssignChildBeamsToArray()
        {
            GetChildLineRenderers();
            GetChildBeamEmitters();
            CacheParticleDensity();
        }

        //get all child line renderers and feed them on beams field.
        private void GetChildLineRenderers()
        {

            beams.Clear();
            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).TryGetComponent(out LineRenderer _lineRenderer))
                {
                    beams.Add(_lineRenderer);
                }
            }


        }
        //get all particle systems with edge shape and feed them on beam systems field.
        private void GetChildBeamEmitters()
        {
            beamSystems.Clear();
            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).TryGetComponent(out ParticleSystem _ps))
                {
                    // need to check if the ps shape is of type edge, since they are the only ones that matter to the beam emitter.
                    var sh = _ps.shape;
                    if (sh.shapeType == ParticleSystemShapeType.SingleSidedEdge)
                    {
                        beamSystems.Add(_ps);
                    }
                }
            }
        }

        #endregion

     //   [ButtonMethod]
        private void AssignBeamThickness()
        {
            desiredWidth.Clear();
            for (int i = 0; i < beams.Count; i++)
            {
                desiredWidth.Add(beams[i].widthMultiplier);
            }



        }


        private void OnEnable()
        {
            // CacheParticleDensity();
            PlayBeam();
            damages.Clear();
        }

        private void OnDisable()
        {
            damages.Clear();
        }


        private IEnumerator BeamStart()
        {
            float elapsedTime = 0f;

            while (elapsedTime <= 1)
            {

                for (int i = 0; i < beams.Count; i++)
                {

                    beams[i].widthMultiplier = Mathf.Lerp(0, desiredWidth[i], elapsedTime / 1);
                    elapsedTime += Time.deltaTime;
                }
                yield return null;
            }
            if (elapsedTime > 1)
            {
                for (int i = 0; i < beams.Count; i++)
                {
                    beams[i].widthMultiplier = desiredWidth[i];
                }
            }

        }

        private void CacheParticleDensity()
        {
            defaultDensity.Clear();

            //Go through every particles and save their spawn rate
            for (int i = 0; i < beamSystems.Count; i++)
            {
                defaultDensity.Add(beamSystems[i].emission.rateOverTime.constant);
            }
        }

        private void UpdateParticleDensity()
        {
            float distance = Vector3.Distance(this.transform.position, beamTarget.position);
            distance -= 5f;
            if (distance > 0)
            {
                float distanceMultiplier = 1 + (distance / 5);
                for (int i = 0; i < beamSystems.Count; i++)
                {
                    var emission = beamSystems[i].emission;
                    emission.rateOverTime = defaultDensity[i].constant * distanceMultiplier;
                }
            }
            else
            {
                for (int i = 0; i < beamSystems.Count; i++)
                {
                    var emission = beamSystems[i].emission;
                    emission.rateOverTime = defaultDensity[i].constant;
                }
            }



        }

        private void UpdateImpactFX()
        {
            beamTargetHitFX.transform.position = beamTarget.position;
            beamTargetHitFX.transform.LookAt(this.transform.position);

        }

        //[ButtonMethod]
        private void PreviewBeam()
        {
            PlayBeam();
            this.GetComponent<ParticleSystem>().Stop(true);
            this.GetComponent<ParticleSystem>().Play(true);
        }
        // Maybe give playbeam on awake options like particle handler?
       // [ButtonMethod]
        public void PlayBeam()
        {
            StopAllCoroutines();
            PlayEdgeSystems();
            PlayLineRenderers();

            // If the beam has no lifetime, it is a continuous beam, therefore we keep calling beam start normally.
            if (beamLifetime == 0)
            {
                StartCoroutine(nameof(BeamStart));
            }
            else
            {
                StartCoroutine(nameof(BeamPlayComplete));
            }
         
        }


        private IEnumerator BeamPlayComplete()
        {
            float elapsedTime = 0f;

            while (elapsedTime <= beamFormationTime)
            {

                for (int i = 0; i < beams.Count; i++)
                {

                    beams[i].widthMultiplier = Mathf.Lerp(0, desiredWidth[i], elapsedTime / beamFormationTime);
                    elapsedTime += Time.deltaTime;
                }
                yield return null;
            }
            if (elapsedTime > beamFormationTime)
            {
                for (int i = 0; i < beams.Count; i++)
                {
                    beams[i].widthMultiplier = desiredWidth[i];
                }
            }

            yield return new WaitForSeconds(beamLifetime);

            float dissipationTime = 0f;

            while (dissipationTime <= beamFormationTime)
            {

                for (int i = 0; i < beams.Count; i++)
                {

                    beams[i].widthMultiplier = Mathf.Lerp(desiredWidth[i],0 , dissipationTime / beamFormationTime);
                    dissipationTime += Time.deltaTime;
                }
                yield return null;
            }
            if (dissipationTime > beamFormationTime)
            {
                for (int i = 0; i < beams.Count; i++)
                {
                    beams[i].widthMultiplier = 0;
                }
            }
        }

        private void StartLineRenderers()
        {
            foreach (LineRenderer _line in beams)
            {
                _line.widthMultiplier = 1.0f;
            }
        }

        private void PlayLineRenderers()
        {
            foreach (LineRenderer _line in beams)
            {
                _line.useWorldSpace = true; // Needed.
                _line.SetPosition(1, _line.transform.position);
                _line.SetPosition(0, beamTarget.position);
            }
        }

        private void PlayEdgeSystems()
        {
            foreach (ParticleSystem _ps in beamSystems)
            {
                // Make particle look toward target.

                Quaternion _lookRotation = Quaternion.LookRotation(beamTarget.position - _ps.transform.position).normalized;
                _ps.gameObject.transform.rotation = _lookRotation;

                // Make shape lenght equal to distance between particle's start and end point.
                var sh = _ps.shape;
                sh.rotation = new Vector3(0, 90, 0); // We do this to allign the beam with the forward direction.
                float beamLenght = Vector3.Distance(beamTarget.position, _ps.transform.position) / 2; // Divide by two since it increases on negative and positive axis
                sh.radius = beamLenght;
                // Increase offset on the Z shape position to set the pivot at start point.
                sh.position = new Vector3(0, 0, beamLenght);
            }
        }

        private Dictionary<GameObject, float> damages = new Dictionary<GameObject, float>();

        private void Update()
        {
            RaycastHit hit;
            if (isBoss)
            {
                Collider[] colliders = Physics.OverlapSphere(beamTarget.position, shootBossRadius, attackBossLayers);
                if (colliders.Length > 0)
                {
                    foreach (Collider collider in colliders)
                    {
                        float lastTime = 0;
                        if (damages.TryGetValue(collider.gameObject, out lastTime))
                        {
                            if (Time.time >= lastTime)
                            {
                                damages[collider.gameObject] = Time.time + damageTime;
                                IDamageable damagable = collider.GetComponent<IDamageable>();
                                if (damagable != null)
                                    damagable.TakeDamage(damageOverTime);
                            }
                        }
                        else
                        {
                            damages.Add(collider.gameObject, Time.time + damageTime);
                            IDamageable damagable = collider.GetComponent<IDamageable>();
                            if (damagable != null)
                                damagable.TakeDamage(damageOverTime);
                        }
                    }
                }

                colliders = Physics.OverlapSphere(transform.position, shootBossRadius, attackBossLayers);
                if (colliders.Length > 0)
                {
                    foreach (Collider collider in colliders)
                    {
                        float lastTime = 0;
                        if (damages.TryGetValue(collider.gameObject, out lastTime))
                        {
                            if (Time.time >= lastTime)
                            {
                                damages[collider.gameObject] = Time.time + damageTime;
                                IDamageable damagable = collider.GetComponent<IDamageable>();
                                if (damagable != null)
                                    damagable.TakeDamage(damageOverTime);
                            }
                        }
                        else
                        {
                            damages.Add(collider.gameObject, Time.time + damageTime);
                            IDamageable damagable = collider.GetComponent<IDamageable>();
                            if (damagable != null)
                                damagable.TakeDamage(damageOverTime);
                        }
                    }
                }

                if (Physics.BoxCast(transform.position, shootBox, (beamTarget.position - transform.position).normalized, out hit, Quaternion.identity, Vector3.Distance(beamTarget.position, transform.position), attackBossLayers))
                {
                    float lastTime = 0;
                    if (damages.TryGetValue(hit.collider.gameObject, out lastTime))
                    {
                        if (Time.time >= lastTime)
                        {
                            damages[hit.collider.gameObject] = Time.time + damageTime;
                            IDamageable damagable = hit.collider.GetComponent<IDamageable>();
                            if (damagable != null)
                                damagable.TakeDamage(damageOverTime);
                        }
                    }
                    else
                    {
                        damages.Add(hit.collider.gameObject, Time.time + damageTime);
                        IDamageable damagable = hit.collider.GetComponent<IDamageable>();
                        if (damagable != null)
                            damagable.TakeDamage(damageOverTime);
                    }
                }
            }
            else
            {
                Ray ray = PlayerShoot.instance.MainCamera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit, MaxDistance, PlayerShoot.instance.ShootableLayers))
                {
                    beamTarget.position = hit.point;
                    if (hit.collider.tag == "Enemy" || hit.collider.tag == "Boss")
                    {
                        IDamageable damageable = hit.collider.GetComponent<IDamageable>();
                        if (damageable != null)
                        {
                            float lastTime = 0;
                            if (damages.TryGetValue(hit.collider.gameObject, out lastTime))
                            {
                                if (Time.time >= lastTime)
                                {
                                    damageable.TakeDamage(damageOverTime);
                                    damages[hit.collider.gameObject] = Time.time + damageTime;
                                }
                            }
                            else
                            {
                                damageable.TakeDamage(damageOverTime);
                                damages.Add(hit.collider.gameObject, Time.time + damageTime);
                            }
                        }
                    }
                }
                else
                {
                    beamTarget.position = ray.GetPoint(MaxDistance);
                }
            }

            PlayEdgeSystems();
            PlayLineRenderers();
            UpdateParticleDensity();
            UpdateImpactFX();
        }




    }
}