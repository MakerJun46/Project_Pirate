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

    public float StartTime = 3f;
    private void Awake()
    {
        instance = this;
        Screen.SetResolution(960, 540, false);
        PhotonNetwork.SendRate = 60;
        PhotonNetwork.SerializationRate = 30;
    }

    private void Start()
    {
        PV = GetComponent<PhotonView>();
        if (PhotonNetwork.IsConnected)
        {
            DisconnetPanel.SetActive(false);
            Invoke("Spawn",1f);
        }
        else
        {
            Connect();
        }
    }

    private void Update()
    {
        if (StartTime >= 0 && GameManager.GetInstance().GameStart==false)
        {
            StartTime -= Time.deltaTime;
        }
        else
        {
            if (PhotonNetwork.IsMasterClient)
            {
                PV.RPC("GameStartRPC",RpcTarget.AllBuffered,true);
            }
        }
    }

    [PunRPC]
    public void GameStartRPC(bool _start)
    {
        GameManager.GetInstance().GameStart = _start;
    }


    /// <summary>
    /// 네트워크 정보로 연결 시도
    /// </summary>
    public void Connect() => PhotonNetwork.ConnectUsingSettings();

    public void GoToLobby()
    {
        SceneManager.LoadScene(1, LoadSceneMode.Single);
    }

    /// <summary>
    /// 플레이어가 네트워크에 접속한 시점에 호출
    /// </summary>
    public override void OnConnectedToMaster()
    {
        //PhotonNetwork.LocalPlayer.NickName = PlayerPrefs.GetString("NickName");
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
    public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient)
    {
        if (PhotonNetwork.IsConnected)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                GameManager.GetInstance().MasterChanged(true);
            }
            else
            {
                GameManager.GetInstance().MasterChanged(false);
            }
        }
    }

    /// <summary>
    /// 플레이어 배 생성
    /// </summary>
    public void Spawn()
    {
        GameObject go = PhotonNetwork.Instantiate("PlayerShip", CalculateSpawnPos(), Quaternion.Euler(0, 90, 0));

        if (go.GetComponent<PhotonView>().IsMine)
        {
            GameManager.GetInstance().SetMyShip(go.GetComponent<Player_Controller_Ship>());
            if (GameManager.GetInstance().GetComponent<BattleRoyalGameManager>())
            {
                GameManager.GetInstance().GetComponent<BattleRoyalGameManager>().SpawnSailor(1, go.transform);
                GameManager.GetInstance().GetComponent<BattleRoyalGameManager>().SpawnSailor(1, go.transform);
            }
            if (FindObjectOfType<CombatManager>())
                FindObjectOfType<CombatManager>().SetMyShip(go.GetComponent<Player_Combat_Ship>());

            go.GetComponent<PhotonView>().RPC("InitializePlayer", RpcTarget.AllBuffered);
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
}
