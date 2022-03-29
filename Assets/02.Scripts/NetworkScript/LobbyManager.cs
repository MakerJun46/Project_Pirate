using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine.SceneManagement;


public class LobbyManager : MonoBehaviourPunCallbacks
{
    #region Variables & Initializer
    [System.Serializable]
    public struct GameModeInfo
    {
        public Sprite sprite;
    }

    [SerializeField] string myNickName;

    [Header("[Room Info]")]
    public string roomName;
    public bool IsVisible = true;
    public bool IsOpen = true;

    [SerializeField] Image GameModeImage;
    [SerializeField] Text GameModeText;
    [SerializeField] GameModeInfo[] GameModeInfos;

    [Header("[RoomList]")]
    [SerializeField] Transform RoomListContainer;
    public GameObject RoomListBtnPrefab;
    public List<RoomListBtn> RoomListBtnLists = new List<RoomListBtn>();

    [Header("[PlayerList]")]
    public Transform PlayerListContainer;
    public PlayerListContent myPlayerListContent;

    [Header("[UI]")]
    [SerializeField] Animator TitleAnim;
    [SerializeField] Text contractText;

    [SerializeField] GameObject MainPanel;
    [SerializeField] GameObject LobbyPanel;
    [SerializeField] GameObject RoomPanel;

    [SerializeField] Text ReadyCountText;
    [SerializeField] GameObject OptionBlindForClient;
    [SerializeField] InputField SetNickNameInputField;

    [Header("[Chat]")]
    [SerializeField] GameObject ChatUI;
    [SerializeField] InputField ChatInputField;
    [SerializeField] List<Text> ChatTexts;

    private string debugLog;


    private void Awake()
    {
        PhotonNetwork.SendRate = 60;
        PhotonNetwork.SerializationRate = 30;
    }

    private void Start()
    {
        // Scene�� �ҷ����� �� ������ Panel Active �� Player�г��� ����
        if (PhotonNetwork.IsConnected && PhotonNetwork.CurrentRoom != null)
        {
            MainPanel.SetActive(false);
            LobbyPanel.SetActive(true);
            RoomPanel.SetActive(false);
            LeaveRoom();
        }
        SetNickNameInputField.text = PhotonNetwork.IsConnected ? PhotonNetwork.LocalPlayer.NickName : "Guest" + Random.Range(0, 1000);
    }
    #endregion


