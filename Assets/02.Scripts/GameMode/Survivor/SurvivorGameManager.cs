using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

[System.Serializable]
public struct EnemyInfo
{
    public GameObject EnemyPrefab;
    public int count;

    public float additionalDamage;
    public float additionalVel;
    public float additionalHealth;
}


public class SurvivorGameManager : GameManager
{

    public int DeathCount;
    public Text DeathText;

    int waveIndex = 0;


    [SerializeField] WaveData waveData;
    protected override void Start()
    {
        base.Start();
        DeathCount = 0;

        if (PhotonNetwork.IsMasterClient || PhotonNetwork.IsConnected==false)
        {
            Invoke("WaveStart", 5f);
        }
    }

    public override void StartGame()
    {
        base.StartGame();
    }

    private void WaveStart()
    {
        StartCoroutine("WaveSpawnCoroutine");
    }

    IEnumerator WaveSpawnCoroutine()
    {
        Debug.Log("[Wave " + waveIndex + "] :< color=red>" + waveData.waves[waveIndex].wave_name + "</color>");
        for (int j = 0; j < waveData.waves[waveIndex].enemies.Count; j++)
        {
            for (int k = 0; k < waveData.waves[waveIndex].enemies[j].count; k++)
            {
                GameObject tmpEnemy = PhotonNetwork.Instantiate(waveData.waves[waveIndex].enemies[j].EnemyPrefab.name, Vector3.zero, Quaternion.identity);

                if(waveData.waves[waveIndex].enemies[j].EnemyPrefab.name != "SnakeParent")
                    tmpEnemy.transform.position = new Vector3(Random.Range(-1, 1f), 0f, Random.Range(-1, 1f)).normalized * waveData.waves[waveIndex].spawnRange;

                tmpEnemy.GetComponent<SurvivorMonster>().ResetEnemy(waveData.waves[waveIndex].enemies[j].EnemyPrefab.GetComponent<SurvivorMonster>());
                tmpEnemy.GetComponent<SurvivorMonster>().InitializeEnemy(
                    waveData.waves[waveIndex].enemies[j].additionalDamage,
                    waveData.waves[waveIndex].enemies[j].additionalVel,
                    waveData.waves[waveIndex].enemies[j].additionalHealth);
                yield return new WaitForSeconds(0.1f);

                // 10마리가 되면 새로 생성하지 않음
                yield return new WaitUntil(()=>FindObjectsOfType<SurvivorMonster>().Length <= 10);
            }
        }
        yield return new WaitForSeconds(waveData.waves[waveIndex].waitTime);


        // 다음 wave가 없으면 마지막 wave를 재사용
        waveIndex++;
        if (waveData.waves.Count <= waveIndex)
            waveIndex = waveData.waves.Count - 1;

        if (GameStarted)
            StartCoroutine("WaveSpawnCoroutine");
    }
    public override void JudgeWinLose()
    {
        List<Player_Controller_Ship> survivedShips = new List<Player_Controller_Ship>();
        for (int i = 0; i < AllShip.Count; i++)
            if (AllShip[i] != null && AllShip[i].GetComponent<Player_Combat_Ship>().health > 0)
                survivedShips.Add(AllShip[i]);

        // 마지막에 플레이어가 살아있다면 승리
        IsWinner = survivedShips.Contains(MyShip) && MyShip != null;

        base.JudgeWinLose();
    }


    protected override void Update()
    {
        base.Update();
        
        if (GameStarted)
        {
            if (currPlayTime >= maxPlayTime)
            {
                FindObjectOfType<NetworkManager>().StartEndGame(false);
            }
            else
            {
                int count = 0;
                for (int i = 0; i < AllShip.Count; i++)
                    if (AllShip[i] != null && AllShip[i].GetComponent<Player_Combat_Ship>().health > 0)
                        count++;
                if (count <= 1)
                {
                    FindObjectOfType<NetworkManager>().StartEndGame(false);
                }
            }
        }
    }


    public override void MasterChanged(bool _isMaster)
    {
        base.MasterChanged(_isMaster);
        if (_isMaster)
        {
            WaveStart();
        }
    }
}
