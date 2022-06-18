using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Cinemachine;

public class GhostShipGameManager : GameManager
{
    float scoreTime;

    private PhotonView PV;

    public GameObject LoadingPanel;

    public Cinemachine.CinemachineVirtualCamera VC_Bomb;

    List<AttackInfo> AttackIDs = new List<AttackInfo>();

    [SerializeField] List<GhostShip> ghostShips;
    [SerializeField] float GhostSpawnRadius=10f;

    protected override void Start()
    {
        base.Start();

        PV = GetComponent<PhotonView>();

    }
    public override void MasterChanged(bool _isMaster)
    {
        base.MasterChanged(_isMaster);
    }
    public override void JudgeWinLose()
    {
        if (MyShip)
        {
            IsWinner = !MyShip.GetComponent<Player_Combat_Ship>().isTagger;
            Debug.Log(PhotonNetwork.LocalPlayer.NickName + " 유령이 되어 패배 !!"); // game Over Scene
            if (MyShip.GetComponent<Player_Combat_Ship>().isTagger)
                PV.RPC("GhostDIedRPC", RpcTarget.AllBuffered, MyShip.photonView.ViewID);
            MyShip.GetComponent<Player_Combat_Ship>().isTagger = false;
        }
        base.JudgeWinLose();
    }

    public override void SetObserverCamera()
    {
        base.SetObserverCamera();

        for (int i = 0; i < AllShip.Count; i++)
        {
            Transform canvas = AllShip[i].gameObject.transform.Find("Canvas");
            canvas.Find("HealthArea").gameObject.SetActive(false);
            canvas.Find("Health").gameObject.SetActive(false);
            canvas.Find("Count_Text").gameObject.SetActive(false);
        }
    }

    public override void StartGame()
    {
        base.StartGame();
        if (PhotonNetwork.IsMasterClient || PhotonNetwork.IsConnected == false)
        {
            for (int i = 0; i < 3; i++)
            {
                GameObject tmpEnemy = PhotonNetwork.Instantiate("GhostShip", new Vector3(Random.Range(-1, 1f), 0f, Random.Range(-1, 1f)).normalized * GhostSpawnRadius, Quaternion.identity);
            }
        }

        if (GameManager.isObserver)
        {
            if (PhotonNetwork.IsMasterClient == false)
            {
                InitializeGame(); // Observer모드인 경우 masterClient(옵저버)제외하고 initializeGame()
            }
        }
        else
        {
            InitializeGame();
        }
    }

    public void InitializeGame()
    {
        for (int i = 0; i < AllShip.Count; i++)
        {
            Transform canvas = AllShip[i].gameObject.transform.Find("Canvas");
            canvas.Find("HealthArea").gameObject.SetActive(false);
            canvas.Find("Health").gameObject.SetActive(false);
            canvas.Find("Count_Text").gameObject.SetActive(false);
        }
        CombatManager.instance.EquipSail();
        CombatManager.instance.EquipSpecialCannon(0, 0);

        MyShip.MoveSpeed = 20;
    }


    protected override void Update()
    {
        base.Update();

        for (int i = AttackIDs.Count - 1; i >= 0; i--)
        {
            AttackIDs[i].lifetime -= Time.deltaTime;
            if (AttackIDs[i].lifetime <= 0)
                AttackIDs.RemoveAt(i);
        }

        if (GameStarted)
        {
            if (currPlayTime >= maxPlayTime)
            {
                FindObjectOfType<NetworkManager>().EndGame();
            }
            else
            {
                int taggedPlayer = 0;
                Player_Combat_Ship[] ships = FindObjectsOfType<Player_Combat_Ship>();
                for(int i = 0; i < ships.Length; i++)
                {
                    if (ships[i].isTagger)
                        taggedPlayer++;
                }

                if (taggedPlayer >= ships.Length)
                {
                    FindObjectOfType<NetworkManager>().EndGame();
                }
            }

            if (MyShip.GetComponent<Player_Combat_Ship>().isTagger == false)
                scoreTime += Time.deltaTime;

            if (scoreTime >= 1 && !GameManager.isObserver) // 옵저버가 없는 경우
            {
                scoreTime -= 1;
                RoomData.GetInstance().SetCurrScore(PhotonNetwork.LocalPlayer.ActorNumber, 10);
            }
            else if (scoreTime >= 1 && (GameManager.isObserver && PhotonNetwork.IsMasterClient == false)) // 옵저버가 있는 경우 옵저버 제외하고 점수 추가
            {
                scoreTime -= 1;
                RoomData.GetInstance().SetCurrScore(PhotonNetwork.LocalPlayer.ActorNumber, 10);
            }
        }
    }

    [PunRPC]
    public void GhostDIedRPC(int ViewID)
    {
        if ((GameManager.isObserver && PhotonNetwork.IsMasterClient))
        {
            UI_Observer.transform.GetChild(0).gameObject.SetActive(false);
            UI_Observer.transform.GetChild(1).gameObject.SetActive(false);
        }
        else
        {
            MyShip.Ship_Stop();
        }
    }


    [PunRPC]
    public void Infection(int PlayerId)
    {
        PV.RPC("On_Second", RpcTarget.AllBuffered, PlayerId);
        GameObject to_Ship = PhotonView.Find(PlayerId).gameObject;

        to_Ship.GetComponent<Player_Combat_Ship>().isTagger = true;
    }



    [PunRPC]
    public void On_Second(int ViewID)
    {
        PhotonView.Find(ViewID).gameObject.GetComponent<Player_Combat_Ship>().SetToGhost();
    }

    public void CrashOtherShip(GameObject attacker, GameObject CrashedShip)
    {
        if (MyShip.photonView.ViewID != CrashedShip.GetPhotonView().ViewID && attacker.GetPhotonView().ViewID == MyShip.photonView.ViewID)
        {
            print(MyShip.photonView.Owner.NickName + " with " + CrashedShip.GetPhotonView().Owner.NickName);
            PV.RPC("GhostInfectionRPC", RpcTarget.AllBuffered, new object[] { MyShip.photonView.ViewID, CrashedShip.GetPhotonView().ViewID });
        }
    }

    [PunRPC]
    public void GhostInfectionRPC(int FromViewID, int toViewID)
    {
        if (FromViewID > 0)
            RoomData.GetInstance().SetCurrScore(PhotonView.Find(FromViewID).GetComponent<PhotonView>().Owner.ActorNumber, 100);
        PV.RPC("On_Second", RpcTarget.AllBuffered, toViewID);

        GameObject to_Ship = PhotonView.Find(toViewID).gameObject;

        //if (to_Ship.GetPhotonView().IsMine)
        to_Ship.GetComponent<Player_Combat_Ship>().isTagger = true;
    }
}
