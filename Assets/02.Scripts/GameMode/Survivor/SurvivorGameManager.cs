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
    int waveIndex = 0;

    float scoreTime;

    [SerializeField] WaveData waveData;

    public override void StartGame()
    {
        base.StartGame();

        if (PhotonNetwork.IsMasterClient || PhotonNetwork.IsConnected == false)
        {
            Invoke("WaveStart", 1f);
        }
    }

    protected override void Start()
    {
        base.Start();
    }

    private void WaveStart()
    {
        StartCoroutine("WaveSpawnCoroutine");
        StartCoroutine("NaturalDisastersCoroutine");
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

    IEnumerator NaturalDisastersCoroutine()
    {
        int spawnCount = Random.Range(3, 6);
        for (int i = 0; i < spawnCount; i++)
        {
            GameObject tmp = PhotonNetwork.Instantiate("CannonBall_Rain", new Vector3(Random.Range(-40,40),50f, Random.Range(-40,40)), Quaternion.identity,
                0, new object[] { 25.0f,3f });
            tmp.GetComponent<CannonBall>().gravity = Vector3.up * -9.8f * 4f;
            yield return new WaitForSeconds(Random.Range(0.1f,.5f));
        }
        yield return new WaitForSeconds(Random.Range(1f,2f));
        if (GameStarted)
            StartCoroutine("NaturalDisastersCoroutine");
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
            bool shouldGameEnd = false;

            if (currPlayTime >= maxPlayTime)
            {
                shouldGameEnd = true;
            }
            else
            {
                int count = 0;
                for (int i = 0; i < AllShip.Count; i++)
                    if (AllShip[i] != null && AllShip[i].GetComponent<Player_Combat_Ship>().health > 0)
                        count++;
                if (count < 1) shouldGameEnd = true;
            }

            if (shouldGameEnd)
            {
                FindObjectOfType<NetworkManager>().EndGame();
            }

            scoreTime += Time.deltaTime;
            if (scoreTime>=1 && PhotonNetwork.IsMasterClient == false)
            {
                scoreTime -= 1;
                RoomData.GetInstance().SetCurrScore(PhotonNetwork.LocalPlayer.ActorNumber, 10);
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
