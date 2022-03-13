using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public NetworkManager instance;

    [SerializeField]private GameObject DisconnetPanel;
    private PhotonView PV;

    [SerializeField] GameObject LoadingPanel;
    [SerializeField]private int loading_sec=3;


    private void Awake()
    {
        instance = this;
        //Screen.SetResolution(960, 540, false);
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

        // RoomData.GetInstance() == null Debug를 위함
        if (RoomData.GetInstance() == null ||  RoomData.GetInstance().PlayedGameCount < 3 || (RoomData.GetInstance().PlayedGameCount >= 3 &&  SceneManager.GetActiveScene().name!= "GameScene_Room"))
            StartEndGame(true);
    }

    public void StartEndGame(bool _start)
    {
        StartCoroutine(StartEndGameCoroutine(_start));
    }

    public IEnumerator StartEndGameCoroutine(bool _start)
    {
        if (PhotonNetwork.IsConnected == false || PhotonNetwork.IsMasterClient)
        {
            if (RoomData.GetInstance().setSceneRandom && RoomData.GetInstance().PlayedGameCount!=0)
                if(Random.Range(0,1f)>0.5f)
                    RoomData.GetInstance().GetComponent<PhotonView>().RPC("SetGameModeRPC",RpcTarget.AllBuffered,0);
                else
                    RoomData.GetInstance().GetComponent<PhotonView>().RPC("SetGameModeRPC", RpcTarget.AllBuffered,2);
            while (_start)
            {
                yield return new WaitForEndOfFrame();
                if (GameManager.GetInstance().BestPlayerLists.Count >= PhotonNetwork.CurrentRoom.PlayerCount)
                    break;
            }
            RoomData.GetInstance().GetComponent<PhotonView>().RPC("StartLoading", RpcTarget.AllBuffered,_start);
        }
    }

    public void setting(bool _start)
    {
        StartCoroutine(Loading(_start));
    }
    IEnumerator Loading(bool _start)
    {
        LoadingPanel.SetActive(true);
        yield return new WaitForEndOfFrame();

        if (_start == false)
            GameManager.GetInstance().GameStart = false;

        for (int i = loading_sec; i > 0; i--)
        {
            LoadingPanel.transform.Find("Loading_Second").GetComponent<TextMeshProUGUI>().text = i.ToString();
            yield return new WaitForSecondsRealtime(1.0f);
        }

        LoadingPanel.SetActive(false);

        print("LOADING END");
        if(_start)
            GameManager.GetInstance().StartGame();
        else
            GameManager.GetInstance().EndGame();
    }

    /// <summary>
    /// 네트워크 정보로 연결 시도
    /// </summary>
    public void Connect() => PhotonNetwork.ConnectUsingSettings();

    public void GoToLobby()
    {
        SceneManager.LoadScene("GameScene_Room", LoadSceneMode.Single);
    }
    public void ExitRoom()
    {
        SceneManager.LoadScene(0, LoadSceneMode.Single);
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
        if (RoomData.GetInstance() == null)
            PhotonNetwork.Instantiate("RoomData", Vector3.zero, Quaternion.identity);

        Spawn();
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        RoomData.GetInstance().DestroyRoomData();
    }

    /// <summary>
    /// photon 네트워크와 연결이 끊어진 시점에 호출
    /// </summary>
    /// <param name="cause"></param>
    public override void OnDisconnected(DisconnectCause cause)
    {
        DisconnetPanel.SetActive(true);
    }
    public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient)
    {
        if (PhotonNetwork.IsConnected)
        {
            GameManager.GetInstance().MasterChanged(PhotonNetwork.IsMasterClient ? true : false);
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
