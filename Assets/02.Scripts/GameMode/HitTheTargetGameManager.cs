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


    [SerializeField] GameObject LevelUpPanel;
    [SerializeField] Transform LevelUpBtnContainer;
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

        LevelUpPanel.SetActive(true);

        if (PhotonNetwork.IsMasterClient == false)
        {
            List<Vector2> randomRoullet = new List<Vector2>();
            Player_Combat_Ship currShip = MyShip.GetComponent<Player_Combat_Ship>();
            int spotIndex = currShip.GetLastSailIndex();
            if (spotIndex >= 0)
            {
                randomRoullet.Add(new Vector2(0, 0));
                randomRoullet.Add(new Vector2(0, 1));
            }
            spotIndex = currShip.GetLastAutoCannonIndex();
            if (spotIndex >= 0)
            {
                randomRoullet.Add(new Vector2(1, 0));
                randomRoullet.Add(new Vector2(1, 1));
                randomRoullet.Add(new Vector2(1, 2));
                randomRoullet.Add(new Vector2(1, 3));
            }
            spotIndex = currShip.GetLastmySpecialCannonsIndex();
            if (spotIndex >= 0)
            {
                randomRoullet.Add(new Vector2(2, 0));
                randomRoullet.Add(new Vector2(2, 1));
                randomRoullet.Add(new Vector2(2, 2));
                randomRoullet.Add(new Vector2(2, 3));
            }
            if (MyShip.upgradeIndex <= 1)
            {
                randomRoullet.Add(new Vector2(3, 0));
            }


            for (int i = 0; i < LevelUpBtnContainer.childCount; i++)
            {
                GameObject levelUpBtn = LevelUpBtnContainer.GetChild(i).gameObject;


                if (randomRoullet.Count > 0)
                {
                    int selectIndex = Random.Range(0, randomRoullet.Count);
                    Vector2 selectedRoullet = randomRoullet[selectIndex];
                    randomRoullet.RemoveAt(selectIndex);

                    levelUpBtn.SetActive(true);
                    levelUpBtn.GetComponentInChildren<Text>().text = "";
                    levelUpBtn.GetComponent<Button>().onClick.RemoveAllListeners();


                    int levelUpIndex = (int)selectedRoullet.y;

                    switch ((int)selectedRoullet.x)
                    {
                        case 0:
                            levelUpBtn.GetComponentInChildren<Text>().text = "Get Sail " + levelUpIndex;
                            levelUpBtn.GetComponent<Button>().onClick.AddListener(() => CombatManager.instance.EquipSail(currShip.GetLastSailIndex(), levelUpIndex));
                            break;
                        case 1:
                            levelUpBtn.GetComponentInChildren<Text>().text = "Get Cannon " + levelUpIndex;
                            levelUpBtn.GetComponent<Button>().onClick.AddListener(() => CombatManager.instance.EquipCannon(currShip.GetLastAutoCannonIndex(), levelUpIndex));
                            break;
                        case 2:
                            levelUpBtn.GetComponentInChildren<Text>().text = "Get SpecialCannon " + levelUpIndex;
                            levelUpBtn.GetComponent<Button>().onClick.AddListener(() => CombatManager.instance.EquipSpecialCannon(currShip.GetLastmySpecialCannonsIndex(), levelUpIndex));
                            break;
                        case 3:
                            levelUpBtn.GetComponentInChildren<Text>().text = "Upgrade";
                            levelUpBtn.GetComponent<Button>().onClick.AddListener(() => TryUpgradeShip());
                            break;
                    }
                    levelUpBtn.GetComponent<Button>().onClick.AddListener(() => LevelUpPanel.SetActive(false));
                }
                else
                {
                    levelUpBtn.SetActive(false);
                }
            }
        }
    }

    public override void JudgeWinLose()
    {
        int rank= RoomData.GetInstance().GetPlayerCurrentRank(PhotonNetwork.LocalPlayer.ActorNumber);
        IsWinner = rank <= 0 ? true : false;
        base.JudgeWinLose();
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
                FindObjectOfType<NetworkManager>().StartEndGame(false);
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
