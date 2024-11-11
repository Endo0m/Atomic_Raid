using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerShoot : MonoBehaviour
{
    public static PlayerShoot instance = null;
    
    [SerializeField] private Transform weaponTransform;
    [SerializeField] private float maxShootDistance = 100f;
    [SerializeField] private LayerMask shootableLayers;
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private float bulletSpeed = 50f;
    [SerializeField] private float bulletLifetime = 1f;
    [SerializeField] private float chainlightningCooldown = 1f;
    [SerializeField] private BulletPoolManager bulletPoolManager;
    [SerializeField] private AudioClip laserBulletSFX;
    [SerializeField] private LayerMask bossBulletLayer;
    [SerializeField] private ExplosiveWeaponSettings explosiveWeaponSettings;
    [SerializeField] private WeaponUI weaponUI;

    private bool isAutoFireEnabled = false;
    private Coroutine autoFireCoroutine;
    private bool isFireRateEnhanced = false;
    private float enhancedFireRate;
    private bool canShoot = true;
    private const string LaserSoundName = "PlayerLaserShot";
    private Camera mainCamera;
    private List<WeaponType> availableWeapons = new List<WeaponType> { WeaponType.Standard };
    private WeaponType currentWeapon = WeaponType.Standard;
    private Dictionary<WeaponType, int> ammo = new Dictionary<WeaponType, int>();
    private Dictionary<WeaponType, float> nextFireTime = new Dictionary<WeaponType, float>();
    private Dictionary<WeaponType, float> reloadProgress = new Dictionary<WeaponType, float>();
    private float originalFireRate;

    private bool isShootButtonPressed = false;
    public enum WeaponType
    {
        Standard,
        Explosive
    }

    public Camera MainCamera
    {
        get { return mainCamera; }
    }

    public LayerMask ShootableLayers
    {
        get { return shootableLayers; }
    }

    public Transform WeaponTransform
    {
        get { return weaponTransform; }
    }

    public WeaponType CurrentWeapon
    {
        get { return currentWeapon; }
    }

    private void Awake()
    {
        instance = this;
        mainCamera = Camera.main;
        bulletPoolManager = FindObjectOfType<BulletPoolManager>();
        InitializeWeapons();
    }

    private void Start()
    {
        originalFireRate = ConfigData.Instance.playerShootRate;
        if (weaponUI == null)
        {
            weaponUI = FindObjectOfType<WeaponUI>();
        }
    }

    private void InitializeWeapons()
    {
        foreach (WeaponType weapon in System.Enum.GetValues(typeof(WeaponType)))
        {
            ammo[weapon] = (weapon == WeaponType.Standard) ? int.MaxValue : explosiveWeaponSettings.magazineSize;
            nextFireTime[weapon] = 0f;
            reloadProgress[weapon] = 1f; // Полная перезарядка
        }
    }

    private void Update()
    {
        if (Time.timeScale == 0 || !canShoot) return;

        HandleWeaponSwitch();
        HandleAutoFire();
        HandleShootInput();
        HandleShooting();
        UpdateReload();
        UpdateUI();
    }

    private void HandleShootInput()
    {
        isShootButtonPressed = Input.GetMouseButton(0);
    }

    private void HandleWeaponSwitch()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) SwitchWeapon(0);
        if (Input.GetKeyDown(KeyCode.Alpha2) && availableWeapons.Count > 1) SwitchWeapon(1);
        if (Input.GetKeyDown(KeyCode.Alpha3) && availableWeapons.Count > 2) SwitchWeapon(2);
    }

    private void HandleAutoFire()
    {
        if (Input.GetMouseButtonDown(1) && currentWeapon == WeaponType.Standard)
        {
            ToggleAutoFire();
        }
    }

    private void ToggleAutoFire()
    {
        if (currentWeapon != WeaponType.Standard) return;

        isAutoFireEnabled = !isAutoFireEnabled;
        if (isAutoFireEnabled && autoFireCoroutine == null)
        {
            autoFireCoroutine = StartCoroutine(AutoFireCoroutine());
        }
        else if (!isAutoFireEnabled && autoFireCoroutine != null)
        {
            StopCoroutine(autoFireCoroutine);
            autoFireCoroutine = null;
        }
        UpdateUI();
    }

    private IEnumerator AutoFireCoroutine()
    {
        while (isAutoFireEnabled && currentWeapon == WeaponType.Standard)
        {
            if (Time.time >= nextFireTime[currentWeapon] && canShoot)
            {
                Shoot();
                nextFireTime[currentWeapon] = Time.time + GetFireRate();
            }
            yield return null;
        }
    }
    public void SwitchWeapon(int index)
    {
        if (index < availableWeapons.Count)
        {
            WeaponType newWeapon = availableWeapons[index];
            if (newWeapon != WeaponType.Standard && ammo[newWeapon] == 0)
            {
                return; // Запрещаем переключение, если нет патронов
            }
            currentWeapon = newWeapon;
            if (currentWeapon != WeaponType.Standard && isAutoFireEnabled)
            {
                isAutoFireEnabled = false;
                if (autoFireCoroutine != null)
                {
                    StopCoroutine(autoFireCoroutine);
                    autoFireCoroutine = null;
                }
            }
            UpdateUI();
        }
    }

    private void HandleShooting()
    {
        bool canFireNow = Time.time >= nextFireTime[currentWeapon] && ammo[currentWeapon] > 0 && canShoot;

        if (canFireNow && (isShootButtonPressed || (isAutoFireEnabled && currentWeapon == WeaponType.Standard)))
        {
            Shoot();
            nextFireTime[currentWeapon] = Time.time + GetFireRate();
            if (currentWeapon != WeaponType.Standard)
            {
                ammo[currentWeapon]--;
                reloadProgress[currentWeapon] = (float)ammo[currentWeapon] / explosiveWeaponSettings.magazineSize;
                if (ammo[currentWeapon] == 0)
                {
                    SwitchToStandardWeapon();
                }
            }
            UpdateUI();
        }
    }

    private void UpdateReload()
    {
        foreach (var weapon in availableWeapons)
        {
            if (weapon != WeaponType.Standard)
            {
                if (ammo[weapon] == 0 && reloadProgress[weapon] < 1f)
                {
                    reloadProgress[weapon] += Time.deltaTime / explosiveWeaponSettings.reloadTime;
                    if (reloadProgress[weapon] >= 1f)
                    {
                        reloadProgress[weapon] = 1f;
                        ammo[weapon] = explosiveWeaponSettings.magazineSize;
                    }
                }
                else
                {
                    reloadProgress[weapon] = (float)ammo[weapon] / explosiveWeaponSettings.magazineSize;
                }
            }
        }
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (weaponUI != null)
        {
            int[] ammoCount = new int[availableWeapons.Count];
            float[] reloadProgressArray = new float[availableWeapons.Count];
            for (int i = 0; i < availableWeapons.Count; i++)
            {
                ammoCount[i] = ammo[availableWeapons[i]];
                reloadProgressArray[i] = reloadProgress[availableWeapons[i]];
            }
            bool showAutoFire = currentWeapon == WeaponType.Standard;
            weaponUI.UpdateUI(availableWeapons.IndexOf(currentWeapon), ammoCount, reloadProgressArray, isAutoFireEnabled, showAutoFire);
        }
    }

    private float GetFireRate()
    {
        if (currentWeapon == WeaponType.Standard)
        {
            return isFireRateEnhanced ? enhancedFireRate : ConfigData.Instance.playerShootRate;
        }
        else
        {
            return explosiveWeaponSettings.fireRate;
        }
    }

    private float lastTimeChainlightning = 0;
    private void Shoot()
    {
        if (!canShoot) return;

        if (PlayerFlags.Chainlightning && PlayerFlags.CurWeapon == currentWeapon)
        {
            if (Time.time - lastTimeChainlightning < chainlightningCooldown)
                return;
            lastTimeChainlightning = Time.time;
        }
        
        Vector3 aimPoint = GetAimPoint();
        Vector3 shootDirection = (aimPoint - weaponTransform.position).normalized;

        if (currentWeapon == WeaponType.Standard)
        {
            ShootStandardBullet(shootDirection);
        }
        else if (currentWeapon == WeaponType.Explosive)
        {
            ShootExplosiveBullet(shootDirection);
        }

        if (PlayerFlags.PlasmaBullet || PlayerFlags.Firecannon || PlayerFlags.Chainlightning) { }
        else if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }

        if (laserBulletSFX != null)
        {
            AudioPlayer.Instance.PlaySound(laserBulletSFX, 1f, LaserSoundName);
        }
    }

    private void ShootStandardBullet(Vector3 direction)
    {
        Bullet bullet = PlayerFlags.PlasmaBullet && PlayerFlags.CurWeapon == WeaponType.Standard ? bulletPoolManager.GetPlasmaBullet() :
            PlayerFlags.Chainlightning && PlayerFlags.CurWeapon == WeaponType.Standard ? bulletPoolManager.GetChainlightningBullet() :
            PlayerFlags.Firecannon && PlayerFlags.CurWeapon == WeaponType.Standard ? bulletPoolManager.GetFirecannonBullet() :
            bulletPoolManager.GetPlayerBullet();
        if (bullet != null)
        {
            bullet.transform.position = weaponTransform.position;
            bullet.transform.rotation = Quaternion.LookRotation(direction);
            
            if (PlayerFlags.LargeBullet && PlayerFlags.CurWeapon == WeaponType.Standard)
                bullet.transform.localScale = new Vector3(2f, 2f, 2f);

            SetStandartBullet(direction, bullet);
        }
    }

    private void SetStandartBullet(Vector3 direction, Bullet bullet)
    {
        if (PlayerFlags.Chainlightning)
        {
            bullet.Fire();
        }
        else
        {
            bullet.Initialize(direction, bulletSpeed * (PlayerFlags.LargeBullet && PlayerFlags.CurWeapon == currentWeapon ? 0.5f : 1f),
                PlayerFlags.PlasmaBullet || PlayerFlags.Firecannon || PlayerFlags.Chainlightning ? 100 : bulletLifetime);

            DamageDealer damageDealer = bullet.GetComponent<DamageDealer>();
            if (damageDealer == null)
            {
                damageDealer = bullet.gameObject.AddComponent<DamageDealer>();
            }

            damageDealer.SetDamageAmount(1);
        }
    }

    private void ShootExplosiveBullet(Vector3 direction)
    {
        Bullet bullet = PlayerFlags.PlasmaBullet && PlayerFlags.CurWeapon == WeaponType.Explosive ? bulletPoolManager.GetPlasmaBullet() :
            PlayerFlags.Chainlightning && PlayerFlags.CurWeapon == WeaponType.Explosive ? bulletPoolManager.GetChainlightningBullet() :
            PlayerFlags.Firecannon && PlayerFlags.CurWeapon == WeaponType.Explosive ? bulletPoolManager.GetFirecannonBullet() :
            bulletPoolManager.GetPlayerBullet();
        if (bullet != null)
        {
            bullet.transform.position = weaponTransform.position;
            bullet.transform.rotation = Quaternion.LookRotation(direction);

            if (PlayerFlags.LargeBullet && PlayerFlags.CurWeapon == WeaponType.Explosive)
                bullet.transform.localScale = new Vector3(2f, 2f, 2f);

            if (PlayerFlags.PlasmaBullet && PlayerFlags.CurWeapon == WeaponType.Explosive)
            {
                SetStandartBullet(direction, bullet);
            }
            else if (PlayerFlags.Firecannon && PlayerFlags.CurWeapon == WeaponType.Explosive)
            {
                SetStandartBullet(direction, bullet);
            }
            else if (PlayerFlags.Chainlightning && PlayerFlags.CurWeapon == WeaponType.Explosive)
            {
                SetStandartBullet(direction, bullet);
            }
            else
            {
                ExplosiveBullet explosiveBullet = bullet.gameObject.AddComponent<ExplosiveBullet>();
                explosiveBullet.settings = explosiveWeaponSettings;
                explosiveBullet.Initialize(direction, explosiveWeaponSettings.bulletSpeed * (PlayerFlags.LargeBullet && PlayerFlags.CurWeapon == WeaponType.Explosive ? 0.5f : 1f));

                DamageDealer damageDealer = bullet.GetComponent<DamageDealer>();
                if (damageDealer == null)
                {
                    damageDealer = bullet.gameObject.AddComponent<DamageDealer>();
                }

                damageDealer.SetDamageAmount(explosiveWeaponSettings.mainBulletDamage);
            }
        }
    }

    public Vector3 GetAimPoint()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, maxShootDistance, shootableLayers | bossBulletLayer))
        {
            return hit.point;
        }
        return ray.GetPoint(maxShootDistance);
    }

    public void AddWeapon(WeaponType weaponType)
    {
        if (!availableWeapons.Contains(weaponType))
        {
            availableWeapons.Add(weaponType);
            ammo[weaponType] = explosiveWeaponSettings.magazineSize;
            reloadProgress[weaponType] = 1f;
            UpdateUI();
        }
    }

    public void SetShootingEnabled(bool enabled)
    {
        canShoot = enabled;
    }

    public bool IsShootingEnabled()
    {
        return canShoot;
    }

    public void EnhanceFireRate(float duration)
    {
        StopAllCoroutines();
        isFireRateEnhanced = true;
        enhancedFireRate = ConfigData.Instance.playerShootRate / 2f;
        StartCoroutine(ResetFireRateAfterDelay(duration));
    }

    private IEnumerator ResetFireRateAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ResetFireRate();
    }

    public void ResetFireRate()
    {
        isFireRateEnhanced = false;
        // Не изменяем ConfigData.Instance.playerShootRate здесь
    }

    public void ResetAvailableWeapons()
    {
        availableWeapons.Clear();
        availableWeapons.Add(WeaponType.Standard);
        if (explosiveWeaponSettings != null)
        {
            availableWeapons.Add(WeaponType.Explosive);
            ammo[WeaponType.Explosive] = explosiveWeaponSettings.magazineSize;
            reloadProgress[WeaponType.Explosive] = 1f;
        }
        currentWeapon = WeaponType.Standard;
        UpdateUI();
    }

    private void SwitchToStandardWeapon()
    {
        currentWeapon = WeaponType.Standard;
        UpdateUI();
    }
}