using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public NetworkManager instance;


    public GameObject DisconnetPanel;
    public GameObject RespawnPanel;
    public PhotonView PV;
    System.Random random = new System.Random();


    public GameObject test_Island;

    [SerializeField] private Vector3 shipSpawnPos= new Vector3(100, 0, 100);
    [SerializeField] private Vector2 SpawnIsland_X_MinMax = new Vector2(-300, -28);
    [SerializeField] private Vector2 SpawnIsland_Y_MinMax = new Vector2(-80, 180);
    private void Awake()
    {
        instance = this;
        Screen.SetResolution(960, 540, false);
        PhotonNetwork.SendRate = 60;
        PhotonNetwork.SerializationRate = 30;
    }

    private void Start()
    {
        Connect();
    }


    /// <summary>
    /// 네트워크 정보로 연결 시도
    /// </summary>
    public void Connect() => PhotonNetwork.ConnectUsingSettings();


    /// <summary>
    /// 플레이어가 네트워크에 접속한 시점에 호출
    /// </summary>
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.LocalPlayer.NickName = PlayerPrefs.GetString("NickName");
        PhotonNetwork.JoinOrCreateRoom("Room", new RoomOptions { MaxPlayers = 4 }, null);
        Debug.Log("Conneted to Master");
    }

    /// <summary>
    /// photon 서버의 방(Room)에 합류한 시점에 호출
    /// </summary>
    public override void OnJoinedRoom()
    {
        DisconnetPanel.SetActive(false);
        Spawn();
    }

    /// <summary>
    /// photon 네트워크와 연결이 끊어진 시점에 호출
    /// </summary>
    /// <param name="cause"></param>
    public override void OnDisconnected(DisconnectCause cause)
    {
        DisconnetPanel.SetActive(true);
        RespawnPanel.SetActive(false);
    }

    /// <summary>
    /// 플레이어 배 생성
    /// </summary>
    public void Spawn()
    {
        GameObject go = null;
        if (PhotonNetwork.IsMasterClient) // 생성 지점 구분을 위한 조건
        {
            go = PhotonNetwork.Instantiate("Raft", shipSpawnPos, Quaternion.Euler(0, 90, 0));
        }
        else
        {
            go = PhotonNetwork.Instantiate("Raft", shipSpawnPos+new Vector3(10,0,10), Quaternion.Euler(0, 90, 0));
        }

        if (FindObjectOfType<CombatManager>() && go.GetComponent<PhotonView>().IsMine)
        {
            GameManager.GetIstance().SetMyShip(go.GetComponent<Player_Controller_Ship>());
            SpawnSailor(1, go.transform);
            SpawnSailor(1, go.transform);
            FindObjectOfType<CombatManager>().SetMyShip(go.GetComponent<Player_Combat_Ship>());
        }

        RespawnPanel.SetActive(false);
    }

    public void SpawnSailor(int count, Transform _ship)
    {
        if(PhotonNetwork.IsMasterClient)
        {
            GameObject go = PhotonNetwork.Instantiate("Sailor", Vector3.zero, Quaternion.identity);
            go.transform.parent = _ship.transform.Find("SailorSpawnPos");
            go.transform.localPosition = Vector3.zero;
        }
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape) && PhotonNetwork.IsConnected) // ESC 입력 시 연결 끊어짐
        {
            PhotonNetwork.Disconnect();
        }
    }

}
