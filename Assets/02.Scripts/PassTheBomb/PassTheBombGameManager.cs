using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;

public class PassTheBombGameManager : GameManager
{
    public bool hasBomb;
    private PhotonView PV;
    public TextMeshProUGUI bomb_Second;

    public GameObject LoadingPanel;

    public Cinemachine.CinemachineVirtualCamera VC_Bomb;

    List<AttackInfo> AttackIDs = new List<AttackInfo>();
    protected override void Start()
    {
        base.Start();
    }
    public override void MasterChanged(bool _isMaster)
    {
        base.MasterChanged(_isMaster);
    }
    public override void JudgeWinLose(bool _win)
    {
        base.JudgeWinLose(_win);
        print("End : " + _win);
    }

    public override void StartGame()
    {
        base.StartGame();
        InitializeGame();
    }

    public void InitializeGame()
    {
        print("InitializeGame");
        bomb_Second = MyShip.transform.Find("Canvas").transform.Find("Bomb_Second").GetComponent<TextMeshProUGUI>();

        for (int i = 0; i < AllShip.Count; i++)
        {
            Transform canvas = AllShip[i].gameObject.transform.Find("Canvas");
            canvas.Find("HealthArea").gameObject.SetActive(false);
            canvas.Find("Health").gameObject.SetActive(false);
            canvas.Find("Bomb_Second").gameObject.SetActive(false);
        }

        PV = GetComponent<PhotonView>();
        if (PhotonNetwork.IsMasterClient)
        {
            int randomPlayerIndex = selectBomb();
            print("BOMB : "+ randomPlayerIndex);
            PV.RPC("FirstHasBomb", RpcTarget.AllBuffered, randomPlayerIndex);
        }

        TryUpgradeShip();
        CombatManager.instance.EquipSail(0, 1);
        CombatManager.instance.EquipSpecialCannon(0, 0);

        MyShip.MoveSpeed = 20;
    }


    protected override void Update()
    {
        base.Update();


        for(int i = 0; i < AllShip.Count; i++)
        {
            if(AllShip[i]!=null)
                AllShip[i].transform.Find("Canvas").transform.Find("Bomb_Second").GetComponent<TextMeshProUGUI>().text = (60-(int)playTime).ToString();
        }

        for (int i = AttackIDs.Count - 1; i >= 0; i--)
        {
            AttackIDs[i].lifetime -= Time.deltaTime;
            if (AttackIDs[i].lifetime <= 0)
                AttackIDs.RemoveAt(i);
        }

        if (GameStart)
        {
            if (playTime >= 60)
            {
                JudgeWinLose(!hasBomb);
                Debug.Log(PhotonNetwork.LocalPlayer.NickName + " 폭탄 소지로 패배 !!"); // game Over Scene
                if(hasBomb)
                    PV.RPC("Bomb_Explode", RpcTarget.AllBuffered, MyShip.photonView.ViewID);
                hasBomb = false;
                FindObjectOfType<NetworkManager>().StartEndGame(false);
            }
            else
            {
                int count = 0;
                int index = -1;
                for (int i = 0; i < AllShip.Count; i++)
                {
                    if (AllShip[i] != null && AllShip[i].GetComponent<Player_Combat_Ship>().health > 0)
                    {
                        count++;
                        index = i;
                    }
                }
                if (count <= 1)
                {
                    if (index >= 0 && index < AllShip.Count && AllShip[index] == MyShip)
                    {
                        JudgeWinLose(true);
                    }
                    else
                    {
                        JudgeWinLose(false);
                    }
                }
            }
        }
    }

    [PunRPC]
    public void Bomb_Explode(int ViewID)
    {
        MyShip.Ship_Stop();
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

    private int selectBomb()
    {
        Debug.Log("플레이어 수 : " + RoomPlayerCount.playerCount);
        //return random.Next(RoomPlayerCount.playerCount);
        return AllShip[Random.Range(0, AllShip.Count)].GetComponent<PhotonView>().OwnerActorNr;
    }

    [PunRPC]
    public void FirstHasBomb(int PlayerIndex)
    {
        print("FirstHasBomb :"+PlayerIndex + "  // "+ MyShip.GetComponent<PhotonView>().OwnerActorNr);
        if (PlayerIndex == MyShip.GetComponent<PhotonView>().OwnerActorNr)
        {
            hasBomb = true;
            if (PV == null)
            {
                PV = GetComponent<PhotonView>();
            }
            PV.RPC("On_Second", RpcTarget.AllBuffered, MyShip.photonView.ViewID);
            PV.RPC("Change_VC_Lookat", RpcTarget.AllBuffered, MyShip.photonView.ViewID);
        }
        else
        {
            hasBomb = false;
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
        PhotonView.Find(ViewID).gameObject.transform.Find("Canvas").transform.Find("Bomb_Second").gameObject.SetActive(false);
        PhotonView.Find(ViewID).gameObject.transform.Find("PassTheBomb").gameObject.SetActive(false);
    }

    [PunRPC]
    public void On_Second(int ViewID)
    {
        PhotonView.Find(ViewID).gameObject.transform.Find("Canvas").transform.Find("Bomb_Second").gameObject.SetActive(true);
        PhotonView.Find(ViewID).gameObject.transform.Find("PassTheBomb").gameObject.SetActive(true);
    }

    public void CrashOtherShip(GameObject CrashedShip)
    {
        if(MyShip.photonView.ViewID != CrashedShip.GetPhotonView().ViewID)
        {
            print(MyShip.photonView.ViewID + " / " + CrashedShip.GetPhotonView().ViewID);
            PV.RPC("change_has_bomb", RpcTarget.AllBuffered, new object[] { MyShip.photonView.ViewID, CrashedShip.GetPhotonView().ViewID });
        }
    }

    [PunRPC]
    public void change_has_bomb(int FromViewID, int toViewID)
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
            PV.RPC("Off_Second", RpcTarget.AllBuffered, FromViewID);
            PV.RPC("On_Second", RpcTarget.AllBuffered, toViewID);

            GameObject from_Ship = PhotonView.Find(FromViewID).gameObject;
            GameObject to_Ship = PhotonView.Find(toViewID).gameObject;

            if (from_Ship.GetPhotonView().IsMine)
            {
                hasBomb = false;
            }
            if (to_Ship.GetPhotonView().IsMine)
            {
                hasBomb = true;
            }

            Change_VC_Lookat(toViewID);
        }
    }

}
