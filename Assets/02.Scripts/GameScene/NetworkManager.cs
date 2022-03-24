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

        // Play한 게임 count가 3 이하이라면 다음 게임을 위해 Start Game 호출
        // 만약 3보다 크다면 RoomGameManager에서 처리
        if (RoomData.GetInstance() == null || RoomData.GetInstance().PlayedGameCount <= 3 && !(RoomData.GetInstance().PlayedGameCount ==3 && SceneManager.GetActiveScene().name== "GameScene_Room"))
        {
            Debug.Log("Start End Game is Called");
            StartEndGame(true);
        }
        else
        {
            Debug.Log("Start End Game is Not Called");
        }
    }

    public void StartEndGame(bool _start)
    {
        if (_start == false)
            GameManager.GetInstance().GameStart = false;
        StartCoroutine(StartEndGameCoroutine(_start));
    }

    public IEnumerator StartEndGameCoroutine(bool _start)
    {
        if (PhotonNetwork.IsConnected == false || PhotonNetwork.IsMasterClient)
        {
            while (_start)
            {
                yield return new WaitForEndOfFrame();
                // 모든 플레이어가 씬에 로드되어야지만 while문 벗어나서 게임 시작
                if (GameManager.GetInstance().BestPlayerCount >= PhotonNetwork.CurrentRoom.PlayerCount)
                    break;
            }

            // 맨 처음에는 마스터가 지정한 게임을 하고, 아니라면 랜덤한 게임을 시작
            if (RoomData.GetInstance().PlayedGameCount != 0 && SceneManager.GetActiveScene().name == "GameScene_Room" && _start )
            {
                int tmpRandom = Random.Range(0, 3);
                RoomData.GetInstance().GetComponent<PhotonView>().RPC("SetGameModeRPC", RpcTarget.AllBuffered, tmpRandom);
            }

            if (PhotonNetwork.CurrentRoom.PlayerCount <= 1)
            {
                Debug.Log("PhotonNetwork.CurrentRoom.PlayerCount <= 1");
                // 플레이어 혼자만 남으면 Loading하지 않음 -> RoomGameManager에서 RoomExit하는 Panel Active
            }
            else
            {
                Debug.Log("RoomData.GetInstance().GetComponent<PhotonView>().RPC(, RpcTarget.AllBuffered, _start)");
                // 플레이어가 남아있을 경우 정상적으로 작동
                RoomData.GetInstance().GetComponent<PhotonView>().RPC("StartLoading", RpcTarget.AllBuffered, _start);
            }
        }
    }

    public void LoadingFunc(bool _start)
    {

        StartCoroutine(LoadingCoroutine(_start));
    }
    IEnumerator LoadingCoroutine(bool _start)
    {
        LoadingPanel.SetActive(true);

        //if (_start == false)
        //    GameManager.GetInstance().GameStart = false;

        TextMeshProUGUI loadingTxt = LoadingPanel.GetComponentInChildren<TextMeshProUGUI>();
        for (int i = loading_sec; i > 0; i--)
        {
            loadingTxt.text = i.ToString();
            yield return new WaitForSecondsRealtime(1.0f);
        }

        LoadingPanel.SetActive(false);

        print("LOADING END : Start : "+ _start);
        if(_start)
            GameManager.GetInstance().StartGame();
        else
            GameManager.GetInstance().EndGame();
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
        CustomizeManager.GetInstance().EquipCostume(go.GetComponent<PhotonView>().ViewID);
    }

    [SerializeField] float PlayerSpawnRadius = 100f;
    public Vector3 CalculateSpawnPos()
    {
        for (int i = 0; i < 50; i++)
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


    #region Networking
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

    #endregion
}
