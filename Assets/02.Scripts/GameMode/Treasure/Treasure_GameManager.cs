using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Cinemachine;
using UnityEngine.UI;
using TMPro;

public class Treasure_GameManager : GameManager
{
    public static Treasure_GameManager instance;

    [SerializeField] GameObject TreasureSpawner_Object;

    PhotonView PV;

    private float treasureSpawn_Time;

    public int Player_TreasureCount_Value;
    public TextMeshProUGUI Player_TreasureCount_Text;

    protected override void Start()
    {
        base.Start();
        instance = this;
        treasureSpawn_Time = 1.0f;

        PV = GetComponent<PhotonView>();

        Player_TreasureCount_Value = 0;
    }

    public override void StartGame()
    {
        base.StartGame();
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("SpawnStart");
            StartCoroutine(TreasureSpawner());
        }
        else
        {
            VC_Top.GetComponent<CinemachineVirtualCamera>().LookAt = TreasureSpawner_Object.transform;
            VC_Top.GetComponent<CinemachineVirtualCamera>().Follow = TreasureSpawner_Object.transform;

            Player_TreasureCount_Text = MyShip.transform.Find("Canvas").transform.Find("Count_Text").GetComponent<TextMeshProUGUI>();
        }
    }

    public override void MasterChanged(bool _isMaster)
    {
        base.MasterChanged(_isMaster);
        if (_isMaster)
        {
            StartCoroutine(TreasureSpawner());
        }
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

    public void Update_TreasureCount(int _viewID)
    {
        Player_TreasureCount_Value++;
        Player_TreasureCount_Text.text = Player_TreasureCount_Value.ToString();

        RoomData.GetInstance().SetCurrScore(PhotonView.Find(_viewID).OwnerActorNr, 1);
        PV.RPC("Set_Count", RpcTarget.AllBuffered, new object[] { Player_TreasureCount_Value, _viewID });
    }

    [PunRPC]
    public void Set_Count(int value, int ViewID)
    {
        PhotonView.Find(ViewID).GetComponent<Player_Controller_Ship>().Count_Text.text = value.ToString();
    }

    IEnumerator TreasureSpawner()
    {
        int TreasureSpawnCount = 0;

        while(currPlayTime < maxPlayTime)
        {
            if (TreasureSpawnCount % 20 == 0)
            {
                treasureSpawn_Time = 1.0f - 0.1f * (TreasureSpawnCount / 10); // 시간이 지날수록 점점 빠르게 생성
            }

            Debug.Log("Spawn");
            PhotonNetwork.Instantiate("Treasure", NetworkManager.instance.CalculateSpawnPos(), Quaternion.identity);
            TreasureSpawnCount++;

            yield return new WaitForSeconds(treasureSpawn_Time); 
        }
    }
}
