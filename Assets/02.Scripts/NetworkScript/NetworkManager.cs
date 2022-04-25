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
    public static NetworkManager instance;

    private PhotonView PV;

    [SerializeField] private GameObject DisconnetPanel;
    [SerializeField] GameObject LoadingPanel;
    [SerializeField] GameObject CountDownPanel;
    [SerializeField] GameObject FadeScreenPanel;
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
        CountDownPanel = LoadingPanel.transform.GetChild(0).gameObject;
        FadeScreenPanel = LoadingPanel.transform.GetChild(1).gameObject;
        FadeScreenPanel.GetComponent<Image>().color = Color.black;

        if (PhotonNetwork.IsConnected)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                DisconnetPanel.SetActive(false);
                Invoke("Spawn", 1f);
            }
        }
        else
        {
            Connect();
        }

        if (RoomData.GetInstance() != null)
        {
            if (RoomData.GetInstance().PlayGameCountOvered() == false)
            {
                // �÷��� �� ���Ӹ�尡 �ִ밡 ���� ������ ���������� ���� ����
                StartEndGame(true);
                if (SceneManager.GetActiveScene().name != "GameScene_Room")
                {
                    RoomData.GetInstance().RemoveCurrGameMode();
                }
            }
            else
            {
                // �÷��� �� ���Ӹ�尡 �ִ밡 �Ǹ� ��¥ ���� ����
                // GameScene_Room�� RoomGameManager�� ���� ���� ó��
                if (SceneManager.GetActiveScene().name != "GameScene_Room")
                {
                    SceneManager.LoadScene("GameScene_Room");
                }
            }
        }
    }

    public void StartEndGame(bool _start)
    {
        if (PhotonNetwork.IsConnected == false || PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(StartEndGameCoroutine(_start));
        }
    }

    public IEnumerator StartEndGameCoroutine(bool _start)
    {
        if (RoomData.GetInstance() && _start)
        {
            // ù ��° ���� �ƴϰų�, ù ��° ���ε� random�� �� ��� -> ����
            if (RoomData.GetInstance().PlayedGameCount > 0 || (RoomData.GetInstance().PlayedGameCount==0 && RoomData.GetInstance().gameMode == System.Enum.GetValues(typeof(GameMode)).Length))
            {
                RoomData.GetInstance().GetComponent<PhotonView>().RPC("SetGameModeRPC", RpcTarget.AllBuffered, RoomData.GetInstance().GetNotOverlappedRandomGameMode());
            }
        }

        while (_start)
        {
            yield return new WaitForEndOfFrame();
            // ��� �÷��̾ ���� �ε�Ǿ�� while�� ����� ���� ����
            // �������� ������ ��� �÷��̾� ���̱� ������ - 1 ������
            if (GameManager.GetInstance().BestPlayerCount + 1 >= PhotonNetwork.CurrentRoom.PlayerCount) 
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
            GetComponent<PhotonView>().RPC("LoadingFunc", RpcTarget.AllBuffered, _start);
        }
    }

    [PunRPC]
    public void LoadingFunc(bool _start)
    {
        StartCoroutine(LoadingCoroutine(_start));
    }
    IEnumerator LoadingCoroutine(bool _start)
    {
        LoadingPanel.SetActive(true);
        if (_start)
        {
            yield return StartCoroutine("LoadingFadeInOut", true);
        }
        else
        {
            GameManager.GetInstance().JudgeWinLose();
        }
        CountDownPanel.SetActive(true);
        for (int i = loading_sec; i > 0; i--)
        {
            CountDownPanel.GetComponentInChildren<TextMeshProUGUI>().text = i.ToString();
            yield return new WaitForSecondsRealtime(1.0f);
        }
        CountDownPanel.SetActive(false);

        if (_start==false)
            yield return StartCoroutine("LoadingFadeInOut",false); // 0420 ���ӽ��� �� �� FadeOut ȿ�� �߰�


        LoadingPanel.SetActive(false);
        if (_start)
            GameManager.GetInstance().StartGame();
        else
            GameManager.GetInstance().EndGame();
    }
    IEnumerator LoadingFadeInOut(bool FadeIn)
    {

        while (true)
        {
            Color c = FadeScreenPanel.GetComponent<Image>().color;
            if (FadeIn)
            {
                c.a -= Time.deltaTime;
                if (c.a <= 0)
                {
                    c.a = 0;
                    break;
                }
            }
            else
            {
                c.a += Time.deltaTime;
                if (c.a >= 1)
                {
                    c.a = 1;
                    break;
                }
            }

            FadeScreenPanel.GetComponent<Image>().color = c;

            yield return new WaitForSeconds(Time.deltaTime);
        }
    }


    /// <summary>
    /// �÷��̾� �� ����
    /// </summary>
    public void Spawn()
    {
        GameObject go = PhotonNetwork.Instantiate("PlayerShip", CalculateSpawnPos(), Quaternion.Euler(0, 90, 0));

        if (go.GetComponent<PhotonView>().IsMine)
        {
            GameManager.GetInstance().SetMyShip(go.GetComponent<Player_Controller_Ship>());
            CombatManager.instance.SetMyShip(go.GetComponent<Player_Combat_Ship>());

            go.GetComponent<PhotonView>().RPC("InitializePlayer", RpcTarget.AllBuffered);
        }
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
    }
    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
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
