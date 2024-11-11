using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Класс BuffFieldsController
/// отвечает за эфекты баффов и 
/// находиться на объекте (Игроке в нашем случае)
/// который получаеть баффы
/// </summary>
public class BuffFieldsController : MonoBehaviour
{
    public static int RevertMoveValue = 1;
    public static bool WeaponEnhancementCheking = false;
    private Coroutine weaponLockCoroutine;
    [SerializeField] private float _lockWeaponTime;
    [SerializeField] private float _revertMoveTime;
    [SerializeField] private float _invulnerabilityTime;
    [SerializeField] private float _weaponEnhancement;
    [SerializeField] private float _weaponLaser;
    [SerializeField] private float _weaponEnergyBeam;
    [SerializeField] private float _weaponEnergyWeapon;
    [SerializeField] private float _weaponFlamethrower;
    [SerializeField] private float _weaponChainlightning;
    [SerializeField] private int GetHP;
    private GameUIManager gameUIManager;
    private PlayerParticleEffects particleEffects;
    private PlayerLife _playerLife;
    private PlayerMove _playerMove;
    private string _buffName;
    private PlayerAvatarManager avatarManager;
    private BuffManager buffManager;

    private void Start()
    {
        avatarManager = FindObjectOfType<PlayerAvatarManager>();
        _playerLife = GetComponent<PlayerLife>();
        _playerMove = GetComponent<PlayerMove>(); particleEffects = GetComponent<PlayerParticleEffects>();
        gameUIManager = FindObjectOfType<GameUIManager>();
        buffManager = GetComponent<BuffManager>();
        if (buffManager == null)
        {
            buffManager = gameObject.AddComponent<BuffManager>();
        }
    }

    public bool CanApplyBuff(BuffList buffType)
    {
        if (buffType == BuffList.GetLives || buffType == BuffList.LaserWeapon || buffType == BuffList.EnergyBeamWeapon || buffType == BuffList.EnergyWeapon || buffType == BuffList.FlamethrowerWeapon || buffType == BuffList.ChainlightningWeapon)
        {
            return true; // GetLives всегда можно подобрать
        }

        // Проверяем, есть ли активные баффы
        return !buffManager.HasActiveBuffs();
    }

    private void CheckCancel(BuffList buffType)
    {
        foreach (KeyValuePair<BuffList, float> entry in buffManager.ActiveBuffs)
            if (entry.Key != buffType)
                if (CancelWithOtherBuff(entry.Key))
                    if (!CanStack(buffType))
                        CancelBuff(entry.Key);
    }

    /// <summary>
    /// начало действия баффа
    /// </summary>
    /// <param name="buffList">получаем имя баффа</param>
    public void StartBuff(string buffList)
    {
        _buffName = buffList;
        BuffList buffType = (BuffList)System.Enum.Parse(typeof(BuffList), _buffName);
        float duration = GetBuffDuration(_buffName);
        if (CanApplyBuff(buffType))
        {
            CheckCancel(buffType);
            BuffDefinition();
            if (buffType != BuffList.GetLives)
            {
                if (particleEffects != null)
                {
                    if (buffType == BuffList.WeaponLocked || buffType == BuffList.RevertMove)
                    {
                        particleEffects.PlayDebuffEffect(buffType, duration);
                    }
                    else
                    {
                        particleEffects.PlayBuffEffect(buffType, duration);
                    }
                }
                if (gameUIManager != null)
                {
                    gameUIManager.ShowBuff(buffType, duration);
                }
                buffManager.AddBuff(buffType, duration);
            }
            else
            {
                if (particleEffects != null)
                {
                    particleEffects.PlayHealEffect();
                }
                if (gameUIManager != null)
                {
                    gameUIManager.UpdateHealthDisplay(_playerLife.CurrentHealth);
                }
            }
        }
    }

