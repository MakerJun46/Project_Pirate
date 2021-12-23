using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public NetworkManager instance;

    public GameManager GM;

    public GameObject DisconnetPanel;
    public GameObject RespawnPanel;
    public PhotonView PV;
    public Camera MainCamera;
    System.Random random = new System.Random();


    public GameObject test_Island;

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
        if(PhotonNetwork.IsMasterClient) // 생성 지점 구분을 위한 조건
        {
            GameObject go = PhotonNetwork.Instantiate("Raft", new Vector3(100, 0, 100), Quaternion.Euler(0, 90, 0));
            Debug.Log(go);
            GM.instance.MyShip = go;
            SpawnSailor(1, go.transform);
            SpawnSailor(1, go.transform);
            SpawnIsland_Resource(10, test_Island);
        }
        else
        {
            GameObject go = PhotonNetwork.Instantiate("Ship", new Vector3(-100, 0, -100), Quaternion.Euler(0, 90, 0));
            GM.instance.MyShip = go;
            SpawnSailor(1, go.transform);
            SpawnSailor(1, go.transform);
            SpawnIsland_Resource(10, test_Island);
        }

        if (FindObjectOfType<CombatManager>())
        {
            FindObjectOfType<CombatManager>().SetMyShip(GM.instance.MyShip.GetComponent<Player_Combat_Ship>());
        }

        RespawnPanel.SetActive(false);
    }

    public void SpawnSailor(int count, Transform SpawnPoint)
    {
        if(PhotonNetwork.IsMasterClient)
        {
            GameObject go = PhotonNetwork.Instantiate("Sailor", SpawnPoint.position + new Vector3(4.5f, 8, 0), Quaternion.identity);
            go.transform.parent = SpawnPoint;
        }
    }


    public void SpawnIsland_Resource(int count, GameObject Island)
    {
        float island_location_min = -0.5f;
        float island_location_max = 0.5f;


        for(int i = 0; i < count; i ++)
        {
            float LocationX = random.Next(-300, -28);
            float LocationZ = random.Next(-80, 180);

            PhotonNetwork.Instantiate("Wood", new Vector3(LocationX, 25, LocationZ), Quaternion.identity).transform.parent = Island.transform;
            
            LocationX = random.Next(-300, -28);
            LocationZ = random.Next(-80, 180);

            PhotonNetwork.Instantiate("Rock", new Vector3(LocationX, 15, LocationZ), Quaternion.identity).transform.parent = Island.transform;
        }

        // -0.5 ~ 0.5;
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape) && PhotonNetwork.IsConnected) // ESC 입력 시 연결 끊어짐
        {
            PhotonNetwork.Disconnect();
        }
    }

}
