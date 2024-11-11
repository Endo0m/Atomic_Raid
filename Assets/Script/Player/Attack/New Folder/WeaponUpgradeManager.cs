using UnityEngine;
using System.Collections.Generic;

public class WeaponUpgradeManager : MonoBehaviour
{
    [SerializeField] private PlayerShoot playerShoot;
    [SerializeField] private List<PlayerShoot.WeaponType> availableUpgrades;

    private List<PlayerShoot.WeaponType> remainingUpgrades;

    private void Start()
    {
        remainingUpgrades = new List<PlayerShoot.WeaponType>(availableUpgrades);
    }

    public void OnBossDefeated()
    {
        if (remainingUpgrades.Count > 0)
        {
            int randomIndex = Random.Range(0, remainingUpgrades.Count);
            PlayerShoot.WeaponType newWeapon = remainingUpgrades[randomIndex];

            playerShoot.AddWeapon(newWeapon);
            remainingUpgrades.RemoveAt(randomIndex);

            // ����� ����� �������� ������ ��� ����������� UI � ����������� � ����� ������
            Debug.Log($"��������� ����� ������: {newWeapon}");
        }
        else
        {
            Debug.Log("��� ��������� ������ ��� ��������");
        }
    }
}