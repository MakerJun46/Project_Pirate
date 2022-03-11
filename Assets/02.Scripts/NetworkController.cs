using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine.SceneManagement;

public enum GameMode
{
    BattleRoyale,
    PassTheBomb,
    Survivor,
    BossRaid,
}

public class NetworkController : MonoBehaviourPunCallbacks
{
    [SerializeField] string myNickName;
    public string roomName;
    public bool IsVisible = true;
    public bool IsOpen = true;

    [SerializeField] Transform RoomListContainer;
    public GameObject RoomListBtnPrefab;

    public Transform PlayerListContainer;
    public PlayerListContent myPlayerListContent;

    string debugLog;

    public List<RoomListBtn> _Listings = new List<RoomListBtn>();

    [SerializeField] Animator TitleAnim;
    [SerializeField] Text contractText;

    [SerializeField] InputField SetNickNameInputField;

    [SerializeField] GameObject ChatUI;
    [SerializeField] InputField ChatInputField;
    [SerializeField] List<Text> ChatTexts;

    [SerializeField] GameObject MainPanel;
    [SerializeField] GameObject LobbyPanel;
    [SerializeField] GameObject RoomPanel;


    [SerializeField] Text ReadyCountText;

    [SerializeField] GameObject OptionBlindForClient;

    [System.Serializable]
    public struct GameModeInfo{
        public Sprite sprite;
    }

    [SerializeField] Image GameModeImage;
    [SerializeField] Text GameModeText;
    [SerializeField] GameModeInfo[] GameModeInfos;

    public GameMode gameMode = 0; //0:배틀로얄 1:폭탄돌리기 2: 몬스터피하기

