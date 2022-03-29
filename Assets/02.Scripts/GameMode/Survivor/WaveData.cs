using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WaveData")]
public class WaveData : ScriptableObject
{
    [System.Serializable]
    public class WaveInfo
    {
        [HideInInspector]
        public string wave_name;

        public List<EnemyInfo> enemies;
        public float waitTime;

        public float spawnRange;

    }

    public List<WaveInfo> waves = new List<WaveInfo>();

    private void OnValidate()
    {
        for(int i = 0; i < waves.Count; i++)
        {
            waves[i].wave_name = "";
            for (int j = 0; j < waves[i].enemies.Count; j++)
            {
                waves[i].wave_name += "[" + waves[i].enemies[j].EnemyPrefab.name + "*" + waves[i].enemies[j].count + "]";
            }
        }
    }
}
