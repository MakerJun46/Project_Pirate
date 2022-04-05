using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Treasure_GameManager : GameManager
{


    private float treasureSpawn_Time;

    void Start()
    {
        base.Start();
        treasureSpawn_Time = 1.0f;
    }

    void Update()
    {
        base.Update();
    }

    IEnumerator TreasureSpawner()
    {
        while(currPlayTime < maxPlayTime)
        {
            yield return new WaitForSeconds(treasureSpawn_Time);
            //treasureSpawn_Time -= 
        }
    }
}
