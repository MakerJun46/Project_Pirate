using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public NetworkManager instance;


    public GameObject DisconnetPanel;
    public GameObject RespawnPanel;
    public PhotonView PV;
    System.Random random = new System.Random();


    public GameObject test_Island;

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
        if (PhotonNetwork.IsConnected)
        {
            DisconnetPanel.SetActive(false);
            Spawn();
        }
        else
        {
            Connect();
        }
        //GameManager.GetIstance().GenerateObstacles();
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
        //PhotonNetwork.LocalPlayer.NickName = PlayerPrefs.GetString("NickName");
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
            go = PhotonNetwork.Instantiate("PlayerShip", CalculateSpawnPos(), Quaternion.Euler(0, 90, 0));
        }
        else
        {
            go = PhotonNetwork.Instantiate("PlayerShip", CalculateSpawnPos(), Quaternion.Euler(0, 90, 0));
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

    [SerializeField] float PlayerSpawnRadius=100f;
    public Vector3 CalculateSpawnPos()
    {
        for (int i = 0; i < 50;i++)
        {
            Vector3 radomPos = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)) * PlayerSpawnRadius;
            RaycastHit hit;
            if (Physics.SphereCast(radomPos + Vector3.up * 100, 10f, Vector3.down, out hit, 200f))
            {
                if (hit.transform.CompareTag("Sea"))
                {
                    return radomPos;
                }
            }
        }
        return new Vector3(10, 0, 10);
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

    public void GoToLobby()
    {
        SceneManager.LoadScene(0, LoadSceneMode.Single);
    }
}
