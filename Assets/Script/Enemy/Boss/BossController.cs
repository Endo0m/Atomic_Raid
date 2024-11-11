using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Audio;
using PilotoStudio;

public class BossController : MonoBehaviour
{
    [SerializeField] private Transform[] shootPoints;
    [SerializeField] private Transform enemySpawnPoint;
    [SerializeField] private float bulletSpeed = 10f;
    [SerializeField] private float attackDuration = 5f;
    [SerializeField] private float weaponChangeDuration = 3.25f;
    [SerializeField] private float fastShootInterval = 0.5f;
    [SerializeField] private float normalShootInterval = 1.25f;
    [SerializeField] private float enemySpawnInterval = 2f;
    [SerializeField] private float shootRadius = 20f;
    [SerializeField] private GameObject[] bulletPrefabs;
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Animator animator;
    [SerializeField] private AudioClip attackSoundEffect;
    [SerializeField] private AudioClip weaponChangeSoundEffect;
    [SerializeField] private int scoreValue = 1000;
    [SerializeField] private int baseEnemyCount = 3;
    [SerializeField] private int additionalEnemiesAt50Percent = 2;
    [SerializeField] private int additionalEnemiesAt25Percent = 2;
    [Header("Время на спавн врагов")]
    [SerializeField] private float spawnEnemiesTime = 3f;       // сколько времени босс спавнит врагов
    [Header("Скорость движения лазера")]
    [SerializeField] private float laserMoveSpeed = 1f;
    [SerializeField] private string isSpawningAnimationParameter = "IsSpawning";
    [SerializeField] private AudioClip bossRoarSound;
    [SerializeField] private string bossAppearanceKey = "boss_1";
    [SerializeField] private string bossVulnerableKey = "boss_vulnerable_1";
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private AudioMixerGroup soundGroup;
    [SerializeField] private BeamEmitter beamEmitter;
    [SerializeField] private BeamEmitter[] stadia4Emitters;
    [SerializeField] private GameObject stadia4Projectile;
    [SerializeField] private Transform[] stadia5Projectile;
    [SerializeField] private Transform[] stadia5Explosive;
    [SerializeField] private float stadia5ExplosionRadius, stadia5BulletSecondsToTarget, stadia5SubBulletSpeed;
    [SerializeField] private int stadia5ExplosiveDamage;
    private AudioSource bossAudioSource;
    private WeaponUpgradeManager weaponUpgradeManager;
    private AudioSource audioSource;
    private bool isVulnerable = false;
    private BulletPoolManager bulletPoolManager;
    private int currentModeIndex;
    private bool isChangingWeapon = false;
    private Transform playerTransform;
    private BossLife bossLife;
    private LookAtPlayerBehavior lookAtPlayerBehavior;
    private int currentAdditionalEnemies = 0;
    //private bool hasReachedPlayer = false;

    public Transform[] ShootPoints => shootPoints;

    private bool isBossDefeated = false;

    private void Start()
    {
        //bulletPoolManager = FindObjectOfType<BulletPoolManager>();
        bulletPoolManager = BulletPoolManager.instance;
        bossLife = GetComponent<BossLife>();
        FindPlayer();
       // EventManager.Instance.OnBossApproaching();
        lookAtPlayerBehavior = GetComponent<LookAtPlayerBehavior>();
        if (lookAtPlayerBehavior == null)
        {
            lookAtPlayerBehavior = gameObject.AddComponent<LookAtPlayerBehavior>();
        }
        weaponUpgradeManager = FindObjectOfType<WeaponUpgradeManager>();
        if (weaponUpgradeManager == null)
        {
            Debug.LogWarning("WeaponUpgradeManager not found in the scene.");
        }
        if (bossLife != null)
        {
            bossLife.OnBossDeath += HandleBossDeath;
        }
        Debug.Log("Boss initialized");
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        bossAudioSource = gameObject.AddComponent<AudioSource>();
        bossAudioSource.outputAudioMixerGroup = soundGroup;
        StartCoroutine(BossRoutine());
    }

