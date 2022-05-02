using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class HitTheTargetGameManager : GameManager
{
    [SerializeField] float targetSpawnDistance=50;
    int waveIndex = 0;
    [SerializeField] List<int> targetCounts;

    protected override void Start()
    {
        base.Start();
        if (PhotonNetwork.IsMasterClient || PhotonNetwork.IsConnected == false)
        {
            Invoke("WaveStart", 3f);
        }
    }

    public override void StartGame()
    {
        base.StartGame();

        CombatManager.instance.SetLevelUpCount(3);
    }

    private void WaveStart()
    {
        StartCoroutine("WaveSpawnCoroutine");
    }
    IEnumerator WaveSpawnCoroutine()
    {
        for (int j = 0; j < targetCounts[waveIndex]; j++)
        {
            Vector2 spawnPoint = Random.insideUnitCircle * targetSpawnDistance;
            GameObject tmpTarget = PhotonNetwork.Instantiate("ScoreTarget_" + Random.Range(0, 3),new Vector3(spawnPoint.x,0, spawnPoint.y) , Quaternion.identity);
            yield return new WaitForSeconds(0.1f);
        }
        yield return new WaitForSeconds(3f);

        if(targetCounts.Count-1 > waveIndex)
        {
            waveIndex++;
        }

        if (GameStarted)
            StartCoroutine("WaveSpawnCoroutine");
    }

    public override void JudgeWinLose()
    {
        int rank = RoomData.GetInstance().GetPlayerCurrentRank(PhotonNetwork.LocalPlayer.ActorNumber);
        IsWinner = rank <= 0 ? true : false;
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
                if (count <= 1) shouldGameEnd = true;
            }

            if (shouldGameEnd)
            {
                FindObjectOfType<NetworkManager>().EndGame();
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
