using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BuffManager : MonoBehaviour
{
    private Dictionary<BuffList, float> activeBuffs = new Dictionary<BuffList, float>();
    private GameUIManager uiManager;

    public Dictionary<BuffList, float> ActiveBuffs
    {
        get { return activeBuffs; }
    }

    private void Start()
    {
        uiManager = FindObjectOfType<GameUIManager>();
    }

    public bool IsBuffActive(BuffList buffType)
    {
        return activeBuffs.ContainsKey(buffType);
    }
    public bool HasActiveBuffs()
    {
        return activeBuffs.Count > 0;
    }
    public void AddBuff(BuffList buffType, float duration)
    {
        if (activeBuffs.ContainsKey(buffType))
        {
            activeBuffs[buffType] = duration;
        }
        else
        {
            activeBuffs.Add(buffType, duration);
        }

        if (uiManager != null)
        {
            uiManager.ShowBuff(buffType, duration);
        }
    }

    private void Update()
    {
        List<BuffList> expiredBuffs = new List<BuffList>();

        foreach (var buff in activeBuffs.ToList()) // Используем ToList() для создания копии
        {
            activeBuffs[buff.Key] -= Time.deltaTime;
            if (activeBuffs[buff.Key] <= 0)
            {
                expiredBuffs.Add(buff.Key);
            }
        }

        foreach (var expiredBuff in expiredBuffs)
        {
            activeBuffs.Remove(expiredBuff);
            if (uiManager != null)
            {
                uiManager.ClearBuff();
            }
        }

        // Обновляем отображение активного баффа
        if (activeBuffs.Count > 0)
        {
            var activeBuff = activeBuffs.OrderByDescending(b => b.Value).First();
            if (uiManager != null)
            {
                uiManager.UpdateBuffDisplay(activeBuff.Key, activeBuff.Value);
            }
        }
    }
}