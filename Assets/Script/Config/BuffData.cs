using UnityEngine;

[CreateAssetMenu(fileName = "BuffData", menuName = "Buffs/BuffData")]
public class BuffData : ScriptableObject
{
    [System.Serializable]
    public class BuffInfo
    {
        public BuffList buffType;
        public Sprite icon;
    }

    public BuffInfo[] buffs;
}