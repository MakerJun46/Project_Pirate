using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreasureSpawner : MonoBehaviour
{
    void Start()
    {
        
    }

    void Update()
    {
        
    }


    public Vector3 RandomPosition()
    {
        return NetworkManager.instance.CalculateSpawnPos();
    }
}