    /// <summary>
    /// в зависимости от баффа вызываем эффект 
    /// </summary>
    private void BuffDefinition()
    {
        switch (_buffName)
        {
            case "WeaponLocked":
                WeaponLocked();
                avatarManager.UpdateAvatarState(AvatarState.Negative);
                break;
            case "GetLives":
                GetLives();
                avatarManager.UpdateAvatarState(AvatarState.Positive);
                break;
            case "RevertMove":
                RevertMove();
                avatarManager.UpdateAvatarState(AvatarState.Negative);
                break;           
            case "Invulnerability":
                InvulnerabilityPlayer();
                avatarManager.UpdateAvatarState(AvatarState.Positive);
                break;
            case "WeaponEnhancement":
                WeaponEnhancement();
                avatarManager.UpdateAvatarState(AvatarState.Positive);
                break;
            case "LaserWeapon":
                LaserWeapon();
                avatarManager.UpdateAvatarState(AvatarState.Positive);
                break;
            case "EnergyBeamWeapon":
                EnergyBeamWeapon();
                avatarManager.UpdateAvatarState(AvatarState.Positive);
                break;
            case "EnergyWeapon":
                EnergyWeapon();
                avatarManager.UpdateAvatarState(AvatarState.Positive);
                break;
            case "FlamethrowerWeapon":
                FlamethrowerWeapon();
                avatarManager.UpdateAvatarState(AvatarState.Positive);
                break;
            case "ChainlightningWeapon":
                ChainlightningWeapon();
                avatarManager.UpdateAvatarState(AvatarState.Positive);
                break;
        }
    }

    private float EnergyWeaponTime = 0;
    private void EnergyWeapon()
    {
        bool startCoroutine = EnergyWeaponTime == 0;
        EnergyWeaponTime = Time.time + GetBuffDuration("EnergyWeapon");

        if (startCoroutine)
            StartCoroutine(StartEnergyWeapon());
    }

    IEnumerator StartEnergyWeapon()
    {
        yield return null;
        
        var playerShoot = PlayerShoot.instance;

        PlayerShoot.WeaponType curWeapon = playerShoot.CurrentWeapon;

        PlayerFlags.PlasmaBullet = true;
        
        while (Time.time < EnergyWeaponTime && curWeapon == playerShoot.CurrentWeapon)
            yield return null;

        PlayerFlags.PlasmaBullet = false;
        
        if (buffManager.ActiveBuffs.ContainsKey(BuffList.EnergyWeapon))
            buffManager.ActiveBuffs[BuffList.EnergyWeapon] = 0;

        EnergyWeaponTime = 0;
    }


    private float FlamethrowerWeaponTime = 0;
    private void FlamethrowerWeapon()
    {
        bool startCoroutine = FlamethrowerWeaponTime == 0;
        FlamethrowerWeaponTime = Time.time + GetBuffDuration("FlamethrowerWeapon");

        if (startCoroutine)
            StartCoroutine(StartFlamethrowerWeapon());
    }

    IEnumerator StartFlamethrowerWeapon()
    {
        yield return null;
        
        var playerShoot = PlayerShoot.instance;

        PlayerShoot.WeaponType curWeapon = playerShoot.CurrentWeapon;

        PlayerFlags.Firecannon = true;
        
        while (Time.time < FlamethrowerWeaponTime && curWeapon == playerShoot.CurrentWeapon)
            yield return null;

        PlayerFlags.Firecannon = false;
        
        if (buffManager.ActiveBuffs.ContainsKey(BuffList.FlamethrowerWeapon))
            buffManager.ActiveBuffs[BuffList.FlamethrowerWeapon] = 0;

        FlamethrowerWeaponTime = 0;
    }

    private float ChainlightningWeaponTime = 0;
    private void ChainlightningWeapon()
    {
        bool startCoroutine = ChainlightningWeaponTime == 0;
        ChainlightningWeaponTime = Time.time + GetBuffDuration("ChainlightningWeapon");

        if (startCoroutine)
            StartCoroutine(StartChainlightningWeapon());
    }

    IEnumerator StartChainlightningWeapon()
    {
        yield return null;
        
        var playerShoot = PlayerShoot.instance;

        PlayerShoot.WeaponType curWeapon = playerShoot.CurrentWeapon;

        PlayerFlags.instance.ActivateChainlightning();
        
        while (Time.time < ChainlightningWeaponTime && curWeapon == playerShoot.CurrentWeapon)
            yield return null;

        PlayerFlags.instance.DeactivateChainlightning();
        
        if (buffManager.ActiveBuffs.ContainsKey(BuffList.ChainlightningWeapon))
            buffManager.ActiveBuffs[BuffList.ChainlightningWeapon] = 0;

        ChainlightningWeaponTime = 0;
    }

