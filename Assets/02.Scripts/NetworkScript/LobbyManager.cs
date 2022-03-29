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
        // Scene을 불러왔을 때 적절한 Panel Active 및 Player닉네임 설정
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
                // IsKicked라는 CustomProperty를 가지고있고, 그것이 True라면, 강제 퇴장
                if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("IsKicked"))
                {
                    if ((bool)PhotonNetwork.LocalPlayer.CustomProperties["IsKicked"])
                    {
                        LeaveRoom();
                    }
                }

                // Ready를 한 사람의 수가 CurrentRoom의 플레이어 인원수보다 많다면 게임 시작(마스터가 한 번 실행)
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
    /// 모든 플레이어가 Ready가 되었을 때 마스터가 호출하여 게임을 시작한다.
    /// </summary>
    [PunRPC]
    public void StartGame()
    {
        if (PhotonNetwork.IsConnected)
        {
            // Room Setting, Player CustomProperty 변경
            ExitGames.Client.Photon.Hashtable cp = PhotonNetwork.CurrentRoom.CustomProperties;
            cp["IsGameStarted"] = true;
            PhotonNetwork.CurrentRoom.SetCustomProperties(cp);
            PhotonNetwork.CurrentRoom.IsOpen = false;
            //PhotonNetwork.CurrentRoom.IsVisible = false;
            RoomPlayerCount.playerCount = PhotonNetwork.CountOfPlayers;

            PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "Ready", "0" } });

            // 게임 대기 씬 로드
            SceneManager.LoadScene("GameScene_Room");
        }
    }

    /// <summary>
    /// 게임이 종료되었을 때 호출 -> 현재는 게임이 종료되면 모두가 방에서 나가기 때문에 쓰이지는 않음
    /// </summary>
    [PunRPC]
    public void StopGameRPC()
    {
        if (PhotonNetwork.IsConnected)
        {
            // Room Setting, Player CustomProperty 변경
            ExitGames.Client.Photon.Hashtable cp = PhotonNetwork.CurrentRoom.CustomProperties;
            cp["IsGameStarted"] = false;
            PhotonNetwork.CurrentRoom.SetCustomProperties(cp);
            PhotonNetwork.CurrentRoom.IsOpen = true;
            //PhotonNetwork.CurrentRoom.IsVisible = true;
        }
    }

    /// <summary>
    /// UI에서 Connect Btn을 눌렀을 때 실행
    /// </summary>
    public void Connect()
    {
        contractText.text = "연결 시도";
        PhotonNetwork.ConnectUsingSettings();
    }
    public override void OnConnectedToMaster()
    {
        JoinLobby();

        contractText.text = "연결 됨";

        SetNickName();
        MainPanel.SetActive(false);
    }

    /// <summary>
    /// 접속이 끊겼을 때 UI 등 초기화
    /// </summary>
    public void Disconnect()
    {
        PhotonNetwork.Disconnect();
    }
    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);
        contractText.text = "연결 끊어짐";

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

    // 로비에 접속하면 해당 Panel Active
    public void JoinLobby()
    {
        PhotonNetwork.JoinLobby();
    }
    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();
        contractText.text = "로비 접속 성공" + myNickName;

        LobbyPanel.SetActive(true);
        RoomPanel.SetActive(false);
    }
    // 로비를 나가는 경우는 거의 없음
    public void LeftLobby()
    {
        PhotonNetwork.LeaveLobby();
    }
    public override void OnLeftLobby()
    {
        base.OnLeftLobby();
        contractText.text = "로비 나가기";
    }

    // 방 생성, 입장, 랜덤 방 입장
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
        contractText.text = "방 만들기 성공";

        // 방이 생성되면 생성한 플레이어가 마스터가 되어서 RoomData를 생성
        PhotonNetwork.Instantiate("RoomData",  Vector3.zero, Quaternion.identity);
        // UI 업데이트를 위해 GameMode에 0을 더해줌
        AddGameModeIndex(0);
    }
    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();

        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "Ready", "0" }, { "IsKicked", false } });
        contractText.text = "방 입장 성공";

        ChatUI.SetActive(true);
        ChatInputField.text = "";
        for (int i = 0; i < ChatTexts.Count; i++)
            ChatTexts[i].text = "";

        LobbyPanel.SetActive(false);
        RoomPanel.SetActive(true);

        // 이미 만들어진 플레이어 리스트가 있다면 삭제
        if (myPlayerListContent)
            PhotonNetwork.Destroy(myPlayerListContent.gameObject);
        foreach (PlayerListContent player in FindObjectsOfType<PlayerListContent>())
            Destroy(player.gameObject);



        // 플레이어 리스트 다시 생성 및 동기화
        if (myPlayerListContent == null)
        {
            myPlayerListContent = PhotonNetwork.Instantiate("PlayerListContent", PlayerListContainer.transform.position, Quaternion.identity).GetComponent<PlayerListContent>();
            //myPlayerListContent.myPlayer = PhotonNetwork.LocalPlayer;
        }

        // 마스터에게만 Option Setting 권한 부여
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
        contractText.text = "방 생성 실패";
    }
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        base.OnJoinRoomFailed(returnCode, message);
        contractText.text = "방 만들기 실패";
    }
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        base.OnJoinRandomFailed(returnCode, message);
        contractText.text = "랜덤한 방 입장 실패";
    }
    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        contractText.text = "방 나가기";

        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "Ready", "0" }, { "IsKicked", false }});

        // Lobby 상황에 맞춰서 UI 세팅
        ChatUI.SetActive(false);
        LobbyPanel.SetActive(true);
        RoomPanel.SetActive(false);

        // 생성되어있는 플레이어리스트 삭제
        if (myPlayerListContent)
            PhotonNetwork.Destroy(myPlayerListContent.gameObject);
        foreach (PlayerListContent player in FindObjectsOfType<PlayerListContent>())
            Destroy(player.gameObject);

        // 동기화되어 생성된 RoomData LocalPlayer에서만 삭제(다른 플레이어는 사용중일 수 있으므로)
        RoomData.GetInstance().DestroyRoomData();
    }

    public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient)
    {
        debugLog = "방장 바뀜 to :" + newMasterClient.NickName;

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
                //roomList에 쓸모없는 list가 있다면

                int index = RoomListBtnLists.FindIndex(x => x.roomName == info.Name);
                if (index != -1)
                {
                    //쓸모없는 roomList가 내 리스트에 있다면 지움
                    Destroy(RoomListBtnLists[index].gameObject);
                    RoomListBtnLists.RemoveAt(index);
                }
            }
            else
            {
                // 유효한 room LIst

                int index = RoomListBtnLists.FindIndex(x => x.roomName == info.Name);
                if (index == -1)
                {
                    //roomList에 있는 것이 내 리스트에 없다면 방을 생성함
                    RoomListBtn tmp = Instantiate(RoomListBtnPrefab, RoomListContainer).GetComponent<RoomListBtn>();
                    tmp.titleTxt.text = info.Name;
                    tmp.roomName = info.Name;
                    tmp.memberCountTxt.text = "[" + info.PlayerCount + "/" + info.MaxPlayers + "]";
                    RoomListBtnLists.Add(tmp);
                }
                else
                {
                    //roomList에 있는 것이 내 리스트에 있다면 Update
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
                    // 게임이 이미 시작했거나 플레이어 수가 최대가 되면 입장 불가
                    tmpBtn.GetComponent<Button>().interactable = ((bool)info.CustomProperties["IsGameStarted"] || info.MaxPlayers <= info.PlayerCount) ? false : true;
                }
            }
        }
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        Chat(newPlayer.NickName + "님이 입장하셨습니다.");
    }
    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        Chat(otherPlayer.NickName + "님이 퇴장하셨습니다.");
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
            // Ready가 1이라면 Ready를 0으로, Ready가 0이라면 1로 Toggle
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
                // IsKicked가 되면 Update에서 자동으로 LeaveRoom실행
                PhotonNetwork.PlayerList[i].SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "Ready", "0" }, { "IsKicked", true } });
            }
        }
    }
    /// <summary>
    /// 마스터 이외의 다른 플레이어가 Option을 건드리지 못하도록 Raycast Block Image로 가림  
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
                    // RoomData의 SetGameModeRPC는 Data 변경 동기화
                    RoomData.GetInstance().GetComponent<PhotonView>().RPC("SetGameModeRPC", RpcTarget.AllBuffered, resultIndex);
                    // NetworkController의 SetGameModeRPC는 UI 동기화
                    GetComponent<PhotonView>().RPC("SetGameModeRPC", RpcTarget.AllBuffered);
                }
            }
        }
    }

    /// <summary>
    /// GameMode가 변경되었을 때 모든 Client의 UI를 동기화한다.
    /// </summary>
    [PunRPC]
    public void SetGameModeRPC()
    {
        print("(int)RoomData.GetInstance().gameMode " + (int)RoomData.GetInstance().gameMode);
        GameModeImage.sprite = GameModeInfos[(int)RoomData.GetInstance().gameMode].sprite;
        GameModeText.text = RoomData.GetInstance().gameMode.ToString();

        // 만약 게임 모드에 따라 Room에 다른 이펙트를 추가하고싶다면 아래의 코드를 사용
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
