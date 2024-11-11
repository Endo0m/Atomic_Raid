using UnityEngine;

[CreateAssetMenu(fileName = "ExplosiveWeaponSettings", menuName = "Weapons/Explosive Weapon Settings", order = 1)]
public class ExplosiveWeaponSettings : ScriptableObject
{
    [Header("Основные настройки")]
    public float fireRate = 0.5f;
    public int magazineSize = 10;
    public float reloadTime = 2f;

    [Header("Настройки основного снаряда")]
    public float mainBulletLifetime = 5f;
    public int mainBulletDamage = 3;
    public float explosionRadius = 3f;
    public float bulletSpeed = 20f;
    public LayerMask explosionLayers; 

    [Header("Настройки осколков")]
    public int fragmentCount = 5;
    public float fragmentSpeed = 10f;
    public float fragmentLifetime = 2f;
}