    private float LaserWeaponTime = 0;
    private void LaserWeapon()
    {
        bool startCoroutine = LaserWeaponTime == 0;
        LaserWeaponTime = Time.time + GetBuffDuration("LaserWeapon");

        if (startCoroutine)
            StartCoroutine(StartLaserWeapon());
    }

    IEnumerator StartLaserWeapon()
    {
        yield return null;
        
        var playerShoot = PlayerShoot.instance;

        PlayerShoot.WeaponType curWeapon = playerShoot.CurrentWeapon;
        
        PlayerFlags.instance.FireLaser();
        
        playerShoot.SetShootingEnabled(false);
        while (Time.time < LaserWeaponTime && curWeapon == playerShoot.CurrentWeapon)
            yield return null;
        
        PlayerFlags.instance.StopLaser();
        
        if (!buffManager.ActiveBuffs.ContainsKey(BuffList.WeaponLocked))
            playerShoot.SetShootingEnabled(true);

        if (buffManager.ActiveBuffs.ContainsKey(BuffList.LaserWeapon))
            buffManager.ActiveBuffs[BuffList.LaserWeapon] = 0;

        LaserWeaponTime = 0;
    }

    private float EnergyBeamWeaponTime = 0;
    private void EnergyBeamWeapon()
    {
        bool startCoroutine = EnergyBeamWeaponTime == 0;
        EnergyBeamWeaponTime = Time.time + GetBuffDuration("EnergyBeamWeapon");

        if (startCoroutine)
            StartCoroutine(StartEnergyBeamWeapon());
    }

    IEnumerator StartEnergyBeamWeapon()
    {
        yield return null;

        var playerShoot = PlayerShoot.instance;

        PlayerShoot.WeaponType curWeapon = playerShoot.CurrentWeapon;

        PlayerFlags.LargeBullet = true;
        PlayerFlags.CurWeapon = curWeapon;
        
        while (Time.time < EnergyBeamWeaponTime && curWeapon == playerShoot.CurrentWeapon)
            yield return null;

        PlayerFlags.LargeBullet = false;
        
        if (buffManager.ActiveBuffs.ContainsKey(BuffList.EnergyBeamWeapon))
            buffManager.ActiveBuffs[BuffList.EnergyBeamWeapon] = 0;

        EnergyBeamWeaponTime = 0;
    }

    /// <summary>
    /// Блокируем оружие игрока
    /// </summary>
    private void WeaponLocked()
    {
        var playerShoot = GetComponent<PlayerShoot>();

        if (playerShoot != null)
        {
            if (playerShoot.IsShootingEnabled())
            {
                playerShoot.SetShootingEnabled(false);
                if (weaponLockCoroutine != null)
                {
                    StopCoroutine(weaponLockCoroutine);
                }
                weaponLockCoroutine = StartCoroutine(WeaponLockCoroutine(_lockWeaponTime));
            }
            // Если оружие уже заблокировано, ничего не делаем
        }
    }

    private IEnumerator WeaponLockCoroutine(float time)
    {
        yield return new WaitForSeconds(time);

        var playerShoot = GetComponent<PlayerShoot>();
        if (playerShoot != null)
        {
            playerShoot.SetShootingEnabled(true);
        }
        weaponLockCoroutine = null;
    }
    /// <summary>
    /// Добавляем ХП игроку 
    /// </summary>
    private void GetLives()
    {
        PlayerLife playerLife = GetComponent<PlayerLife>();
        if (playerLife != null && playerLife.CurrentHealth < playerLife.MaxHealth)
        {
            playerLife.RestoreHealth(GetHP);
            // Не показываем UI для отхила
        }
    }

    /// <summary>
    /// Реверсивное управление 
    /// </summary>
    private void RevertMove()
    {
        if (_playerMove != null)
        {
            _playerMove.SetRevertMove(true);
            StartCoroutine(RevertMoveCoroutine());
        }
    }

    private IEnumerator RevertMoveCoroutine()
    {
        yield return new WaitForSeconds(_revertMoveTime);
        if (_playerMove != null)
        {
            _playerMove.SetRevertMove(false);
        }
    }

