using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WaveData")]
public class WaveData : ScriptableObject
{
    [System.Serializable]
    public struct WaveInfo
    {
        public string wave_name;

        public List<EnemyInfo> enemies;
        public float waitTime;

        public float spawnRange;
    }

    public List<WaveInfo> waves = new List<WaveInfo>();
}