    #region Networking
    private void Update()
    {
        if (PhotonNetwork.IsConnected && PhotonNetwork.LocalPlayer != null)
        {
            if (PhotonNetwork.InRoom)
            {
                // IsKicked��� CustomProperty�� �������ְ�, �װ��� True���, ���� ����
                if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("IsKicked"))
                {
                    if ((bool)PhotonNetwork.LocalPlayer.CustomProperties["IsKicked"])
                    {
                        LeaveRoom();
                    }
                }

                // Ready�� �� ����� ���� CurrentRoom�� �÷��̾� �ο������� ���ٸ� ���� ����(�����Ͱ� �� �� ����)
                int readyCount = 0;
                foreach (var p in PhotonNetwork.PlayerList)
                {
                    if ((string)p.CustomProperties["Ready"] == "1")
                        readyCount++;
                }
                ReadyCountText.text = "Ready : " + readyCount + " / " + PhotonNetwork.CurrentRoom.PlayerCount;
                if (readyCount >= 2 && readyCount >= PhotonNetwork.CurrentRoom.PlayerCount && PhotonNetwork.IsMasterClient)
                {
                    GetComponent<PhotonView>().RPC("StartGame", RpcTarget.All);
                }
            }
            else
            {
                ReadyCountText.text = "";
            }
        }
    }

    /// <summary>
    /// ��� �÷��̾ Ready�� �Ǿ��� �� �����Ͱ� ȣ���Ͽ� ������ �����Ѵ�.
    /// </summary>
    [PunRPC]
    public void StartGame()
    {
        if (PhotonNetwork.IsConnected)
        {
            // Room Setting, Player CustomProperty ����
            ExitGames.Client.Photon.Hashtable cp = PhotonNetwork.CurrentRoom.CustomProperties;
            cp["IsGameStarted"] = true;
            PhotonNetwork.CurrentRoom.SetCustomProperties(cp);
            PhotonNetwork.CurrentRoom.IsOpen = false;
            //PhotonNetwork.CurrentRoom.IsVisible = false;
            RoomPlayerCount.playerCount = PhotonNetwork.CountOfPlayers;

            PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "Ready", "0" } });

            // ���� ��� �� �ε�
            SceneManager.LoadScene("GameScene_Room");
        }
    }

    /// <summary>
    /// ������ ����Ǿ��� �� ȣ�� -> ����� ������ ����Ǹ� ��ΰ� �濡�� ������ ������ �������� ����
    /// </summary>
    [PunRPC]
    public void StopGameRPC()
    {
        if (PhotonNetwork.IsConnected)
        {
            // Room Setting, Player CustomProperty ����
            ExitGames.Client.Photon.Hashtable cp = PhotonNetwork.CurrentRoom.CustomProperties;
            cp["IsGameStarted"] = false;
            PhotonNetwork.CurrentRoom.SetCustomProperties(cp);
            PhotonNetwork.CurrentRoom.IsOpen = true;
            //PhotonNetwork.CurrentRoom.IsVisible = true;
        }
    }

    /// <summary>
    /// UI���� Connect Btn�� ������ �� ����
    /// </summary>
    public void Connect()
    {
        contractText.text = "���� �õ�";
        PhotonNetwork.ConnectUsingSettings();
    }
    public override void OnConnectedToMaster()
    {
        JoinLobby();

        contractText.text = "���� ��";

        SetNickName();
        MainPanel.SetActive(false);
    }

    /// <summary>
    /// ������ ������ �� UI �� �ʱ�ȭ
    /// </summary>
    public void Disconnect()
    {
        PhotonNetwork.Disconnect();
    }
    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);
        contractText.text = "���� ������";

        MainPanel.SetActive(true);
        LobbyPanel.SetActive(false);
        RoomPanel.SetActive(false);

        foreach (RoomListBtn rl in RoomListBtnLists)
        {
            if (rl != null)
            {
                Destroy(rl.gameObject);
            }
        }
        RoomListBtnLists.Clear();
    }

    // �κ� �����ϸ� �ش� Panel Active
    public void JoinLobby()
    {
        PhotonNetwork.JoinLobby();
    }
    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();
        contractText.text = "�κ� ���� ����" + myNickName;

        LobbyPanel.SetActive(true);
        RoomPanel.SetActive(false);
    }
    // �κ� ������ ���� ���� ����
    public void LeftLobby()
    {
        PhotonNetwork.LeaveLobby();
    }
    public override void OnLeftLobby()
    {
        base.OnLeftLobby();
        contractText.text = "�κ� ������";
    }

    // �� ����, ����, ���� �� ����
    public void CreateRoom()
    {
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.IsVisible = IsVisible;
        roomOptions.IsOpen = IsOpen;
        roomOptions.MaxPlayers = (byte)5;
        roomOptions.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable() { { "ReadyPlayerCount", 0 },  { "IsGameStarted", false }};
        roomOptions.CustomRoomPropertiesForLobby = new string[] {"IsGameStarted"};
        SetRoomName(PhotonNetwork.LocalPlayer.NickName + "'s Room");
        PhotonNetwork.CreateRoom(roomName, roomOptions);
    }
    public void JoinRoom()
    {
        PhotonNetwork.JoinRoom(roomName);
    }
    public void JoinOrCreateRoom()
    {
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.IsVisible = IsVisible;
        roomOptions.IsOpen = IsOpen;
        roomOptions.MaxPlayers = (byte)5;
        roomOptions.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable() { { "ReadyPlayerCount", 0 },  { "IsGameStarted", false }};
        roomOptions.CustomRoomPropertiesForLobby = new string[] {"IsGameStarted"};
        PhotonNetwork.JoinOrCreateRoom(roomName, roomOptions, null);
    }
    public void JoinRandomRoom()
    {
        PhotonNetwork.JoinRandomRoom();
    }
    public void LeaveRoom()
    {
        if (PhotonNetwork.IsConnected)
            PhotonNetwork.LeaveRoom();
        else
            RoomPanel.SetActive(false);
    }
    public override void OnCreatedRoom()
    {
        base.OnCreatedRoom();
        contractText.text = "�� ����� ����";

        // ���� �����Ǹ� ������ �÷��̾ �����Ͱ� �Ǿ RoomData�� ����
        PhotonNetwork.Instantiate("RoomData",  Vector3.zero, Quaternion.identity);
        // UI ������Ʈ�� ���� GameMode�� 0�� ������
        AddGameModeIndex(0);
    }
    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();

        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "Ready", "0" }, { "IsKicked", false } });
        contractText.text = "�� ���� ����";

        ChatUI.SetActive(true);
        ChatInputField.text = "";
        for (int i = 0; i < ChatTexts.Count; i++)
            ChatTexts[i].text = "";

        LobbyPanel.SetActive(false);
        RoomPanel.SetActive(true);

        // �̹� ������� �÷��̾� ����Ʈ�� �ִٸ� ����
        if (myPlayerListContent)
            PhotonNetwork.Destroy(myPlayerListContent.gameObject);
        foreach (PlayerListContent player in FindObjectsOfType<PlayerListContent>())
            Destroy(player.gameObject);



        // �÷��̾� ����Ʈ �ٽ� ���� �� ����ȭ
        if (myPlayerListContent == null)
        {
            myPlayerListContent = PhotonNetwork.Instantiate("PlayerListContent", PlayerListContainer.transform.position, Quaternion.identity).GetComponent<PlayerListContent>();
            //myPlayerListContent.myPlayer = PhotonNetwork.LocalPlayer;
        }

        // �����Ϳ��Ը� Option Setting ���� �ο�
        if (PhotonNetwork.IsConnected)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                ActiveOptionBlind(false);
            }
            else
            {
                ActiveOptionBlind(true);
            }
        }
    }
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        base.OnCreateRoomFailed(returnCode, message);
        contractText.text = "�� ���� ����";
    }
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        base.OnJoinRoomFailed(returnCode, message);
        contractText.text = "�� ����� ����";
    }
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        base.OnJoinRandomFailed(returnCode, message);
        contractText.text = "������ �� ���� ����";
    }
    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        contractText.text = "�� ������";

        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "Ready", "0" }, { "IsKicked", false }});

        // Lobby ��Ȳ�� ���缭 UI ����
        ChatUI.SetActive(false);
        LobbyPanel.SetActive(true);
        RoomPanel.SetActive(false);

        // �����Ǿ��ִ� �÷��̾��Ʈ ����
        if (myPlayerListContent)
            PhotonNetwork.Destroy(myPlayerListContent.gameObject);
        foreach (PlayerListContent player in FindObjectsOfType<PlayerListContent>())
            Destroy(player.gameObject);

        // ����ȭ�Ǿ� ������ RoomData LocalPlayer������ ����(�ٸ� �÷��̾�� ������� �� �����Ƿ�)
        RoomData.GetInstance().DestroyRoomData();
    }

    public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient)
    {
        debugLog = "���� �ٲ� to :" + newMasterClient.NickName;

        if (PhotonNetwork.IsConnected)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                ActiveOptionBlind(false);
            }
            else
            {
                ActiveOptionBlind(true);
            }
        }
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        print("OnRoomListUpdate");
        foreach (RoomInfo info in roomList)
        {
            if (info.RemovedFromList)
            {
                //roomList�� ������� list�� �ִٸ�

                int index = RoomListBtnLists.FindIndex(x => x.roomName == info.Name);
                if (index != -1)
                {
                    //������� roomList�� �� ����Ʈ�� �ִٸ� ����
                    Destroy(RoomListBtnLists[index].gameObject);
                    RoomListBtnLists.RemoveAt(index);
                }
            }
            else
            {
                // ��ȿ�� room LIst

                int index = RoomListBtnLists.FindIndex(x => x.roomName == info.Name);
                if (index == -1)
                {
                    //roomList�� �ִ� ���� �� ����Ʈ�� ���ٸ� ���� ������
                    RoomListBtn tmp = Instantiate(RoomListBtnPrefab, RoomListContainer).GetComponent<RoomListBtn>();
                    tmp.titleTxt.text = info.Name;
                    tmp.roomName = info.Name;
                    tmp.memberCountTxt.text = "[" + info.PlayerCount + "/" + info.MaxPlayers + "]";
                    RoomListBtnLists.Add(tmp);
                }
                else
                {
                    //roomList�� �ִ� ���� �� ����Ʈ�� �ִٸ� Update
                    if (info.PlayerCount != RoomListBtnLists[index].playerCount)
                    {
                        RoomListBtnLists[index].playerCount = info.PlayerCount;
                        RoomListBtnLists[index].memberCountTxt.text = "[" + info.PlayerCount + "/" + info.MaxPlayers + "]";
                    }
                }
            }

            int btnIndex = RoomListBtnLists.FindIndex(x => x.roomName == info.Name);
            if (btnIndex >= 0)
            {
                RoomListBtn tmpBtn = RoomListBtnLists[btnIndex];
                if ((bool)info.CustomProperties.ContainsKey("IsGameStarted"))
                {
                    // ������ �̹� �����߰ų� �÷��̾� ���� �ִ밡 �Ǹ� ���� �Ұ�
                    tmpBtn.GetComponent<Button>().interactable = ((bool)info.CustomProperties["IsGameStarted"] || info.MaxPlayers <= info.PlayerCount) ? false : true;
                }
            }
        }
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        Chat(newPlayer.NickName + "���� �����ϼ̽��ϴ�.");
    }
    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        Chat(otherPlayer.NickName + "���� �����ϼ̽��ϴ�.");
    }
    #endregion

    #region Room Setting
    public void SetNickName()
    {
        myNickName = SetNickNameInputField.text;
        if (PhotonNetwork.IsConnected)
            PhotonNetwork.NickName = myNickName;
    }
    public void SetRoomName(string name)
    {
        if (name != null && name != "")
            roomName = name;
    }
    public void SetVisible(bool visible)
    {
        IsVisible = visible;
    }
    public void SetOpen(bool open)
    {
        IsOpen = open;
    }
    public void ToggleReady()
    {
        if (PhotonNetwork.InRoom)
        {
            // Ready�� 1�̶�� Ready�� 0����, Ready�� 0�̶�� 1�� Toggle
            ExitGames.Client.Photon.Hashtable cp = PhotonNetwork.LocalPlayer.CustomProperties;
            cp["Ready"] = ((string)PhotonNetwork.LocalPlayer.CustomProperties["Ready"] == "0") ? "1" : "0";
            PhotonNetwork.LocalPlayer.SetCustomProperties(cp);
        }
    }
    public void KickPlayer(string id)
    {
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            if (PhotonNetwork.PlayerList[i].UserId == id)
            {
                // IsKicked�� �Ǹ� Update���� �ڵ����� LeaveRoom����
                PhotonNetwork.PlayerList[i].SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "Ready", "0" }, { "IsKicked", true } });
            }
        }
    }
    /// <summary>
    /// ������ �̿��� �ٸ� �÷��̾ Option�� �ǵ帮�� ���ϵ��� Raycast Block Image�� ����  
    /// </summary>
    public void ActiveOptionBlind(bool active)
    {
        OptionBlindForClient.SetActive(active);
    }

    public void AddGameModeIndex(int addAmount)
    {
        int resultIndex = (addAmount + (int)RoomData.GetInstance().gameMode);
        resultIndex = Mathf.Clamp(resultIndex,0, GameModeInfos.Length-1);
        if (PhotonNetwork.IsConnected == false)
        {
            RoomData.GetInstance().SetGameModeRPC(resultIndex);
            SetGameModeRPC();
        }
        else
        {
            if (PhotonNetwork.InRoom)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    // RoomData�� SetGameModeRPC�� Data ���� ����ȭ
                    RoomData.GetInstance().GetComponent<PhotonView>().RPC("SetGameModeRPC", RpcTarget.AllBuffered, resultIndex);
                    // NetworkController�� SetGameModeRPC�� UI ����ȭ
                    GetComponent<PhotonView>().RPC("SetGameModeRPC", RpcTarget.AllBuffered);
                }
            }
        }
    }

    /// <summary>
    /// GameMode�� ����Ǿ��� �� ��� Client�� UI�� ����ȭ�Ѵ�.
    /// </summary>
    [PunRPC]
    public void SetGameModeRPC()
    {
        print("(int)RoomData.GetInstance().gameMode " + (int)RoomData.GetInstance().gameMode);
        GameModeImage.sprite = GameModeInfos[(int)RoomData.GetInstance().gameMode].sprite;
        GameModeText.text = RoomData.GetInstance().gameMode.ToString();

        // ���� ���� ��忡 ���� Room�� �ٸ� ����Ʈ�� �߰��ϰ�ʹٸ� �Ʒ��� �ڵ带 ���
        switch (RoomData.GetInstance().gameMode)
        {
            case GameMode.BattleRoyale:
                break;
            case GameMode.PassTheBomb:
                break;
            case GameMode.Survivor:
                break;
        }
    }


    [System.Obsolete]
    public void AddMaxPlayers(int _add)
    {
        if (PhotonNetwork.CurrentRoom.MaxPlayers + (byte)_add >= (byte)1 && PhotonNetwork.CurrentRoom.MaxPlayers + (byte)_add <= (byte)5)
        {
            PhotonNetwork.CurrentRoom.MaxPlayers += (byte)_add;
        }
    }
    #endregion

    #region Chat
    public void ChattedByPlayer()
    {
        if (ChatInputField.text == "")
        {
            ChatInputField.DeactivateInputField();
        }
        else
        {
            Chat(PhotonNetwork.LocalPlayer.NickName + " : " + ChatInputField.text);
            ChatInputField.text = "";
            ChatInputField.ActivateInputField();
            ChatInputField.Select();
        }
    }

    public void Chat(string msg)
    {
        GetComponent<PhotonView>().RPC("ChatRPC", RpcTarget.All, msg);
    }

    [PunRPC]
    void ChatRPC(string msg)
    {
        bool isInput = false;
        for (int i = 0; i < ChatTexts.Count; i++)
        {
            if (ChatTexts[i].text == "")
            {
                isInput = true;
                ChatTexts[i].text = msg;
                break;
            }
        }
        if (!isInput)
        {
            for (int i = 1; i < ChatTexts.Count; i++)
            {
                ChatTexts[i - 1].text = ChatTexts[i].text;
            }
            ChatTexts[ChatTexts.Count - 1].text = msg;
        }
    }
    #endregion


    void OnGUI() { GUILayout.Label(debugLog); }

}