    /// <summary>
    /// Включаем неуязвимость
    /// </summary>
    private void InvulnerabilityPlayer()
    {
        StartCoroutine(_playerLife.InvulnerabilityCoroutine(_invulnerabilityTime));
        Shield shield = GetComponent<Shield>();
        if (shield != null)
        {
            shield.gameObject.SetActive(true);
        }
        if (particleEffects != null)
        {
            particleEffects.PlayShieldEffect(true);
        }
        StartCoroutine(DisableShieldAfterDelay(_invulnerabilityTime));
    }

    private IEnumerator DisableShieldAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Shield shield = GetComponent<Shield>();
        if (shield != null)
        {
            shield.gameObject.SetActive(false);
        }
        if (particleEffects != null)
        {
            particleEffects.PlayShieldEffect(false);
        }
    }

    private void WeaponEnhancement()
    {
        PlayerShoot playerShoot = GetComponent<PlayerShoot>();
        if (playerShoot != null)
        {
            if (!WeaponEnhancementCheking)
            {
                WeaponEnhancementCheking = true;
                playerShoot.EnhanceFireRate(_weaponEnhancement);
                StartCoroutine(WeaponEnhancementCoroutine(_weaponEnhancement));
            }
            else
            {
                WeaponEnhancementCheking = false;
                playerShoot.ResetFireRate();
            }
        }
    }

    private IEnumerator WeaponEnhancementCoroutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        WeaponEnhancement(); // This will reset the enhancement
    }

    private float GetBuffDuration(string buffName)
    {
        switch (buffName)
        {
            case "WeaponLocked":
                return _lockWeaponTime;
            case "RevertMove":
                return _revertMoveTime;
            case "Invulnerability":
                return _invulnerabilityTime;
            case "WeaponEnhancement":
                return _weaponEnhancement;
            case "LaserWeapon":
                return _weaponLaser;
            case "EnergyBeamWeapon":
                return _weaponEnergyBeam;
            case "EnergyWeapon":
                return _weaponEnergyWeapon;
            case "FlamethrowerWeapon":
                return _weaponFlamethrower;
            case "ChainlightningWeapon":
                return _weaponChainlightning;
            default:
                return 0f;
        }
    }


    private bool CancelWithOtherBuff(BuffList buff)
    {
        switch (buff)
        {
            case BuffList.LaserWeapon:
            case BuffList.EnergyBeamWeapon:
            case BuffList.EnergyWeapon:
            case BuffList.FlamethrowerWeapon:
            case BuffList.ChainlightningWeapon:
                return true;
            default:
                return false;
        }
    }

    private bool CanStack(BuffList buff)
    {
        switch (buff)
        {
            case BuffList.EnergyBeamWeapon:
                return !buffManager.ActiveBuffs.ContainsKey(BuffList.LaserWeapon);
            default:
                return false;
        }
    }

    private void CancelBuff(BuffList buff)
    {
        switch (buff)
        {
            case BuffList.LaserWeapon:
                if (buffManager.ActiveBuffs.ContainsKey(BuffList.LaserWeapon))
                    buffManager.ActiveBuffs[BuffList.LaserWeapon] = 0;
                LaserWeaponTime = 0;
                break;
            case BuffList.EnergyBeamWeapon:
                if (buffManager.ActiveBuffs.ContainsKey(BuffList.EnergyBeamWeapon))
                    buffManager.ActiveBuffs[BuffList.EnergyBeamWeapon] = 0;
                EnergyBeamWeaponTime = 0;
                break;
            case BuffList.EnergyWeapon:
                if (buffManager.ActiveBuffs.ContainsKey(BuffList.EnergyWeapon))
                    buffManager.ActiveBuffs[BuffList.EnergyWeapon] = 0;
                EnergyWeaponTime = 0;
                break;
            case BuffList.FlamethrowerWeapon:
                if (buffManager.ActiveBuffs.ContainsKey(BuffList.FlamethrowerWeapon))
                    buffManager.ActiveBuffs[BuffList.FlamethrowerWeapon] = 0;
                FlamethrowerWeaponTime = 0;
                break;
            case BuffList.ChainlightningWeapon:
                if (buffManager.ActiveBuffs.ContainsKey(BuffList.ChainlightningWeapon))
                    buffManager.ActiveBuffs[BuffList.ChainlightningWeapon] = 0;
                ChainlightningWeaponTime = 0;
                break;
        }
    }

}
