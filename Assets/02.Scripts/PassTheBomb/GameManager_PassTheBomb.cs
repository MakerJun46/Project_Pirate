using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon;
using System;
using TMPro;
using Photon.Realtime;

public class GameManager_PassTheBomb : MonoBehaviour
{
    public static GameManager_PassTheBomb instance;
    private void Awake()
    {
        instance = this;
    }

    public bool hasBomb;
    public PhotonView PV;
    System.Random random;
    public TextMeshProUGUI bomb_Second;
    public int Count_Sec;

    public GameObject LoadingPanel;

    public Cinemachine.CinemachineVirtualCamera VC_Bomb;

    bool isSetting;

    private void Start()
    {
        isSetting = false;
    }
    private void Update()
    {
        if(!isSetting && GameManager.GetInstance().MyShip != null && PhotonNetwork.IsMasterClient 
            && GameManager.GetInstance().AllShip.Count >= RoomPlayerCount.playerCount)
        {
            PV.RPC("setting", RpcTarget.AllBuffered);
            isSetting = true;
        }

        if (Count_Sec < 1 && hasBomb)
        {
            Debug.LogError(PhotonNetwork.LocalPlayer.NickName + " 폭탄 소지로 패배 !!"); // game Over Scene
            PV.RPC("Bomb_Explode", RpcTarget.AllBuffered, GameManager.GetInstance().MyShip.photonView.ViewID);
            hasBomb = false;
        }
    }

    [PunRPC]
    public void Bomb_Explode(int ViewID)
    {
        GameManager.GetInstance().MyShip.GetComponent<Player_Controller_Ship>().Ship_Stop();
        StartCoroutine(Explosion(ViewID));
    }

    IEnumerator Explosion(int ViewID)
    {
        VC_Bomb.Priority = 15;
        yield return new WaitForSeconds(1.0f);

        PhotonView.Find(ViewID).transform.Find("PassTheBomb").GetChild(1).gameObject.SetActive(true);
        PhotonView.Find(ViewID).transform.Find("PassTheBomb").GetChild(0).gameObject.SetActive(false);
    }

    [PunRPC]
    public void setting()
    {
        StartCoroutine(Loading());
    }
    IEnumerator Loading()
    {
        int loading_sec = 4;

        while(loading_sec > 1)
        {
            loading_sec--;
            LoadingPanel.transform.Find("Loading_Second").GetComponent<TextMeshProUGUI>().text = loading_sec.ToString();
            yield return new WaitForSecondsRealtime(1.0f);
        }

        hasBomb = false;
        random = new System.Random();
        bomb_Second = GameManager.GetInstance().MyShip.transform.Find("Canvas").transform.Find("Bomb_Second").GetComponent<TextMeshProUGUI>();

        for (int i = 0; i < GameManager.GetInstance().AllShip.Count; i++)
        {
            GameManager.GetInstance().AllShip[i].gameObject.transform.Find("Canvas").transform.Find("HealthArea").gameObject.SetActive(false);
            GameManager.GetInstance().AllShip[i].gameObject.transform.Find("Canvas").transform.Find("Health").gameObject.SetActive(false);
            GameManager.GetInstance().AllShip[i].gameObject.transform.Find("Canvas").transform.Find("Bomb_Second").gameObject.SetActive(false);
        }

        Count_Sec = 60;

        loading_sec--;
        LoadingPanel.transform.Find("Loading_Second").GetComponent<TextMeshProUGUI>().text = loading_sec.ToString();
        yield return new WaitForSecondsRealtime(1.0f);

        if (PhotonNetwork.IsMasterClient)
        {
            int randomPlayerIndex = selectBomb();
            PV.RPC("FirstHasBomb", RpcTarget.AllBuffered, randomPlayerIndex);
        }

        StartCoroutine(CountSecond());

        GameManager.GetInstance().TryUpgradeShip();
        CombatManager.instance.EquipSail(0, 1);
        CombatManager.instance.EquipSpecialCannon(0, 0);

        GameManager.GetInstance().MyShip.MoveSpeed = 20;

        LoadingPanel.SetActive(false);
    }


    IEnumerator CountSecond()
    {
        while(Count_Sec > 0)
        {
            Count_Sec--;
            
            if(hasBomb)
                PV.RPC("update_Bomb_Sec", RpcTarget.AllBuffered, GameManager.GetInstance().MyShip.photonView.ViewID);
            
            yield return new WaitForSecondsRealtime(1.0f);
        }
    }

    private int selectBomb()
    {
        Debug.Log("플레이어 수 : " + RoomPlayerCount.playerCount);
        return random.Next(RoomPlayerCount.playerCount);
    }

    [PunRPC]
    public void FirstHasBomb(int PlayerIndex)
    {
        if(PhotonNetwork.PlayerList[PlayerIndex].UserId == PhotonNetwork.LocalPlayer.UserId)
        {
            hasBomb = true;
            PV.RPC("On_Second", RpcTarget.AllBuffered, GameManager.GetInstance().MyShip.photonView.ViewID);
            PV.RPC("Change_VC_Lookat", RpcTarget.AllBuffered, GameManager.GetInstance().MyShip.photonView.ViewID);
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

    [PunRPC]
    public void update_Bomb_Sec(int ViewID)
    {
        PhotonView.Find(ViewID).gameObject.transform.Find("Canvas").transform.Find("Bomb_Second").GetComponent<TextMeshProUGUI>().text = Count_Sec.ToString();
    }

    public void CrashOtherShip(GameObject CrashedShip)
    {
        if (this.hasBomb)
            PV.RPC("change_has_bomb", RpcTarget.AllBuffered, new object[] { GameManager.GetInstance().MyShip.photonView.ViewID, CrashedShip.GetPhotonView().ViewID });
    }

    [PunRPC]
    public void change_has_bomb(int FromViewID, int toViewID)
    {
        PV.RPC("Off_Second", RpcTarget.AllBuffered, FromViewID);
        PV.RPC("On_Second", RpcTarget.AllBuffered, toViewID);

        GameObject from_Ship = PhotonView.Find(FromViewID).gameObject;
        GameObject to_Ship = PhotonView.Find(toViewID).gameObject;

        if(from_Ship.GetPhotonView().IsMine)
        {
            hasBomb = false;
        }
        if(to_Ship.GetPhotonView().IsMine)
        {
            hasBomb = true;
        }

        Change_VC_Lookat(toViewID);
    }
}