    private void Update()
    {
        if (playerTransform != null && lookAtPlayerBehavior != null)
        {
            lookAtPlayerBehavior.LookAtPlayer(playerTransform);
        }

        UpdateAdditionalEnemies();

        /*if (!hasReachedPlayer && IsPlayerInRange())
        {
            hasReachedPlayer = true;
            StartCoroutine(BattleRoutine());
        }*/
    }
    
    private void UpdateAdditionalEnemies()
    {
        float healthPercentage = (float)bossLife.CurrentHealth / bossLife.MaxHealth * 100;
        if (healthPercentage <= 25 && currentAdditionalEnemies < additionalEnemiesAt25Percent + additionalEnemiesAt50Percent)
        {
            currentAdditionalEnemies = additionalEnemiesAt25Percent + additionalEnemiesAt50Percent;
        }
        else if (healthPercentage <= 50 && currentAdditionalEnemies < additionalEnemiesAt50Percent)
        {
            currentAdditionalEnemies = additionalEnemiesAt50Percent;
        }
    }

    private void FindPlayer()
    {
        /*GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
        }
        else
        {
            Debug.LogError("No object with 'Player' tag found in the scene.");
        }*/

        playerTransform = PlayerShoot.instance.transform;
    }

    private IEnumerator BossRoutine()
    {
        animator.SetBool("IsIdle", true);
        StartCoroutine(SetVulnerable());

        // Проигрываем рев босса
        if (bossRoarSound != null)
        {
            PlayBossSound(bossRoarSound);
            yield return new WaitForSeconds(bossRoarSound.length);
        }

        // Воспроизводим событие появления босса
        BossEventManager.Instance.PlayBossEvent(bossAppearanceKey);

        /*while (!hasReachedPlayer)
        {
            yield return null;
        }*/

        animator.SetBool("IsIdle", false);

        while (!IsPlayerInRange())
            yield return null;

        StartCoroutine(BattleRoutine());
    }

    IEnumerator SetVulnerable()
    {
        yield return new WaitForSeconds(1);

        isVulnerable = true;
        bossLife.SetInvulnerability(false);
    }

    private IEnumerator BattleRoutine()
    {
        while (true)
        {
            // Reset all animation parameters
            animator.SetBool("IsAttacking", false);
            animator.SetBool(isSpawningAnimationParameter, false);
            animator.SetBool("IsChangingWeapon", false);

            yield return StartCoroutine(AttackRoutine());

            isVulnerable = false;
            bossLife.SetInvulnerability(true);
            yield return StartCoroutine(ChangeWeapon());
        }
    }

