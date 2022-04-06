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
    private bool SpawnStart;

    public int Player_TreasureCount_Value;
    public TextMeshProUGUI Player_TreasureCount_Text;

    void Start()
    {
        base.Start();
        instance = this;
        treasureSpawn_Time = 1.0f;
        SpawnStart = false;
        PV = GetComponent<PhotonView>();

        Player_TreasureCount_Value = 0;
    }

    void Update()
    {
        base.Update();
        if(!SpawnStart && GameManager.GetInstance().MyShip != null)
        {
            initialize();
        }
    }

    public void initialize()
    {
        VC_Top.GetComponent<CinemachineVirtualCamera>().LookAt = TreasureSpawner_Object.transform;
        VC_Top.GetComponent<CinemachineVirtualCamera>().Follow = TreasureSpawner_Object.transform;

        Player_TreasureCount_Text = MyShip.transform.Find("Canvas").transform.Find("Count_Text").GetComponent<TextMeshProUGUI>();

        StartCoroutine(TreasureSpawner());
        SpawnStart = true;
    }

    public void Update_TreasureCount()
    {
        Player_TreasureCount_Text.text = Player_TreasureCount_Value.ToString();

        PV.RPC("Set_Count", RpcTarget.AllBuffered, new object[] { Player_TreasureCount_Value, MyShip.photonView.ViewID });
    }

    [PunRPC]
    public void Set_Count(int value, int ViewID)
    {
        PhotonView.Find(ViewID).GetComponent<Player_Controller_Ship>().Count_Text.text = value.ToString();
    }

    public override void StartGame()
    {
        base.StartGame();
        if(PhotonNetwork.IsMasterClient)
        {
            Debug.Log("SpawnStart");
            StartCoroutine(TreasureSpawner());
        }

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
