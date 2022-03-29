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
        if (PhotonNetwork.IsConnected && !PhotonNetwork.IsMasterClient)
        {
            DisconnetPanel.SetActive(false);
            Invoke("Spawn",1f);
        }
        else if(!PhotonNetwork.IsConnected)
        {
            Connect();
        }

        // RoomData.GetInstance() == null Debug�� ����
        // �÷��� �� ���Ӹ�尡 3���� �Ǹ� ��¥ ���� ����
        //
        if (RoomData.GetInstance() == null ||  RoomData.GetInstance().PlayedGameCount < 3 || (RoomData.GetInstance().PlayedGameCount >= 3 && SceneManager.GetActiveScene().name != "GameScene_Room"))
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
            // �� ó������ �����Ͱ� ������ ������ �ϰ�, �ƴ϶�� ������ ������ ����
            if (RoomData.GetInstance() && RoomData.GetInstance().setSceneRandom && RoomData.GetInstance().PlayedGameCount!=0 && _start)
                RoomData.GetInstance().GetComponent<PhotonView>().RPC("SetGameModeRPC", RpcTarget.AllBuffered, Random.Range(0, 3));

            while (_start)
            {
                yield return new WaitForEndOfFrame();
                // ��� �÷��̾ ���� �ε�Ǿ������ while�� ����� ���� ����
                if (GameManager.GetInstance().BestPlayerCount + 1 >= PhotonNetwork.CurrentRoom.PlayerCount) // �������� ������ ��� �÷��̾� ���̱� ������ - 1 ������_0327
                {
                    break;
                }
            }

            if (PhotonNetwork.IsMasterClient)    // ������ Ŭ���̾�Ʈ�� ��� ������
            {
                GameManager.GetInstance().SetObserverCamera();  // ������ ���� ����
            }

            if (PhotonNetwork.CurrentRoom != null && PhotonNetwork.CurrentRoom.PlayerCount <= 1 && FindObjectOfType<RoomGameManager>())
            {
                // �÷��̾� ȥ�ڸ� ������ Loading���� ���� -> RoomGameManager���� RoomExit�ϴ� Panel Active
            }
            else
            {
                // �÷��̾ �������� ��� ���������� �۵�
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

        if (_start == false)
            GameManager.GetInstance().GameStart = false;

        for (int i = loading_sec; i > 0; i--)
        {
            LoadingPanel.GetComponentInChildren<TextMeshProUGUI>().text = i.ToString();
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
    /// �÷��̾� �� ����
    /// </summary>
    public void Spawn()
    {
        Debug.Log("Spawn Ship");

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
        FindObjectOfType<CustomizeManager>().EquipCostume(go.GetComponent<PhotonView>().ViewID);
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
    /// ��Ʈ��ũ ������ ���� �õ�
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
    /// �÷��̾ ��Ʈ��ũ�� ������ ������ ȣ��
    /// </summary>
    public override void OnConnectedToMaster()
    {
        //PhotonNetwork.LocalPlayer.NickName = PlayerPrefs.GetString("NickName");
        PhotonNetwork.JoinOrCreateRoom("Room", new RoomOptions { MaxPlayers = 5 }, null);
        Debug.Log("Conneted to Master");
    }

    /// <summary>
    /// photon ������ ��(Room)�� �շ��� ������ ȣ��
    /// </summary>
    public override void OnJoinedRoom()
    {
        Debug.LogError("OnJoinedRoom");
        DisconnetPanel.SetActive(false);
        if (RoomData.GetInstance() == null)
            PhotonNetwork.Instantiate("RoomData", Vector3.zero, Quaternion.identity);

        if(!PhotonNetwork.IsMasterClient) // masterClient�� Observer �̹Ƿ� �����ϰ� Spawn - 0324
            Spawn();
    }
    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        RoomData.GetInstance().DestroyRoomData();
    }

    /// <summary>
    /// photon ��Ʈ��ũ�� ������ ������ ������ ȣ��
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
