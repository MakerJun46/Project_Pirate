using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using Cinemachine;

public class PassTheBombGameManager : GameManager
{
    float scoreTime;

    private PhotonView PV;
    public TextMeshProUGUI bomb_Second;

    public GameObject LoadingPanel;

    public GameObject statue;

    public Cinemachine.CinemachineVirtualCamera VC_Bomb;

    List<AttackInfo> AttackIDs = new List<AttackInfo>();
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
            IsWinner = !MyShip.GetComponent<Player_Combat_Ship>().hasBomb;
            Debug.Log(PhotonNetwork.LocalPlayer.NickName + " ��ź ������ �й� !!"); // game Over Scene
            if (MyShip.GetComponent<Player_Combat_Ship>().hasBomb)
                PV.RPC("Bomb_Explode", RpcTarget.AllBuffered, MyShip.photonView.ViewID);
            MyShip.GetComponent<Player_Combat_Ship>().hasBomb = false;
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
        if (PhotonNetwork.IsMasterClient)
        {
            int randomPlayerIndex = AllShip[Random.Range(0, AllShip.Count)].GetComponent<PhotonView>().OwnerActorNr;
            print("BOMB : " + randomPlayerIndex);
            PV.RPC("FirstHasBomb", RpcTarget.AllBuffered, randomPlayerIndex);

            if (!GameManager.isObserver) // �������� �ƴϸ� �굵 ���� ����
                InitializeGame();
        }
        else
        {
            InitializeGame();
        }
    }

    public void InitializeGame()
    {
        bomb_Second = MyShip.transform.Find("Canvas").transform.Find("Count_Text").GetComponent<TextMeshProUGUI>();

        for (int i = 0; i < AllShip.Count; i++)
        {
            Transform canvas = AllShip[i].gameObject.transform.Find("Canvas");
            canvas.Find("HealthArea").gameObject.SetActive(false);
            canvas.Find("Health").gameObject.SetActive(false);
            canvas.Find("Count_Text").gameObject.SetActive(false);
        }
        //TryUpgradeShip();
        CombatManager.instance.EquipSail();
        CombatManager.instance.EquipSpecialCannon(0, 0);

        //VC_Top.GetComponent<CinemachineVirtualCamera>().LookAt = statue.transform;

        MyShip.MoveSpeed = 20;
    }


    protected override void Update()
    {
        base.Update();

        if (currPlayTime <= maxPlayTime)
        {
            for (int i = 0; i < AllShip.Count; i++)
            {
                if (AllShip[i] != null)
                    AllShip[i].transform.Find("Canvas").transform.Find("Count_Text").GetComponent<TextMeshProUGUI>().text = (maxPlayTime - (int)currPlayTime).ToString();
            }
        }

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

            if(MyShip.GetComponent<Player_Combat_Ship>().hasBomb ==false)
                scoreTime += Time.deltaTime;
            if (scoreTime >= 1 && !GameManager.isObserver) // �������� ���� ���
            {
                scoreTime -= 1;
                RoomData.GetInstance().SetCurrScore(PhotonNetwork.LocalPlayer.ActorNumber, 10);
            }
            else if(scoreTime >= 1 && (GameManager.isObserver && PhotonNetwork.IsMasterClient == false)) // �������� �ִ� ��� ������ �����ϰ� ���� �߰�
            {
                scoreTime -= 1;
                RoomData.GetInstance().SetCurrScore(PhotonNetwork.LocalPlayer.ActorNumber, 10);
            }
        }
    }

    [PunRPC]
    public void Bomb_Explode(int ViewID)
    {
        if(GameManager.isObserver && PhotonNetwork.IsMasterClient)
        {
            UI_Observer.transform.GetChild(0).gameObject.SetActive(false);
            UI_Observer.transform.GetChild(1).gameObject.SetActive(false);
        }
        else
        {
            MyShip.Ship_Stop();
        }
        StartCoroutine(Explosion(ViewID));
    }

    IEnumerator Explosion(int ViewID)
    {
        VC_Bomb.Priority = 15;
        yield return new WaitForSeconds(1.0f);

        // Explosion VFX
        PhotonView.Find(ViewID).transform.Find("PassTheBomb").GetChild(1).gameObject.SetActive(true);
        // Fire VFX
        PhotonView.Find(ViewID).transform.Find("PassTheBomb").GetChild(0).gameObject.SetActive(false);
    }


    [PunRPC]
    public void FirstHasBomb(int PlayerIndex)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            print("FirstHasBomb :" + PlayerIndex + "  // " + MyShip.GetComponent<PhotonView>().OwnerActorNr);
            if (PlayerIndex == MyShip.GetComponent<PhotonView>().OwnerActorNr)
            {
                MyShip.GetComponent<Player_Combat_Ship>().hasBomb = true;
                if (PV == null)
                {
                    PV = GetComponent<PhotonView>();
                }
                PV.RPC("On_Second", RpcTarget.AllBuffered, MyShip.photonView.ViewID);
                PV.RPC("Change_VC_Lookat", RpcTarget.AllBuffered, MyShip.photonView.ViewID);
            }
            else
            {
                MyShip.GetComponent<Player_Combat_Ship>().hasBomb = false;
            }
        }
    }

    [PunRPC]
    public void Change_VC_Lookat(int ViewID)
    {
        VC_Bomb.LookAt = PhotonView.Find(ViewID).gameObject.transform;
        VC_Bomb.Follow = PhotonView.Find(ViewID).gameObject.transform;
    }

    [PunRPC]
    public void Off_Second(int ViewID)
    {
        PhotonView.Find(ViewID).gameObject.transform.Find("Canvas").transform.Find("Count_Text").gameObject.SetActive(false);
        PhotonView.Find(ViewID).gameObject.transform.Find("PassTheBomb").gameObject.SetActive(false);
    }

    [PunRPC]
    public void On_Second(int ViewID)
    {
        PhotonView.Find(ViewID).gameObject.transform.Find("Canvas").transform.Find("Count_Text").gameObject.SetActive(true);
        PhotonView.Find(ViewID).gameObject.transform.Find("PassTheBomb").gameObject.SetActive(true);
    }

    public void CrashOtherShip(GameObject attacker,GameObject CrashedShip)
    {
        if (MyShip.photonView.ViewID != CrashedShip.GetPhotonView().ViewID && attacker.GetPhotonView().ViewID== MyShip.photonView.ViewID)
        {
            print(MyShip.photonView.Owner.NickName + " with " + CrashedShip.GetPhotonView().Owner.NickName);
            PV.RPC("change_has_bomb_new", RpcTarget.AllBuffered, new object[] { MyShip.photonView.ViewID , CrashedShip.GetPhotonView().ViewID });
        }
    }

    [PunRPC]
    public void change_has_bomb_new(int FromViewID, int toViewID)
    {
        bool canAttack = false;
        if (AttackIDs.Find(s => s.id == FromViewID) == null && AttackIDs.Find(s => s.id == toViewID) == null)
        {
            AttackIDs.Add(new AttackInfo(FromViewID, 0.2f));
            AttackIDs.Add(new AttackInfo(toViewID, 0.2f));
            canAttack = true;
        }
        if (canAttack)
        {
        }

        PV.RPC("Off_Second", RpcTarget.AllBuffered, FromViewID);
        PV.RPC("On_Second", RpcTarget.AllBuffered, toViewID);

        GameObject from_Ship = PhotonView.Find(FromViewID).gameObject;
        GameObject to_Ship = PhotonView.Find(toViewID).gameObject;
        print("from " + PhotonView.Find(FromViewID).Owner.NickName + " to " + PhotonView.Find(toViewID).Owner.NickName);

        /*
        if (from_Ship.GetPhotonView().IsMine)
        {
        }
        if (to_Ship.GetPhotonView().IsMine)
        {
            to_Ship.GetComponent<Player_Combat_Ship>().hasBomb = true;
        }
        */

        from_Ship.GetComponent<Player_Combat_Ship>().hasBomb = false;
        to_Ship.GetComponent<Player_Combat_Ship>().hasBomb = true;
        Change_VC_Lookat(toViewID);
    }

}
