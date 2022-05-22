
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

    // 0416 - chest animation �� ���� �ӽ� ����
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
            ControllerUI.SetActive(false);
        }
        else
        {
            Player_TreasureCount_Text = MyShip.transform.Find("Canvas").transform.Find("Count_Text").GetComponent<TextMeshProUGUI>();

            PV.RPC("UI_initialize", RpcTarget.AllBuffered, MyShip.photonView.ViewID);

            CombatManager.instance.EquipSpecialCannon(0, (int)SpecialCannon.SpecialCannonType.KnockBack);
        }

        VC_Top.GetComponent<CinemachineVirtualCamera>().LookAt = TreasureSpawner_Object.transform.GetChild(0).transform;
        VC_Top.GetComponent<CinemachineVirtualCamera>().Follow = TreasureSpawner_Object.transform.GetChild(0).transform;

    }

    [PunRPC]
    public void UI_initialize(int ViewID)
    {
        Player_Controller_Ship ship = PhotonView.Find(ViewID).GetComponent<Player_Controller_Ship>();

        ship.MyShip_Canvas.transform.Find("HealthArea").gameObject.SetActive(false);
        ship.MyShip_Canvas.transform.Find("Health").gameObject.SetActive(false);
        ship.Count_Text.gameObject.SetActive(true);
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

    public void Update_TreasureCount(int _viewID, int score)
    {
        Debug.LogError("UpdateText");

        Player_TreasureCount_Text.text = Player_TreasureCount_Value.ToString();

        PV.RPC("Set_Count", RpcTarget.AllBuffered, new object[] { Player_TreasureCount_Value, _viewID });

        RoomData.GetInstance().SetCurrScore(PhotonView.Find(_viewID).OwnerActorNr, score);
    }


    /// <summary>
    /// �ٸ� ��� �ε��� ��� ��� ���� ��
    /// </summary>
    public void DropAllTreasure()
    {
        List<int> TreasureScore = new List<int>();
        int Count_tmp = (int)(Player_TreasureCount_Value / 2);
        int DropValue = Count_tmp;
        Count_tmp /= 5;
        while (Count_tmp != 0)
        {
            for(int i = Random.Range(1, 4); i >= 1; i--)
            {
                if(Count_tmp % i == 0)
                {
                    Count_tmp -= i;
                    TreasureScore.Add(i);
                    break;
                }
            }
        }

        for (int i = 0; i < TreasureScore.Count; i++)
        {
            Vector3 randomPos = MyShip.transform.position + new Vector3(Random.Range(-7, 7), 0, Random.Range(-7, 7));

            GameObject go = PhotonNetwork.Instantiate("Treasure", MyShip.transform.position + new Vector3(0, 1, 0), Quaternion.identity);

            go.GetComponent<Treasure>().photonView.RPC("SetTreasureScore", RpcTarget.AllBuffered, TreasureScore[i]);

            go.GetComponent<Treasure>().startMove(randomPos);
            //PV.RPC("TreasureMove", RpcTarget.AllBuffered, new object[] { go.GetPhotonView().ViewID, randomPos });
        }

        Player_TreasureCount_Value /= 2;
        Update_TreasureCount(MyShip.photonView.ViewID, Player_TreasureCount_Value);

        RoomData.GetInstance().GetComponent<PhotonView>().RPC("SetCurrScoreRPC",RpcTarget.AllBuffered, new object[] { MyShip.photonView.OwnerActorNr, DropValue });
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
                treasureSpawn_Time = 1.0f - 0.1f * (TreasureSpawnCount / 10); // �ð��� �������� ���� ������ ����
            }

            GameObject go = PhotonNetwork.Instantiate("Treasure", TreasureSpawner_Object.transform.position + new Vector3(0, 2, 0), Quaternion.identity);

            int randScore = Random.Range(0, go.GetComponent<Treasure>().GemMats.Length) + 1;

            go.GetComponent<Treasure>().photonView.RPC("SetTreasureScore", RpcTarget.AllBuffered, randScore);
            go.GetComponent<Treasure>().startMove(NetworkManager.instance.CalculateSpawnPos());
            Vector3 pos = NetworkManager.instance.CalculateSpawnPos();

            PV.RPC("ChestAnimation", RpcTarget.AllBuffered);

            TreasureSpawnCount++;

            yield return new WaitForSeconds(treasureSpawn_Time); 
        }
    }
}