    private IEnumerator AttackRoutine()
    {
        if (currentModeIndex % 2 != 0) // Spawning enemies (vulnerable)
        {
            animator.SetBool(isSpawningAnimationParameter, true);
            isVulnerable = true;
            bossLife.SetInvulnerability(false);

            // Воспроизводим событие уязвимости босса
            // BossEventManager.Instance.PlayBossEvent(bossVulnerableKey);

            yield return StartCoroutine(SpawnEnemiesRoutine());

            animator.SetBool(isSpawningAnimationParameter, false);
            //isVulnerable = false;
            //bossLife.SetInvulnerability(true);
        }
        else // Shooting (invulnerable) - for both first and second attack modes
        {
            isVulnerable = false;
            bossLife.SetInvulnerability(true);

            switch (currentModeIndex)
            {
                case 0:
                case 4:
                    {
                        animator.SetBool("IsAttacking", true);
                        float elapsedTime = 0f;
                        float interval = (currentModeIndex == 0 || currentModeIndex == 6) ? normalShootInterval : fastShootInterval;
                        while (elapsedTime < attackDuration)
                        {
                            for (int i = 0; i < shootPoints.Length; i++)
                            {
                                ShootBullet(i);
                            }

                            yield return new WaitForSeconds(interval);
                            elapsedTime += interval;
                        }
                        animator.SetBool("IsAttacking", false);
                    }
                    break;
                case 8:         // explosive bullet
                    {
                        animator.SetBool("IsAttacking", true);

                        Transform saveParent = stadia5Projectile[0].parent;

                        float interval = attackDuration * 2f / (float)stadia5Explosive.Length;
                        for (int i = 0; i < stadia5Projectile.Length; i++)
                        {
                            StartCoroutine(ShootExplosive(i, saveParent));
                            yield return new WaitForSeconds(interval);
                        }

                        animator.SetBool("IsAttacking", false);
                    }
                    break;
                case 6:         // 4 rotating lasers
                    {
                        animator.SetBool("IsAttacking", true);

                        Vector3 playerPoint = PlayerColliderController.instance.playerCollider.center + PlayerColliderController.instance.playerCollider.transform.position;
                        float cameraDistance = Vector3.Distance(PlayerShoot.instance.MainCamera.transform.position, playerPoint);
                        Vector3 targetPoint = PlayerShoot.instance.MainCamera.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height / 2, cameraDistance));
                        targetPoint.z = playerPoint.z;

                        Transform saveParent = stadia4Projectile.transform.parent;
                        stadia4Projectile.SetActive(true);
                        stadia4Projectile.transform.parent = null;

                        float endTime = Time.time + 2f;
                        Vector3 startPosition = stadia4Projectile.transform.position;
                        while (Time.time < endTime) {
                            stadia4Projectile.transform.position = Vector3.Lerp(startPosition, targetPoint, (2f - endTime + Time.time) / 2f);
                            yield return null;
                        }
                        stadia4Projectile.transform.position = targetPoint;     // target position

                        Vector3[] startPoints_ = GetStartPoints(cameraDistance);
                        List<int> startPoints = new List<int>();
                        List<float> currentTimes = new List<float>();
                        float[] startSpeed = GetLaserSpeeds();
                        List<Vector3[]> corners = new List<Vector3[]>();
                        for (int i = 0; i < 4; i++)
                            corners.Add(GetCornerPoints(i, cameraDistance));
                        List<int> startTargets = new List<int>();
                        for (int i = 0; i < 4; i++) {
                            if (IsPointOnLine(corners[i][0], corners[i][1], startPoints_[i]))
                            {
                                int index = Random.Range(0, 2);
                                startTargets.Add(index);
                                startPoints.Add(index == 0 ? 1 : 0);
                                currentTimes.Add(Vector3.Distance(index == 0 ? corners[i][1] : corners[i][0], startPoints_[i]) / startSpeed[i]);
                            }
                            else
                            {
                                int index = Random.Range(0, 2);
                                startTargets.Add(index + 1);
                                startPoints.Add(index == 0 ? 2 : 1);
                                currentTimes.Add(Vector3.Distance(index == 0 ? corners[i][2] : corners[i][1], startPoints_[i]) / startSpeed[i]);
                            }
                        }
                        List<float> followTimes = new List<float>();
                        for (int i = 0; i < 4; i++)
                            followTimes.Add(Vector3.Distance(startPoints_[i], corners[i][startTargets[i]]) / startSpeed[i]);

                        for (int i = 0; i < 4; i++)
                        {
                            stadia4Emitters[i].beamTarget.parent = null;
                            stadia4Emitters[i].beamTarget.position = startPoints_[i];
                        }                                                       // emitter set

                        float currentTime = Time.time;

                        float time = Time.time;
                        while (Time.time - currentTime < attackDuration)
                        {
                            yield return null;

                            float spentTime = Time.time - time;
                            time = Time.time;

                            for (int i = 0; i < 4; i++)
                                corners[i] = GetCornerPoints(i, cameraDistance);

                            for (int i = 0; i < 4; i++)
                            {
                                currentTimes[i] += spentTime;

                                float percent = currentTimes[i] / followTimes[i];
                                stadia4Emitters[i].beamTarget.position = Vector3.Lerp(corners[i][startPoints[i]], corners[i][startTargets[i]], percent);

                                if (percent >= 1f)
                                {
                                    currentTimes[i] = 0;
                                    Vector3 savePoint = corners[i][startPoints[i]];
                                    startPoints[i] = startTargets[i];
                                    startTargets[i] = corners[i][startTargets[i]] == corners[i][0] || corners[i][startTargets[i]] == corners[i][2] ? 1 : savePoint == corners[i][0] ? 2 : 0;
                                    followTimes[i] = Vector3.Distance(corners[i][startPoints[i]], corners[i][startTargets[i]]) / startSpeed[i];
                                }
                            }
                        }

                        stadia4Projectile.transform.parent = saveParent;
                        stadia4Projectile.transform.localPosition = Vector3.zero;
                        foreach (BeamEmitter emitter in stadia4Emitters)
                        {
                            emitter.beamTarget.parent = emitter.transform;
                            emitter.beamTarget.localPosition = Vector3.zero;
                        }
                        stadia4Projectile.SetActive(false);

                        animator.SetBool("IsAttacking", false);
                    }
                    break;
                case 2:         // Laser
                    {
                        animator.SetBool("IsAttacking", true);

                        float currentTime = Time.time;

                        beamEmitter.beamTarget.localPosition = Vector3.zero;
                        beamEmitter.gameObject.SetActive(true);

                        Vector3 targetPoint = PlayerColliderController.instance.playerCollider.center + PlayerColliderController.instance.playerCollider.transform.position;

                        yield return new WaitForSeconds(2);

                        beamEmitter.beamTarget.parent = null;
                        beamEmitter.beamTarget.position = targetPoint;

                        float time = Time.time;
                        while (Time.time - currentTime < attackDuration)
                        {
                            yield return null;

                            float spentTime = Time.time - time;
                            time = Time.time;

                            Vector3 direction = ((PlayerColliderController.instance.playerCollider.center + PlayerColliderController.instance.playerCollider.transform.position) - beamEmitter.beamTarget.position).normalized;
                            beamEmitter.beamTarget.position += direction * laserMoveSpeed * spentTime;
                        }

                        beamEmitter.beamTarget.parent = beamEmitter.transform;
                        beamEmitter.beamTarget.localPosition = Vector3.zero;
                        beamEmitter.gameObject.SetActive(false);

                        animator.SetBool("IsAttacking", false); 
                    }
                    break;
            }
        }

        currentModeIndex = (currentModeIndex + 1) % 10; // Cycle through 0, 1s, 2, 3s, 4, 5s, 6, 7s, 8, 9s - нечетные спавн
    }

    IEnumerator ShootExplosive(int index, Transform saveParent)
    {
        Vector3 playerPoint = PlayerColliderController.instance.playerCollider.center + PlayerColliderController.instance.playerCollider.transform.position;

        stadia5Projectile[index].gameObject.SetActive(true);
        Vector3 startPosition = stadia5Projectile[index].position;

        float endTime = Time.time + stadia5BulletSecondsToTarget;
        while (Time.time < endTime)
        {
            yield return null;
            stadia5Projectile[index].position = Vector3.Lerp(startPosition, playerPoint, (stadia5BulletSecondsToTarget - endTime + Time.time) / stadia5BulletSecondsToTarget);
        }
        stadia5Projectile[index].position = playerPoint;

        stadia5Projectile[index].parent = saveParent;
        stadia5Projectile[index].localPosition = Vector3.zero;
        stadia5Projectile[index].gameObject.SetActive(false);

        stadia5Explosive[index].gameObject.SetActive(true);
        stadia5Explosive[index].position = playerPoint;

        Collider[] colliders = Physics.OverlapSphere(playerPoint, stadia5ExplosionRadius, LayerMask.GetMask("Player"));
        foreach (Collider collider in colliders)
        {
            IDamageable damagable = collider.GetComponent<IDamageable>();
            if (damagable != null)
                damagable.TakeDamage(stadia5ExplosiveDamage);
        }

        // shoot bullets ---------------------------
        float cameraDistance = Vector3.Distance(PlayerShoot.instance.MainCamera.transform.position, playerPoint);
        Vector3[] targets = GetStartPoints(cameraDistance);

        //List<Bullet> bullets = new List<Bullet>();
        for (int i = 0; i < targets.Length; i++)
        {
            Bullet bullet = bulletPoolManager.GetBossBullet(2);
            if (bullet != null)
            {
                //bullets.Add(bullet);
                targets[i].z = PlayerShoot.instance.transform.position.z;
                
                bullet.transform.position = playerPoint;
                Vector3 direction = (targets[i] - playerPoint).normalized;
                bullet.Initialize(direction, stadia5SubBulletSpeed, 2.5f);
                //StartCoroutine(BulletTrajectory(bullet, targets[i], stadia5SubBulletSpeed, 7f));
                PlayBossSound(attackSoundEffect);
            }
            else
            {
                Debug.LogWarning($"Failed to get bullet of type {currentModeIndex} from pool.");
            }
        }

        yield return new WaitForSeconds(1);

        endTime = Time.time + 4;
        while (Time.time < endTime)
        {
            /*if (bullets.Count > 0)
                if (endTime - Time.time <= 2)
                {
                    foreach (Bullet bullet in bullets)
                        bullet.ReturnToPool();
                    bullets.Clear();
                }*/
            
            stadia5Explosive[index].Translate(Vector3.back * EnvironmentStaticGenerator.instance.moveSpeed * Time.deltaTime);
            yield return null;
        }
        // -----------------------------------------

        stadia5Explosive[index].parent = saveParent;
        stadia5Explosive[index].localPosition = Vector3.zero;
        stadia5Explosive[index].gameObject.SetActive(false);
    }

    /*IEnumerator BulletTrajectory(Bullet bullet, Vector3 target, float speed, float lifetime)
    {
        Vector3 startPoint = bullet.transform.position;
        float endTime = Time.time + lifetime;
        Vector3 screenPoint = PlayerShoot.instance.MainCamera.WorldToScreenPoint(startPoint);
        while (bullet != null && Time.time < endTime && screenPoint.x >= 0 && screenPoint.x <= Screen.width && screenPoint.y >= 0 && screenPoint.y <= Screen.height)
        {
            startPoint.z = PlayerShoot.instance.transform.position.z;
            target.z = startPoint.z;
            Vector3 direction = (target - startPoint).normalized;
            
            bullet.transform.Translate(direction * speed * Time.deltaTime);
            
            yield return null;
        }
    }*/

    bool IsPointOnLine(Vector3 a, Vector3 b, Vector3 p)
    {
        // Вектор AB и вектор AP
        Vector3 AB = b - a;
        Vector3 AP = p - a;

        // Проверка коллинеарности с помощью векторного произведения
        Vector3 crossProduct = Vector3.Cross(AB, AP);
        if (crossProduct != Vector3.zero) return false;

        // Проверка, что P находится между A и B
        float dotProduct = Vector3.Dot(AP, AB);
        if (dotProduct < 0) return false; // P находится за точкой A

        float squaredLengthAB = AB.sqrMagnitude;
        if (dotProduct > squaredLengthAB) return false; // P находится за точкой B

        return true; // P находится на отрезке AB
    }

    private Vector3[] GetStartPoints(float z)
    {
        List<Vector3> points = new List<Vector3>();
        for (int i = 0; i < 4; i++)
        {
            Vector3[] corners = GetCornerPoints(i, z);
            int j = Random.Range(0, 2);
            points.Add(Vector3.Lerp(corners[j + 0], corners[j + 1], Random.Range(0f, 1f)));
        }

        return points.ToArray();
    }

    private float[] GetLaserSpeeds()
    {
        return new float[4]
        {
            Random.Range(1f, 3f),
            Random.Range(1f, 3f),
            Random.Range(1f, 3f),
            Random.Range(1f, 3f)
        };
    }

    private Vector3[] GetCornerPoints(int index, float z)
    {
        switch (index)
        {
            case 0:
                {
                    Vector3 point1 = PlayerShoot.instance.MainCamera.ScreenToWorldPoint(new Vector3((float)Screen.width * 0.45f, 0, z));
                    Vector3 point2 = PlayerShoot.instance.MainCamera.ScreenToWorldPoint(new Vector3(0, 0, z));
                    Vector3 point3 = PlayerShoot.instance.MainCamera.ScreenToWorldPoint(new Vector3(0, (float)Screen.height * 0.45f, z));
                    /*point1.z = PlayerShoot.instance.transform.position.z;
                    point2.z = PlayerShoot.instance.transform.position.z;
                    point3.z = PlayerShoot.instance.transform.position.z;*/
                    return new Vector3[3]
                    {
                        point1,
                        point2,
                        point3
                    };
                }
            case 1:
                {
                    Vector3 point1 = PlayerShoot.instance.MainCamera.ScreenToWorldPoint(new Vector3((float)Screen.width * 0.55f, 0, z));
                    Vector3 point2 = PlayerShoot.instance.MainCamera.ScreenToWorldPoint(new Vector3((float)Screen.width, 0, z));
                    Vector3 point3 = PlayerShoot.instance.MainCamera.ScreenToWorldPoint(new Vector3((float)Screen.width, (float)Screen.height * 0.45f, z));
                    /*point1.z = PlayerShoot.instance.transform.position.z;
                    point2.z = PlayerShoot.instance.transform.position.z;
                    point3.z = PlayerShoot.instance.transform.position.z;*/
                    return new Vector3[3]
                    {
                        point1,
                        point2,
                        point3
                    };
                }
            case 2:
                {
                    Vector3 point1 = PlayerShoot.instance.MainCamera.ScreenToWorldPoint(new Vector3(0, (float)Screen.height * 0.55f, z));
                    Vector3 point2 = PlayerShoot.instance.MainCamera.ScreenToWorldPoint(new Vector3(0, (float)Screen.height, z));
                    Vector3 point3 = PlayerShoot.instance.MainCamera.ScreenToWorldPoint(new Vector3((float)Screen.width * 0.45f, (float)Screen.height, z));
                    /*point1.z = PlayerShoot.instance.transform.position.z;
                    point2.z = PlayerShoot.instance.transform.position.z;
                    point3.z = PlayerShoot.instance.transform.position.z;*/
                    return new Vector3[3]
                    {
                        point1,
                        point2,
                        point3
                    };
                }
            case 3:
                {
                    Vector3 point1 = PlayerShoot.instance.MainCamera.ScreenToWorldPoint(new Vector3((float)Screen.width, (float)Screen.height * 0.55f, z));
                    Vector3 point2 = PlayerShoot.instance.MainCamera.ScreenToWorldPoint(new Vector3((float)Screen.width, (float)Screen.height, z));
                    Vector3 point3 = PlayerShoot.instance.MainCamera.ScreenToWorldPoint(new Vector3((float)Screen.width * 0.55f, (float)Screen.height, z));
                    /*point1.z = PlayerShoot.instance.transform.position.z;
                    point2.z = PlayerShoot.instance.transform.position.z;
                    point3.z = PlayerShoot.instance.transform.position.z;*/
                    return new Vector3[3]
                    {
                        point1,
                        point2,
                        point3
                    };
                }
        }

        return new Vector3[2] { Vector3.zero, Vector3.zero };
    }

    private IEnumerator ChangeWeapon()
    {
        animator.SetBool("IsChangingWeapon", true);
        PlayBossSound(weaponChangeSoundEffect);

        yield return new WaitForSeconds(weaponChangeDuration);

        animator.SetBool("IsChangingWeapon", false);
    }

    private IEnumerator SpawnEnemiesRoutine()
    {
        int enemiesToSpawn = baseEnemyCount + currentAdditionalEnemies;
        float timeToRest = spawnEnemiesTime / enemiesToSpawn;
        for (int i = 0; i < enemiesToSpawn; i++)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(timeToRest);
        }

        if (enemiesToSpawn == 0)
            yield return new WaitForSeconds(spawnEnemiesTime);
    }



    private void AddScoreOnDeath()
    {
        ScoreManager.Instance.AddScore(scoreValue);
    }
    private void HandleBossDeath()
    {
        if (isBossDefeated) return;

        isBossDefeated = true;
        Debug.Log("Boss defeated, handling death");
        AddScoreOnDeath();
        // Проигрываем рев босса при смерти
        if (bossRoarSound != null)
        {
            PlayBossSound(bossRoarSound);
        }
        EnvironmentStaticGenerator environmentGenerator = FindObjectOfType<EnvironmentStaticGenerator>();
        if (environmentGenerator != null)
        {
            Debug.Log("Notifying EnvironmentStaticGenerator about boss defeat");
            environmentGenerator.OnBossDefeated();
        }
        else
        {
            Debug.LogWarning("EnvironmentStaticGenerator not found!");
        }

        EnemyWaveGenerator enemyWaveGenerator = FindObjectOfType<EnemyWaveGenerator>();
        if (enemyWaveGenerator != null)
        {
            Debug.Log("Notifying EnemyWaveGenerator about boss defeat");
            enemyWaveGenerator.OnBossDefeated();
        }
        else
        {
            Debug.LogWarning("EnemyWaveGenerator not found!");
        }
        if (weaponUpgradeManager != null)
        {
            weaponUpgradeManager.OnBossDefeated();
        }

        StartCoroutine(DelayedDestroy());
    }

    private IEnumerator DelayedDestroy()
    {
        yield return new WaitForSeconds(1f); // Небольшая задержка перед уничтожением
        Debug.Log("Destroying boss object");
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (bossLife != null)
        {
            bossLife.OnBossDeath -= HandleBossDeath;
        }
        Debug.Log("Boss object destroyed");
    }
    public bool IsVulnerable()
    {
        return isVulnerable;
    }
    public bool IsPlayerInRange()
    {
        if (playerTransform == null)
        {
            FindPlayer();
            if (playerTransform == null) return false;
        }
        return Vector3.Distance(transform.position, playerTransform.position) <= shootRadius;
    }

    public void ShootBullet(int index)
    {
        if (playerTransform == null)
        {
            Debug.LogWarning("Cannot shoot bullet: player not found.");
            return;
        }

        Bullet bullet = bulletPoolManager.GetBossBullet(currentModeIndex == 0 || currentModeIndex == 6 ? 0 : 1);
        if (bullet != null)
        {
            bullet.transform.position = shootPoints[index].position;
            Vector3 direction = (playerTransform.position - shootPoints[index].position).normalized;
            bullet.Initialize(direction, bulletSpeed, 7f);
            PlayBossSound(attackSoundEffect);
        }
        else
        {
            Debug.LogWarning($"Failed to get bullet of type {currentModeIndex} from pool.");
        }
    }
    private void PlayBossSound(AudioClip clip)
    {
        if (clip != null && bossAudioSource != null)
        {
            bossAudioSource.PlayOneShot(clip);
        }
    }
    private void SpawnEnemy()
    {
        if (enemySpawnPoint != null)
        {
            Instantiate(enemyPrefab, enemySpawnPoint.position, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("Enemy spawn point is not set!");
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, shootRadius);
    }
}