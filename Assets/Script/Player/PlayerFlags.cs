using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFlags : MonoBehaviour
{
    public static bool LargeBullet = false;
    public static bool PlasmaBullet = false;
    public static bool Firecannon = false;
    public static bool Chainlightning = false;
    public static PlayerShoot.WeaponType CurWeapon = PlayerShoot.WeaponType.Standard;

    public static PlayerFlags instance = null;

    [SerializeField]
    private Transform laser;
    private Transform saveparent;

    [SerializeField] private GameObject ChainlightningFlash;

    private void Awake()
    {
        saveparent = transform.parent;
        instance = this;
    }

    /*private void Start()
    {
        ActivateChainlightning();
    }*/

    public void FireLaser()
    {
        laser.gameObject.SetActive(true);
        
        laser.parent = PlayerShoot.instance.WeaponTransform;
        laser.localPosition = Vector3.zero;
    }

    public void StopLaser()
    {
        if (laser.parent != saveparent)
        {
            laser.parent = saveparent;
            laser.gameObject.SetActive(false);
        }
    }

    public void ActivateChainlightning()
    {
        Chainlightning = true;
        ChainlightningFlash.SetActive(true);
    }

    public void DeactivateChainlightning()
    {
        Chainlightning = false;
        ChainlightningFlash.SetActive(false);
    }
}
