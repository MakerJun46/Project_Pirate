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
        // Scene�� �ҷ����� �� ������ Panel Active �� Player�г��� ����
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
        // �̰� �ص� �� �� Ŭ���� �ؾ� ������ ��.. -> �׷��ٿ��� �������� �������� ����

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

        // ���� ��� �� �ε�
        if(PhotonNetwork.IsMasterClient)
            SceneManager.LoadScene("GameScene_Room");
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

        OptionPanel.SetActive(true);
        OptionPanel.transform.GetChild(0).gameObject.SetActive(true);
        OptionPanel.transform.GetChild(1).gameObject.SetActive(true);
        OptionPanel.transform.GetChild(2).gameObject.SetActive(true);

        MainPanel.SetActive(false);
        TitleImg.SetActive(false);
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
        
        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "Ready", "0" }, { "IsKicked", false }, { "ProfileIndex", "0" } });

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

    public void ActiveCreateRoomPanel()
    {
        CreateRoomPanel.SetActive(true);
    }
    // �� ����, ����, ���� �� ����
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
        contractText.text = "�� ����� ����";

        // ���� �����Ǹ� ������ �÷��̾ �����Ͱ� �Ǿ RoomData�� ����
        GameObject tmpRoomData = PhotonNetwork.Instantiate("RoomData",  Vector3.zero, Quaternion.identity);
        // UI ������Ʈ�� ���� GameMode�� 0�� ������
        AddGameModeIndex(0);
    }
    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();

        PhotonNetwork.LocalPlayer.CustomProperties["Ready"] = "0";
        PhotonNetwork.LocalPlayer.CustomProperties["IsKicked"] = false;
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
            ActiveGameModeAdjustBtns(PhotonNetwork.IsMasterClient);
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

        PhotonNetwork.LocalPlayer.CustomProperties["Ready"] = "0";
        PhotonNetwork.LocalPlayer.CustomProperties["IsKicked"] = false;

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
        //if (RoomData.GetInstance())
        //{
        //RoomData.GetInstance().GetComponent<Photon.Pun.PhotonView>().TransferOwnership(PhotonNetwork.MasterClient);
        // RoomData.GetInstance().DestroyRoomData();
        //}
    }

    public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient)
    {
        debugLog = "���� �ٲ� to :" + newMasterClient.NickName;
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
                //roomList�� ������� list�� �ִٸ�

                int index = RoomListBtnLists.FindIndex(x => x.TitleEqualTo(info.Name));
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
                int index = RoomListBtnLists.FindIndex(x => x.TitleEqualTo(info.Name));
                if (index == -1)
                {
                    //roomList�� �ִ� ���� �� ����Ʈ�� ���ٸ� ���� ������
                    RoomListBtn tmp = Instantiate(RoomListBtnPrefab, RoomListContainer).GetComponent<RoomListBtn>();
                    tmp.InitializeRoomLIstInfo(info.Name, info.PlayerCount, info.MaxPlayers);
                    RoomListBtnLists.Add(tmp);
                }
                else
                {
                    //roomList�� �ִ� ���� �� ����Ʈ�� �ִٸ� Update
                    RoomListBtnLists[index].InitializeRoomLIstInfo(info.Name, info.PlayerCount, info.MaxPlayers);
                }
            }

            int btnIndex = RoomListBtnLists.FindIndex(x => x.TitleEqualTo(info.Name));
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
                PhotonNetwork.PlayerList[i].CustomProperties["Ready"] = "0";
                PhotonNetwork.PlayerList[i].CustomProperties["IsKicked"] = true;
            }
        }
    }
    /// <summary>
    /// ������ �̿��� �ٸ� �÷��̾ Option�� �ǵ帮�� ���ϵ��� Raycast Block Image�� ����  
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
    /// GameMode�� ����Ǿ��� �� ��� Client�� UI�� ����ȭ�Ѵ�.
    /// </summary>
    [PunRPC]
    public void SetGameModeRPC()
    {
        GameModeImage.sprite = GameModeInfos[RoomData.GetInstance().gameMode].sprite;
        GameModeText.text = GameModeInfos[RoomData.GetInstance().gameMode].title;

        // ���� ���� ��忡 ���� Room�� �ٸ� ����Ʈ�� �߰��ϰ�ʹٸ� �Ʒ��� �ڵ带 ���
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
