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
        public string title;
        public Sprite sprite;
    }

    [SerializeField] string myNickName;
    [SerializeField] int myProfileIndex;

    [Header("[Room Info]")]
    public string roomName;
    public bool IsVisible = true;
    public bool IsOpen = true;
    private int selectedMaxPlayerCount=4;

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
    [SerializeField] GameObject TitleAnim;
    [SerializeField] Text contractText;

    [SerializeField] GameObject MainPanel;
    [SerializeField] GameObject OptionPanel;
    [SerializeField] GameObject TitleImg;
    [SerializeField] GameObject LobbyPanel;
    [SerializeField] GameObject RoomPanel;
    [SerializeField] GameObject LoadingFadeOutPanel;
    [SerializeField] GameObject CreateRoomPanel;

    [SerializeField] Text ReadyCountText;
    [SerializeField] List<GameObject> gameModeAdjustBtns;
    [SerializeField] InputField SetNickNameInputField;

    [SerializeField] Text TipTxt;
    [SerializeField] string[] tipTextStrings;
    int tipTxtIndex;

    [Header("[Player]")]
    [SerializeField] List<string> playerNameExamples;
    [SerializeField] List<Sprite> playerProfileExamples;

    [SerializeField] Transform playerProfileBtnContainer;
    [SerializeField] Text NickNameText;
    [SerializeField] Image profileImg;

    [Header("[Chat]")]
    [SerializeField] GameObject ChatUI;
    [SerializeField] InputField ChatInputField;
    [SerializeField] List<Text> ChatTexts;

    private string debugLog;


    [SerializeField] RectTransform parent;
    [SerializeField] GridLayoutGroup grid;
    private float originWidth, originHeight;
    public void SetDynamicGrid(int cnt, int minColsInARow, int maxRow)
    {
        originWidth = parent.rect.width;
        originHeight = parent.rect.height;

        int rows = Mathf.Clamp(Mathf.CeilToInt((float)cnt / minColsInARow), 1, maxRow + 1);
        int cols = Mathf.CeilToInt((float)cnt / rows);

        float spaceW = (grid.padding.left + grid.padding.right) + (grid.spacing.x * (cols - 1));
        float spaceH = (grid.padding.top + grid.padding.bottom) + (grid.spacing.y * (rows - 1));

        float maxWidth = originWidth - spaceW;
        float maxHeight = originHeight - spaceH;

        float width = Mathf.Min(parent.rect.width - (grid.padding.left + grid.padding.right) - (grid.spacing.x * (cols - 1)), maxWidth);
        float height = Mathf.Min(parent.rect.height - (grid.padding.top + grid.padding.bottom) - (grid.spacing.y * (rows - 1)), maxHeight);
        grid.cellSize = new Vector2(width / cols, height / rows);
    }

    private void Awake()
    {
#if UNITY_EDITOR_WIN
        Screen.SetResolution(960, 540, false);
#endif
        PhotonNetwork.SendRate = 60;
        PhotonNetwork.SerializationRate = 30;
        PhotonNetwork.AutomaticallySyncScene = true;
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
        SetNickNameInputField.text = PhotonNetwork.IsConnected ? PhotonNetwork.LocalPlayer.NickName : playerNameExamples[Random.Range(0, playerNameExamples.Count)]+ Random.Range(0, 100);
        NickNameText.text = SetNickNameInputField.text;

        for (int i = 0; i < playerProfileExamples.Count; i++)
        {
            GameObject tmpBtn = Instantiate(playerProfileBtnContainer.GetChild(0).gameObject, playerProfileBtnContainer);
            int tmpIndex = i;
            tmpBtn.SetActive(true);
            tmpBtn.GetComponent<Button>().onClick.AddListener(() => SetProfileIndex(tmpIndex));
            tmpBtn.transform.GetChild(0).GetComponent<Image>().sprite = playerProfileExamples[tmpIndex];
        }
        /*
        for (int i = 0; i < playerProfileExamples.Count; i++)
        {

            GameObject tmpBtn = Instantiate(playerProfileBtnContainer2.GetChild(0).gameObject, playerProfileBtnContainer2);
            int tmpIndex = i;
            tmpBtn.SetActive(true);
            tmpBtn.GetComponent<Button>().onClick.AddListener(() => SetProfileIndex(tmpIndex));
            tmpBtn.transform.GetChild(0).GetComponent<Image>().sprite = playerProfileExamples[tmpIndex];
        }
        */
        //SetProfileIndex(Random.Range(0, playerProfileExamples.Count));
        // 이거 해도 한 번 클릭을 해야 적용이 됨.. -> 그럴바에는 랜덤으로 정해주지 말자

        tipTxtIndex = Random.Range(0, tipTextStrings.Length);
        StartCoroutine("TipTextCoroutine");
    }

    IEnumerator TipTextCoroutine()
    {
        tipTxtIndex++;
        tipTxtIndex = tipTxtIndex % tipTextStrings.Length;

        TipTxt.text = tipTextStrings[tipTxtIndex];
        yield return new WaitForSeconds(8f);
        StartCoroutine("TipTextCoroutine");
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
                if (readyCount >= 3 && readyCount >= PhotonNetwork.CurrentRoom.PlayerCount && PhotonNetwork.IsMasterClient)
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

            PhotonNetwork.LocalPlayer.CustomProperties["Ready"] = "0";

            StartCoroutine(FadeOut_beforeLoadScene());
        }
    }

    IEnumerator FadeOut_beforeLoadScene()
    {
        LoadingFadeOutPanel.SetActive(true);

        Color c = LoadingFadeOutPanel.GetComponent<Image>().color;

        while(c.a < 1)
        {
            c.a += 0.01f;

            LoadingFadeOutPanel.GetComponent<Image>().color = c;

            yield return new WaitForSeconds(0.01f);
        }

        // 게임 대기 씬 로드
        if(PhotonNetwork.IsMasterClient)
            SceneManager.LoadScene("GameScene_Room");
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

        OptionPanel.SetActive(true);
        OptionPanel.transform.GetChild(0).gameObject.SetActive(true);
        OptionPanel.transform.GetChild(1).gameObject.SetActive(true);
        OptionPanel.transform.GetChild(2).gameObject.SetActive(true);

        MainPanel.SetActive(false);
        TitleImg.SetActive(false);
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
        
        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "Ready", "0" }, { "IsKicked", false }, { "ProfileIndex", "0" } });

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

    public void ActiveCreateRoomPanel()
    {
        CreateRoomPanel.SetActive(true);
    }
    // 방 생성, 입장, 랜덤 방 입장
    public void CreateRoom()
    {
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.IsVisible = IsVisible;
        roomOptions.IsOpen = IsOpen;
        roomOptions.MaxPlayers = (byte)(selectedMaxPlayerCount+1);


        PhotonNetwork.LocalPlayer.CustomProperties["Ready"] = "0";
        PhotonNetwork.LocalPlayer.CustomProperties["IsKicked"] = false;
        roomOptions.CustomRoomPropertiesForLobby = new string[] {"IsGameStarted"};
        roomOptions.CleanupCacheOnLeave = false;
        SelectCreatingRoomName(roomName);
        PhotonNetwork.CreateRoom(roomName, roomOptions);

        CreateRoomPanel.SetActive(false);
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


        PhotonNetwork.LocalPlayer.CustomProperties["Ready"] = "0";
        PhotonNetwork.LocalPlayer.CustomProperties["IsKicked"] = false;
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
        GameObject tmpRoomData = PhotonNetwork.Instantiate("RoomData",  Vector3.zero, Quaternion.identity);
        // UI 업데이트를 위해 GameMode에 0을 더해줌
        AddGameModeIndex(0);
    }
    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();

        PhotonNetwork.LocalPlayer.CustomProperties["Ready"] = "0";
        PhotonNetwork.LocalPlayer.CustomProperties["IsKicked"] = false;
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
            ActiveGameModeAdjustBtns(PhotonNetwork.IsMasterClient);
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

        PhotonNetwork.LocalPlayer.CustomProperties["Ready"] = "0";
        PhotonNetwork.LocalPlayer.CustomProperties["IsKicked"] = false;

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
        //if (RoomData.GetInstance())
        //{
        //RoomData.GetInstance().GetComponent<Photon.Pun.PhotonView>().TransferOwnership(PhotonNetwork.MasterClient);
        // RoomData.GetInstance().DestroyRoomData();
        //}
    }

    public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient)
    {
        debugLog = "방장 바뀜 to :" + newMasterClient.NickName;
        if (RoomData.GetInstance())
        {
            RoomData.GetInstance().GetComponent<Photon.Pun.PhotonView>().TransferOwnership(PhotonNetwork.MasterClient);
        }
        if (PhotonNetwork.IsConnected)
        {
            ActiveGameModeAdjustBtns(PhotonNetwork.IsMasterClient);
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

                int index = RoomListBtnLists.FindIndex(x => x.TitleEqualTo(info.Name));
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
                int index = RoomListBtnLists.FindIndex(x => x.TitleEqualTo(info.Name));
                if (index == -1)
                {
                    //roomList에 있는 것이 내 리스트에 없다면 방을 생성함
                    RoomListBtn tmp = Instantiate(RoomListBtnPrefab, RoomListContainer).GetComponent<RoomListBtn>();
                    tmp.InitializeRoomLIstInfo(info.Name, info.PlayerCount, info.MaxPlayers);
                    RoomListBtnLists.Add(tmp);
                }
                else
                {
                    //roomList에 있는 것이 내 리스트에 있다면 Update
                    RoomListBtnLists[index].InitializeRoomLIstInfo(info.Name, info.PlayerCount, info.MaxPlayers);
                }
            }

            int btnIndex = RoomListBtnLists.FindIndex(x => x.TitleEqualTo(info.Name));
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
    public void SelectMaxPlayer(int val)
    {
        selectedMaxPlayerCount = val;
    }
    public void SelectCreatingRoomName(string val)
    {
        if (string.IsNullOrEmpty(val))
        {
            roomName = PhotonNetwork.LocalPlayer.NickName + "'s Room";
        }
        else
        {
            roomName = val;
        }
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
                PhotonNetwork.PlayerList[i].CustomProperties["Ready"] = "0";
                PhotonNetwork.PlayerList[i].CustomProperties["IsKicked"] = true;
            }
        }
    }
    /// <summary>
    /// 마스터 이외의 다른 플레이어가 Option을 건드리지 못하도록 Raycast Block Image로 가림  
    /// </summary>
    public void ActiveGameModeAdjustBtns(bool active)
    {
        for(int i=0;i< gameModeAdjustBtns.Count; i++)
        {
            gameModeAdjustBtns[i].gameObject.SetActive(active);
        }
    }

    public void AddGameModeIndex(int addAmount)
    {
        RoomData.GetInstance().AddGameModeIndex(addAmount);
    }


    public void ToggleRandomGameMode(bool val)
    {
        RoomData.GetInstance().ToggleRandomGameMode(val);
    }

    /// <summary>
    /// GameMode가 변경되었을 때 모든 Client의 UI를 동기화한다.
    /// </summary>
    [PunRPC]
    public void SetGameModeRPC()
    {
        GameModeImage.sprite = GameModeInfos[RoomData.GetInstance().gameMode].sprite;
        GameModeText.text = GameModeInfos[RoomData.GetInstance().gameMode].title;

        // 만약 게임 모드에 따라 Room에 다른 이펙트를 추가하고싶다면 아래의 코드를 사용
        switch ((GameMode)RoomData.GetInstance().gameMode)
        {
            case GameMode.BattleRoyale:
                break;
            case GameMode.PassTheBomb:
                break;
            case GameMode.Survivor:
                break;
            case GameMode.HitTheTarget:
                break;
            default:
                break;
        }
        UpdateameModeInfo();
    }

    [SerializeField] Text GameModeInfoTxt;
    public void UpdateameModeInfo()
    {
        GameModeInfoTxt.text= RoomData.GetInstance().GetCurrGameModeInfo();
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

    #region PlayerSetting
    public void SetNickName()
    {
        myNickName = SetNickNameInputField.text;
        NickNameText.text = myNickName;
        if (PhotonNetwork.IsConnected)
            PhotonNetwork.NickName = myNickName;
    }
    public void SetProfileIndex(int _index)
    {
        playerProfileBtnContainer.GetChild(1 + myProfileIndex).GetChild(1).gameObject.SetActive(false);
        //playerProfileBtnContainer2.GetChild(1 + myProfileIndex).GetChild(1).gameObject.SetActive(false);
        myProfileIndex = _index;
        profileImg.sprite = playerProfileExamples[myProfileIndex];
        playerProfileBtnContainer.GetChild(1 + myProfileIndex).GetChild(1).gameObject.SetActive(true);
        //playerProfileBtnContainer2.GetChild(1 + myProfileIndex).GetChild(1).gameObject.SetActive(true);
        PhotonNetwork.LocalPlayer.CustomProperties["ProfileIndex"] = myProfileIndex.ToString();
        print("Set Profile To : " + myProfileIndex);
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
        for (int i = ChatTexts.Count - 1; i > 0; i--)
        {
            ChatTexts[i].text = ChatTexts[i - 1].text;
        }
        ChatTexts[0].text = msg;
    }
#endregion


    void OnGUI() { GUILayout.Label(debugLog); }

}
