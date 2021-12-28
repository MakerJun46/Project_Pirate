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
    /// ��Ʈ��ũ ������ ���� �õ�
    /// </summary>
    public void Connect() => PhotonNetwork.ConnectUsingSettings();


    /// <summary>
    /// �÷��̾ ��Ʈ��ũ�� ������ ������ ȣ��
    /// </summary>
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.LocalPlayer.NickName = PlayerPrefs.GetString("NickName");
        PhotonNetwork.JoinOrCreateRoom("Room", new RoomOptions { MaxPlayers = 4 }, null);
        Debug.Log("Conneted to Master");
    }

    /// <summary>
    /// photon ������ ��(Room)�� �շ��� ������ ȣ��
    /// </summary>
    public override void OnJoinedRoom()
    {
        DisconnetPanel.SetActive(false);
        Spawn();
    }

    /// <summary>
    /// photon ��Ʈ��ũ�� ������ ������ ������ ȣ��
    /// </summary>
    /// <param name="cause"></param>
    public override void OnDisconnected(DisconnectCause cause)
    {
        DisconnetPanel.SetActive(true);
        RespawnPanel.SetActive(false);
    }

    /// <summary>
    /// �÷��̾� �� ����
    /// </summary>
    public void Spawn()
    {
        GameObject go = null;
        if (PhotonNetwork.IsMasterClient) // ���� ���� ������ ���� ����
        {
            go = PhotonNetwork.Instantiate("Raft", shipSpawnPos, Quaternion.Euler(0, 90, 0));
            SpawnIsland_Resource(10, test_Island);
        }
        else
        {
            go = PhotonNetwork.Instantiate("Raft", shipSpawnPos+new Vector3(10,0,10), Quaternion.Euler(0, 90, 0));
        }

        if (FindObjectOfType<CombatManager>() && go.GetComponent<PhotonView>().IsMine)
        {
            GM.instance.MyShip = go;
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


    public void SpawnIsland_Resource(int count, GameObject Island)
    {
        float island_location_min = -0.5f;
        float island_location_max = 0.5f;


        for(int i = 0; i < count; i ++)
        {
            float LocationX = random.Next((int)SpawnIsland_X_MinMax.x, (int)SpawnIsland_X_MinMax.y);
            float LocationZ = random.Next((int)SpawnIsland_Y_MinMax.x, (int)SpawnIsland_Y_MinMax.y);

            GameObject tmpObj;
            tmpObj = PhotonNetwork.Instantiate("Wood", Vector3.zero, Quaternion.identity).gameObject;
            tmpObj.transform.parent = Island.transform;
            tmpObj.transform.localPosition = new Vector3(LocationX, 0, LocationZ);

            LocationX = random.Next((int)SpawnIsland_X_MinMax.x, (int)SpawnIsland_X_MinMax.y);
            LocationZ = random.Next((int)SpawnIsland_Y_MinMax.x, (int)SpawnIsland_Y_MinMax.y);

            tmpObj = PhotonNetwork.Instantiate("Rock", Vector3.zero, Quaternion.identity).gameObject;
            tmpObj.transform.parent = Island.transform;
            tmpObj.transform.localPosition = new Vector3(LocationX, 0, LocationZ);
        }

        // -0.5 ~ 0.5;
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape) && PhotonNetwork.IsConnected) // ESC �Է� �� ���� ������
        {
            PhotonNetwork.Disconnect();
        }
    }

}
