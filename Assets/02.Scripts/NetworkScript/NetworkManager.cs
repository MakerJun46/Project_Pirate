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

    [SerializeField] GameObject LoadingPanel;
    GameObject CountDownPanel;
    GameObject FadeScreenPanel;

    [SerializeField] int WaitTimeForCount;
    [SerializeField] int CountDownTime=3;
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


        if (RoomData.GetInstance() != null)
        {
            if (RoomData.GetInstance().PlayGameCountOvered() == false)
            {
                // �÷��� �� ���Ӹ�尡 �ִ밡 ���� ������ ���������� ���� ����
                StartGame();
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

    bool characterGened = false;
    public void StartGame()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(StartGameCoroutine());
        }
        else if(characterGened==false)
        {
            characterGened = true;
            Spawn();
        }

    }
    public IEnumerator StartGameCoroutine()
    {
        if (RoomData.GetInstance())
        {
            // ù ��° ���� �ƴϰų�, ù ��° ���ε� random�� �� ��� -> ����
            if (RoomData.GetInstance().PlayedGameCount > 0 || 
                (RoomData.GetInstance().PlayedGameCount==0 && RoomData.GetInstance().gameMode == System.Enum.GetValues(typeof(GameMode)).Length))
            {
                print("CHANGE RNAOMLY : "+ RoomData.GetInstance().GetNotOverlappedRandomGameMode());
                RoomData.GetInstance().GetComponent<PhotonView>().RPC("SetGameModeRPC", RpcTarget.AllBuffered, RoomData.GetInstance().GetNotOverlappedRandomGameMode());
            }
        }
        
        while (true)
        {
            yield return new WaitForEndOfFrame();
            // ��� �÷��̾ ���� �ε�Ǿ�� while�� ����� ���� ����
            // �������� ������ ��� �÷��̾� ���̱� ������ - 1 ������
            if (GameManager.GetInstance().BestPlayerCount + 1 >= PhotonNetwork.CurrentRoom.PlayerCount)
            {
                break;
            }
        }
        
        if (PhotonNetwork.CurrentRoom != null && PhotonNetwork.CurrentRoom.PlayerCount <= 1 && FindObjectOfType<RoomGameManager>())
        {
            // �÷��̾� ȥ�ڸ� ������ Loading���� ���� -> RoomGameManager���� RoomExit�ϴ� Panel Active
        }
        else
        {
            // �÷��̾ �������� ��� ���������� �۵�
            GetComponent<PhotonView>().RPC("StartLoadingFunc", RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    public void StartLoadingFunc()
    {
        StartCoroutine(StartGameEffectCoroutine());
    }
    IEnumerator StartGameEffectCoroutine()
    {
        GameManager.GetInstance().InitializePlayerScore(SceneManager.GetActiveScene().name == "GameScene_Room");
        yield return new WaitForSeconds(1f);

        // ��� �÷��̾ ���� �ε�Ǿ�� while�� ����� ���� ����
        // �������� ������ ��� �÷��̾� ���̱� ������ - 1 ������
        if (PhotonNetwork.IsMasterClient)
        {
            GameManager.GetInstance().SetObserverCamera();  // ������ ���� ����
        }

        yield return StartCoroutine("LoadingFadeInOut", true);

        yield return new WaitForSeconds(WaitTimeForCount);

        if (FindObjectOfType<CutSceneManager>())
            yield return new WaitForSeconds(Mathf.Max(0f, (float)FindObjectOfType<CutSceneManager>().director.duration - 6f));

        yield return StartCoroutine(CountDownCoroutine(CountDownTime));

        GameManager.GetInstance().StartGame();
    }

    public void EndGame()
    {
        StartCoroutine("EndGameCoroutine");
    }
    IEnumerator EndGameCoroutine()
    {
        GameManager.GetInstance().JudgeWinLose();

        yield return StartCoroutine("LoadingFadeInOut", false);

        GameManager.GetInstance().EndGame();
    }

    public void StartCountDown(int loadingSec)
    {
        StartCoroutine(CountDownCoroutine(loadingSec));
    }

    IEnumerator CountDownCoroutine(int loadingSec)
    {
        CountDownPanel.SetActive(true);
        for (int i = loadingSec; i > 0; i--)
        {
            CountDownPanel.GetComponentInChildren<TextMeshProUGUI>().text = i.ToString();
            yield return new WaitForSecondsRealtime(1.0f);
        }
        if(FindObjectOfType<CutSceneManager>())
            FindObjectOfType<CutSceneManager>().director.Stop();

        CountDownPanel.SetActive(false);
    }

    public void StartFadeInOut(bool FadeIn)
    {
        StartCoroutine("LoadingFadeInOut", FadeIn);
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
        Vector3 radomPos = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)) * PlayerSpawnRadius;
        for (int i = 0; i < 50; i++)
        {
            radomPos = new Vector3(Random.Range(-1f, 1f) * PlayerSpawnRadius, 0f, Random.Range(-1f, 1f) * PlayerSpawnRadius);
            RaycastHit hit;
            if (Physics.SphereCast(radomPos + Vector3.up * 100, 10f, Vector3.down, out hit, 200f))
            {
                if (hit.transform.CompareTag("Sea"))
                {
                    radomPos.y = 0;
                    return radomPos;
                }
            }
        }
        radomPos.y = 0;
        return radomPos;
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
        PhotonNetwork.LeaveRoom();
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
        //DisconnetPanel.SetActive(false);
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
        //DisconnetPanel.SetActive(true);
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
