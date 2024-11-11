using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponUI : MonoBehaviour
{
    [System.Serializable]
    public class WeaponUIElement
    {
        public GameObject gameObject;
        public Image weaponImage;
        public Image fillImage;
        public TextMeshProUGUI ammoText;
    }

    public WeaponUIElement[] weaponUIElements;
    public Image autoFireImage;

    public void UpdateUI(int currentWeaponIndex, int[] ammoCount, float[] reloadProgress, bool isAutoFireEnabled, bool showAutoFire)
    {
        for (int i = 0; i < weaponUIElements.Length; i++)
        {
            if (i < ammoCount.Length)
            {
                weaponUIElements[i].gameObject.SetActive(true);

                // Включаем weaponImage для текущего оружия
                weaponUIElements[i].weaponImage.color = (i == currentWeaponIndex) ? Color.white : Color.gray;

                if (i == 0) // Основное оружие
                {
                    weaponUIElements[i].fillImage.enabled = false;
                    weaponUIElements[i].ammoText.enabled = false;
                }
                else // Дополнительное оружие
                {
                    weaponUIElements[i].fillImage.enabled = true;
                    weaponUIElements[i].fillImage.fillAmount = reloadProgress[i];
                    weaponUIElements[i].ammoText.enabled = true;
                    weaponUIElements[i].ammoText.text = ammoCount[i].ToString();
                }
            }
            else
            {
                weaponUIElements[i].gameObject.SetActive(false);
            }
        }

        // Обновляем изображение автоматической стрельбы
        autoFireImage.gameObject.SetActive(showAutoFire);
        if (showAutoFire)
        {
            autoFireImage.color = isAutoFireEnabled ? Color.green : Color.red;
        }
    }
}