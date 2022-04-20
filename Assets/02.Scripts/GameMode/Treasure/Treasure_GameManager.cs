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


    // 0416 - chest animation 을 위한 임시 변수
    public Animator chestAnimator;

    protected override void Start()
    {
        base.Start();
        instance = this;
        treasureSpawn_Time = 1.0f;
        SpawnStart = false;
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

            PV.RPC("UI_initialize", RpcTarget.AllBuffered, MyShip.photonView.ViewID);
        }
    }

    [PunRPC]
    public void UI_initialize(int ViewID)
    {
        Player_Controller_Ship ship = PhotonView.Find(ViewID).GetComponent<Player_Controller_Ship>();

        ship.MyShip_Canvas.transform.Find("HealthArea").gameObject.SetActive(false);
        ship.MyShip_Canvas.transform.Find("Health").gameObject.SetActive(false);
        ship.Count_Text.text = "0";
    }


    public void initialize()
    {

        PV.RPC("UI_initialize", RpcTarget.AllBuffered, MyShip.photonView.ViewID);

        VC_Top.GetComponent<CinemachineVirtualCamera>().LookAt = TreasureSpawner_Object.transform;
        VC_Top.GetComponent<CinemachineVirtualCamera>().Follow = TreasureSpawner_Object.transform;

        Player_TreasureCount_Text = MyShip.transform.Find("Canvas").transform.Find("Count_Text").GetComponent<TextMeshProUGUI>();

        StartCoroutine(TreasureSpawner());
        SpawnStart = true;
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
        //if(!SpawnStart && GameManager.GetInstance().MyShip != null) // 에디터에서 테스트하기 위함
        //{
        //    initialize();
        //}

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
        Debug.LogError("UpdateText");

        Player_TreasureCount_Text.text = Player_TreasureCount_Value.ToString();

        PV.RPC("Set_Count", RpcTarget.AllBuffered, new object[] { Player_TreasureCount_Value, _viewID });

        RoomData.GetInstance().SetCurrScore(PhotonView.Find(_viewID).OwnerActorNr, 1);
    }


    /// <summary>
    /// 다른 배와 부딪힌 경우 모든 보물 뱉어냄
    /// </summary>
    public void DropAllTreasure()
    {
        Debug.Log("DropTreasure");

        for (int i = 0; i < Player_TreasureCount_Value; i++)
        {
            Vector3 randomPos = MyShip.transform.position + new Vector3(Random.Range(-7, 7), 0, Random.Range(-7, 7));

            GameObject go = PhotonNetwork.Instantiate("Treasure", MyShip.transform.position + new Vector3(0, 1, 0), Quaternion.identity);

            PV.RPC("TreasureMove", RpcTarget.AllBuffered, new object[] { go.GetPhotonView().ViewID, randomPos });
        }

        Player_TreasureCount_Value = 0;
        Update_TreasureCount(MyShip.photonView.ViewID);
    }

    [PunRPC]
    public void Set_Count(int value, int ViewID)
    {
        Debug.Log("SetCount");
        PhotonView.Find(ViewID).GetComponent<Player_Controller_Ship>().Count_Text.text = value.ToString();
    }

    public override void JudgeWinLose()
    {
        int rank = RoomData.GetInstance().GetPlayerCurrentRank(PhotonNetwork.LocalPlayer.ActorNumber);
        IsWinner = rank <= 0 ? true : false;
        base.JudgeWinLose();
    }

    [PunRPC]
    public void ChestAnimation()
    {
        chestAnimator.SetTrigger("Open");
    }

    [PunRPC]
    public void TreasureMove(int ViewID, Vector3 Pos)
    {
        PhotonView.Find(ViewID).GetComponent<Treasure>().startMove(Pos);
    }

    IEnumerator TreasureSpawner()
    {
        yield return new WaitForSeconds(3.0f);

        int TreasureSpawnCount = 0;

        while(currPlayTime < maxPlayTime)
        {
            if (TreasureSpawnCount % 20 == 0)
            {
                treasureSpawn_Time = 1.0f - 0.1f * (TreasureSpawnCount / 10); // 시간이 지날수록 점점 빠르게 생성
            }

            GameObject go = PhotonNetwork.Instantiate("Treasure", TreasureSpawner_Object.transform.position + new Vector3(0, 2, 0), Quaternion.identity);
            go.GetComponent<Treasure>().startMove(NetworkManager.instance.CalculateSpawnPos());
            Vector3 pos = NetworkManager.instance.CalculateSpawnPos();

            PV.RPC("ChestAnimation", RpcTarget.AllBuffered);

            TreasureSpawnCount++;

            yield return new WaitForSeconds(treasureSpawn_Time); 
        }
    }
}
