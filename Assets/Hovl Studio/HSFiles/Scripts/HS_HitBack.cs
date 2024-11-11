using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HS_HitBack : MonoBehaviour
{
    [SerializeField] private Transform setParent;
    private Transform saveparent = null;
    
    private void Awake()
    {
        saveparent = transform.parent;
    }

    private void Update()
    {
        if (PlayerShoot.instance.WeaponTransform.position.z - transform.position.z > 10)
        {
            transform.parent = setParent == null ? saveparent : setParent;
            gameObject.SetActive(false);
        }
    }
}
