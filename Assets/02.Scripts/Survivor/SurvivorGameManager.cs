using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public struct EnemyInfo
{
    public GameObject EnemyPrefab;
    public int count;

    public float additionalDamage;
    public float additionalVel;
    public float additionalHealth;
    public bool dontFollowTarget;

    public bool IsElite;
}


public class SurvivorGameManager : GameManager
{

    public int DeathCount;
    public Text DeathText;

    int waveIndex = 0;


    [SerializeField] WaveData waveData;
    void Start()
    {
        DeathCount = 0;

        if (Photon.Pun.PhotonNetwork.IsMasterClient || Photon.Pun.PhotonNetwork.IsConnected==false)
        {
            Invoke("WaveStart", 5f);
        }

        StartCoroutine("PickupGenerateCoroutine");

    }
    IEnumerator PickupGenerateCoroutine()
    {
        yield return new WaitForSeconds(Random.Range(15f, 30f));

        float rand = Random.Range(0f, 100f);
        int index = 1;
        if (rand <= 5f)
        {
            index = 3;
        }
        else if (rand <= 30f)
        {
            index = 2;
        }

        float pickUpIndex = Random.Range(0, 100f);
        string tmpPickUpString = "Pickup_Treasure";

        GameObject tmpObj = ObjectPoolManager.GetObject(tmpPickUpString);
        tmpObj.transform.position = new Vector3(Random.Range(-1, 1f), 0f, Random.Range(-1, 1f)).normalized * 20f;
        //tmpObj.GetComponent<Pickup>().InitializePickup();

        StartCoroutine("PickupGenerateCoroutine");
    }

    private void WaveStart()
    {
        StartCoroutine("WaveSpawnCoroutine");
    }
    public override void MasterChanged(bool _isMaster)
    {
        base.MasterChanged(_isMaster);
        if (_isMaster)
        {
            WaveStart();
        }
    }

    IEnumerator WaveSpawnCoroutine()
    {
        Debug.Log("[Wave " + waveIndex + "] :< color=red>" + waveData.waves[waveIndex].wave_name + "</color>");
        for (int j = 0; j < waveData.waves[waveIndex].enemies.Count; j++)
        {
            for (int k = 0; k < waveData.waves[waveIndex].enemies[j].count; k++)
            {
                GameObject tmpEnemy = ObjectPoolManager.GetObject(waveData.waves[waveIndex].enemies[j].EnemyPrefab.name);
                tmpEnemy.transform.position = new Vector3(Random.Range(-1, 1f), 0f, Random.Range(-1, 1f)).normalized * 15f;

                tmpEnemy.GetComponent<SurvivorMonster>().ResetEnemy(waveData.waves[waveIndex].enemies[j].EnemyPrefab.GetComponent<SurvivorMonster>());
                tmpEnemy.GetComponent<SurvivorMonster>().InitializeEnemy(
                    waveData.waves[waveIndex].enemies[j].additionalDamage,
                    waveData.waves[waveIndex].enemies[j].additionalVel,
                    waveData.waves[waveIndex].enemies[j].additionalHealth,
                    !waveData.waves[waveIndex].enemies[j].dontFollowTarget,
                    waveData.waves[waveIndex].enemies[j].IsElite);
                yield return new WaitForSeconds(0.1f);
            }
        }
        yield return new WaitForSeconds(waveData.waves[waveIndex].waitTime);

        waveIndex++;
        if (waveData.waves.Count <= waveIndex)
        {
            waveIndex = waveData.waves.Count - 1;
        }

        if(GameStart)
            StartCoroutine("WaveSpawnCoroutine");
    }
    public override void EndGame(bool _win)
    {
        base.EndGame(_win);
        print("End : " + _win);
        GameStart = false;
    }


    protected override void Update()
    {
        base.Update();
        
        if (GameStart)
        {
            if (playTime >= 60)
            {
                if (MyShip == null || MyShip.GetComponent<Player_Combat_Ship>().health <= 0)
                {
                    EndGame(false);
                }
                else
                {
                    EndGame(true);
                }
            }
            else
            {
                playTime += Time.deltaTime;

                int count = 0;
                int index = -1;
                for (int i = 0; i < AllShip.Count; i++)
                {
                    if (AllShip[i] != null && AllShip[i].GetComponent<Player_Combat_Ship>().health > 0)
                    {
                        count++;
                        index = i;
                    }
                }
                if (count <= 1)
                {
                    if (index >= 0 && index < AllShip.Count && AllShip[index] == MyShip)
                    {
                        EndGame(true);
                    }
                    else
                    {
                        EndGame(false);
                    }
                }
            }
        }
        TimeText.text = ((int)(playTime / 60)) + ":" + ((int)(playTime % 60));
    }
}