    private void Awake()
    {
        PhotonNetwork.SendRate = 60;
        PhotonNetwork.SerializationRate = 30;
    }
    private void Start()
    {
        if (PhotonNetwork.IsConnected)
        {
            if (PhotonNetwork.CurrentRoom != null)
            {
                MainPanel.SetActive(false);
                LobbyPanel.SetActive(true);
                RoomPanel.SetActive(false);
                LeaveRoom();
            }
            SetNickNameInputField.text = PhotonNetwork.LocalPlayer.NickName;
        }
        else
        {
            SetNickNameInputField.text = "Guest"+Random.Range(0,1000);
        }
    }
    private void Update()
    {
        if (PhotonNetwork.IsConnected && PhotonNetwork.LocalPlayer != null)
        {
            if ((bool)PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("IsKicked"))
            {
                if ((bool)PhotonNetwork.LocalPlayer.CustomProperties["IsKicked"] && PhotonNetwork.InRoom)
                {
                    LeaveRoom();
                }
            }

            if (PhotonNetwork.InRoom)
            {
                int readyCount = 0;
                foreach (var p in PhotonNetwork.PlayerList)
                {
                    if ((string)p.CustomProperties["Ready"] == "1")
                        readyCount++;
                }

                ReadyCountText.text = "Ready : " + readyCount + " / " + PhotonNetwork.CurrentRoom.PlayerCount;
                if (readyCount >= PhotonNetwork.CurrentRoom.PlayerCount && PhotonNetwork.IsMasterClient && readyCount >= 1)
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

    [System.Obsolete]
    public void AddMaxPlayers(int _add)
    {
        if (PhotonNetwork.CurrentRoom.MaxPlayers + (byte)_add >= (byte)1 && PhotonNetwork.CurrentRoom.MaxPlayers + (byte)_add <= (byte)4)
        {
            PhotonNetwork.CurrentRoom.MaxPlayers += (byte)_add;
        }
    }

    public void ConnectOfflineMode()
    {
        RoomPanel.SetActive(true);
    }

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

        foreach (RoomListBtn rl in _Listings)
        {
            if (rl != null)
            {
                Destroy(rl.gameObject);
            }
        }
        _Listings.Clear();
    }


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
    public void LeftLobby()
    {
        PhotonNetwork.LeaveLobby();
    }
    public override void OnLeftLobby()
    {
        base.OnLeftLobby();
        contractText.text = "로비 나가기";
    }

    public void CreateRoom()
    {
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.IsVisible = IsVisible;
        roomOptions.IsOpen = IsOpen;
        roomOptions.MaxPlayers = (byte)4;
        roomOptions.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable() { { "ReadyPlayerCount", 0 },  { "IsGameStarted", false }, { "GameModeIndex", 0 } };
        roomOptions.CustomRoomPropertiesForLobby = new string[] { "IsGameStarted" , "GameModeIndex" };
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
        roomOptions.MaxPlayers = (byte)4;
        roomOptions.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable() { { "ReadyPlayerCount", 0 },  { "IsGameStarted", false }, { "GameModeIndex", 0 } };
        roomOptions.CustomRoomPropertiesForLobby = new string[] { "IsGameStarted", "GameModeIndex" };
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
        AddGameModeIndex(0);
    }
    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        //GameManager.GetInstance().SetGameModeRPC(System.Convert.ToInt16(PhotonNetwork.CurrentRoom.CustomProperties["GameModeIndex"]));
        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "Ready", "0" }, { "IsKicked", false } });
        contractText.text = "방 입장 성공";

        ChatUI.SetActive(true);
        ChatInputField.text = "";
        for (int i = 0; i < ChatTexts.Count; i++)
        {
            ChatTexts[i].text = "";
        }

        LobbyPanel.SetActive(false);
        RoomPanel.SetActive(true);

        if (myPlayerListContent)
            PhotonNetwork.Destroy(myPlayerListContent.gameObject);
        foreach (PlayerListContent player in FindObjectsOfType<PlayerListContent>())
        {
            Destroy(player.gameObject);
        }
        if (myPlayerListContent == null)
        {
            myPlayerListContent = PhotonNetwork.Instantiate("PlayerListContent", PlayerListContainer.transform.position, Quaternion.identity).GetComponent<PlayerListContent>();
            myPlayerListContent.myPlayer = PhotonNetwork.LocalPlayer;
        }

        if (PhotonNetwork.IsConnected)
        {
            ActiveOptionBlind(true);
            if (PhotonNetwork.IsMasterClient)
            {
                ActiveOptionBlind(false);
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

        ChatUI.SetActive(false);

        LobbyPanel.SetActive(true);
        RoomPanel.SetActive(false);

        if (myPlayerListContent)
            PhotonNetwork.Destroy(myPlayerListContent.gameObject);
        foreach (PlayerListContent player in FindObjectsOfType<PlayerListContent>())
        {
            Destroy(player.gameObject);
        }
    }

    public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient)
    {
        debugLog = "방장 바뀜 to :" + newMasterClient.NickName;

        if (PhotonNetwork.IsConnected)
        {
            ActiveOptionBlind(true);
            if (PhotonNetwork.IsMasterClient)
            {
                ActiveOptionBlind(false);
            }
        }
    }
    public void ActiveOptionBlind(bool active)
    {
        OptionBlindForClient.SetActive(active);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        print("OnRoomListUpdate");
        foreach (RoomInfo info in roomList)
        {
            print("RoomInfo loop");
            if (info.RemovedFromList)
            {
                print("info.RemovedFromList");
                //roomList에 쓸모없는 list가 있다면

                int index = _Listings.FindIndex(x => x.roomName == info.Name);
                if (index != -1)
                {
                    //roomList에 내 리스트에 있는것이 없다면, 내꺼 지움
                    Destroy(_Listings[index].gameObject);
                    _Listings.RemoveAt(index);
                }
                else
                {
                    /*
                    RoomListBtn tmp = Instantiate(RoomListBtnPrefab, RoomListContainer).GetComponent<RoomListBtn>();
                    tmp.titleTxt.text = info.Name;
                    tmp.roomName = info.Name;
                    tmp.memberCountTxt.text = "[" + info.PlayerCount + "/" + info.MaxPlayers + "]";
                    _Listings.Add(tmp);
                    */
                }
            }
            else
            {
                print("Valid Room");
                int index = _Listings.FindIndex(x => x.roomName == info.Name);
                if (index == -1)
                {
                    //roomList에 내 리스트에 있는것이 있다면 방을 생성함
                    RoomListBtn tmp = Instantiate(RoomListBtnPrefab, RoomListContainer).GetComponent<RoomListBtn>();
                    tmp.titleTxt.text = info.Name;
                    tmp.roomName = info.Name;
                    tmp.memberCountTxt.text = "[" + info.PlayerCount + "/" + info.MaxPlayers + "]";
                    _Listings.Add(tmp);
                }
                else
                {
                    if (info.PlayerCount != _Listings[index].playerCount)
                    {
                        _Listings[index].playerCount = info.PlayerCount;
                        _Listings[index].memberCountTxt.text = "[" + info.PlayerCount + "/" + info.MaxPlayers + "]";
                    }
                }
            }

            int btnIndex = _Listings.FindIndex(x => x.roomName == info.Name);
            if (btnIndex >= 0)
            {
                RoomListBtn tmpBtn = _Listings[btnIndex];
                if ((bool)info.CustomProperties.ContainsKey("IsGameStarted"))
                {
                    if ((bool)info.CustomProperties["IsGameStarted"] || info.MaxPlayers <= info.PlayerCount)
                    {
                        tmpBtn.GetComponent<Button>().interactable = false;
                    }
                    else
                    {
                        tmpBtn.GetComponent<Button>().interactable = true;
                    }
                }
                if ((bool)info.CustomProperties.ContainsKey("GameModeIndex"))
                {
                    if(int.Parse((string)info.CustomProperties["GameModeIndex"]) >=0 && int.Parse((string)info.CustomProperties["GameModeIndex"]) < GameModeInfos.Length)
                        tmpBtn.GameModeTxt.text = "[" + (GameMode)(int.Parse((string)info.CustomProperties["GameModeIndex"])) + "]";
                }
            }
        }
    }

    public void SetNickName()
    {
        string name = SetNickNameInputField.text;
        if (PhotonNetwork.IsConnected)
        {
            myNickName = name;
            PhotonNetwork.NickName = name;
        }
        else
        {
            myNickName = name;
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


    public void ToggleReady()
    {
        if (PhotonNetwork.InRoom)
        {
            if ((string)PhotonNetwork.LocalPlayer.CustomProperties["Ready"] == "0")
            {
                ExitGames.Client.Photon.Hashtable cp = PhotonNetwork.LocalPlayer.CustomProperties;
                cp["Ready"] = "1";
                PhotonNetwork.LocalPlayer.SetCustomProperties(cp);
            }
            else
            {
                ExitGames.Client.Photon.Hashtable cp = PhotonNetwork.LocalPlayer.CustomProperties;
                cp["Ready"] = "0";
                PhotonNetwork.LocalPlayer.SetCustomProperties(cp);
            }
        }
    }

    public void KickPlayer(string id)
    {
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            if (PhotonNetwork.PlayerList[i].UserId == id)
            {
                PhotonNetwork.PlayerList[i].SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "Ready", "0" }, { "IsKicked", true } });
            }
        }
    }

    public void SetGameMode(int index)
    {
        if (PhotonNetwork.IsConnected == false)
        {
            gameMode = (GameMode)index;

            SetGameModeRPC((int)gameMode);
        }
        else
        {
            if (PhotonNetwork.InRoom)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    gameMode = (GameMode)index;
                    ExitGames.Client.Photon.Hashtable cp = PhotonNetwork.CurrentRoom.CustomProperties;
                    cp["GameModeIndex"] = ((int)gameMode).ToString();
                    PhotonNetwork.CurrentRoom.SetCustomProperties(cp);

                    GetComponent<PhotonView>().RPC("SetGameModeRPC", RpcTarget.AllBuffered, (int)gameMode);
                }
            }
            else
            {
                gameMode = (GameMode)index;

                SetGameModeRPC((int)gameMode);
            }
        }
    }
    public void AddGameModeIndex(int addAmount)
    {
        if (PhotonNetwork.IsConnected == false)
        {
            gameMode = (GameMode)(addAmount+(int)gameMode);
            SetGameModeRPC((int)gameMode);
        }
        else
        {
            if (PhotonNetwork.InRoom)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    gameMode = (GameMode)(addAmount + (int)gameMode);
                    ExitGames.Client.Photon.Hashtable cp = PhotonNetwork.CurrentRoom.CustomProperties;
                    cp["GameModeIndex"] = ((int)gameMode).ToString();
                    PhotonNetwork.CurrentRoom.SetCustomProperties(cp);

                    GetComponent<PhotonView>().RPC("SetGameModeRPC", RpcTarget.AllBuffered, (int)gameMode);
                }
            }
            else
            {
                gameMode = (GameMode)(addAmount + (int)gameMode);
                SetGameModeRPC((int)gameMode);
            }
        }
    }
    [PunRPC]
    public void SetGameModeRPC(int gameModeIndex)
    {
        gameMode = (GameMode)Mathf.Clamp(gameModeIndex, 0, GameModeInfos.Length - 1);

        GameModeImage.sprite = GameModeInfos[(int)gameMode].sprite;
        GameModeText.text = gameMode.ToString();
        switch (gameMode)
        {
            case GameMode.BattleRoyale:
                break;
            case GameMode.PassTheBomb:
                break;
        }
    }

    [PunRPC]
    public void StartGame()
    {
        if (PhotonNetwork.IsConnected)
        {
            ExitGames.Client.Photon.Hashtable cp = PhotonNetwork.CurrentRoom.CustomProperties;
            cp["IsGameStarted"] = true;
            PhotonNetwork.CurrentRoom.SetCustomProperties(cp);
            PhotonNetwork.CurrentRoom.IsOpen = false;
            //PhotonNetwork.CurrentRoom.IsVisible = false;
            RoomPlayerCount.playerCount = PhotonNetwork.CountOfPlayers;

            SceneManager.LoadScene("GameScene_"+ gameMode.ToString());

            PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "Ready", "0" } });
        }
    }
    [PunRPC]
    public void StopGameRPC()
    {
        if (PhotonNetwork.IsConnected)
        {
            ExitGames.Client.Photon.Hashtable cp = PhotonNetwork.CurrentRoom.CustomProperties;
            cp["IsGameStarted"] = false;
            PhotonNetwork.CurrentRoom.SetCustomProperties(cp);
            PhotonNetwork.CurrentRoom.IsOpen = true;
            //PhotonNetwork.CurrentRoom.IsVisible = true;
        }

        //GameStarted = false;
        //GenerationIndex = 0;
        //currGenerationTime = 0;

        //if (myPlayer != null)
        //    myPlayer.gameObject.SetActive(false);

        //currGameModeManager.StopGame();
    }


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
