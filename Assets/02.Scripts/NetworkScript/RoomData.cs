using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

public enum GameMode
{
    BattleRoyale,
    PassTheBomb,
    Survivor,
    HitTheTarget,
    Treasure,
    GhostShip
}

public class RoomData : MonoBehaviourPunCallbacks
{

    private static RoomData instance;
    public static RoomData GetInstance()
    {
        if (instance == null)
        {
            instance = FindObjectOfType<RoomData>();
        }
        return instance;
    }

    PhotonView PV;

    [Header("[GameMode Info]")]
    public int gameMode = 0;
    public bool setSceneRandom;
    public int PlayedGameCount { get; private set; }
    private List<int> remainGameModeList = new List<int>();

    [SerializeField] int MaxPlayGameCount = 3;

    public List<Color> playerColor;

    // Scores
    public List<int> FinalScores = new List<int>(10000);
    public List<int> currGameScores = new List<int>(10000);

    void Start()
    {
        PV = GetComponent<PhotonView>();
        PV.RPC("InitializePlayerScoreRPC", RpcTarget.AllBuffered, PhotonNetwork.LocalPlayer.ActorNumber);
        PlayedGameCount = 0;

        remainGameModeList.Clear();
        for (int i = 0; i < System.Enum.GetValues(typeof(GameMode)).Length; i++)
            remainGameModeList.Add(i);

        DontDestroyOnLoad(this.gameObject);
    }

    public bool PlayGameCountOvered()
    {
        return PlayedGameCount >= MaxPlayGameCount;
    }

    public string GetCurrSceneString()
    {
        return ((GameMode)gameMode).ToString();
    }

    #region GameMode

    public int GetNotOverlappedRandomGameMode()
    {
        int currGameIndex = Random.Range(0, remainGameModeList.Count);
        int returnVal = remainGameModeList[currGameIndex];
        remainGameModeList.RemoveAt(currGameIndex);

        return returnVal;
    }

    public string GetGameModeInfo()
    {
        string info = "";
        switch ((GameMode)gameMode)
        {
            case GameMode.BattleRoyale:
                info = "배틀로얄은 최후의 1인이 승리하는 게임입니다.\n 자원을 수집하고 장비를 제작하여 경쟁자와 싸우세요.";
                break;
            case GameMode.PassTheBomb:
                info = "폭탄돌리기는 일정 시간 뒤에 폭탄을 가진 술래가 탈락하는 게임입니다.\n 탈락하기 싫다면 폭탄에서 최대한 멀어지세요!.";
                break;
            case GameMode.Survivor:
                info = "서바이벌은 여러 종류의 해양 몬스터의 공격을 피해 살아남는 게임입니다.";
                break;
            case GameMode.HitTheTarget:
                info = "타겟 맞추기는 가장 많은 과녁을 맞추는 플레이어가 승리하는 게임입니다.";
                break;
            case GameMode.Treasure:
                info = "보물찾기는 가장 많은 보물을 얻은 플레이어가 승리하는 게임입니다.";
                break;
            case GameMode.GhostShip:
                info = "유령선은 유령선을 피해 끝까지 도망친 플레이어가 승리하는 게임입니다.";
                break;
        }
        return info;
    }
    public string GetGameModeTitle()
    {
        string info = "";
        switch ((GameMode)gameMode)
        {
            case GameMode.BattleRoyale:
                info = "배틀로얄";
                break;
            case GameMode.PassTheBomb:
                info = "폭탄돌리기";
                break;
            case GameMode.Survivor:
                info = "서바이벌";
                break;
            case GameMode.HitTheTarget:
                info = "타겟맞추기";
                break;
            case GameMode.Treasure:
                info = "보물찾기";
                break;
            case GameMode.GhostShip:
                info = "유령선피하기";
                break;
        }
        return info;
    }
    public void AddPlayedGameCount()
    {
        PV.RPC("AddPlayedGameCountRPC", RpcTarget.AllBuffered);
    }
    [PunRPC]
    public void AddPlayedGameCountRPC()
    {
        PlayedGameCount++;
    }

    public void AddGameModeIndex(int addAmount)
    {
        // Random Mode도 있기에 +1
        int selectableGameModeCount = (System.Enum.GetValues(typeof(GameMode)).Length + 1);
        int resultIndex = (addAmount + (int)gameMode) % selectableGameModeCount;
        if (resultIndex < 0)
            resultIndex += selectableGameModeCount;

        if (PhotonNetwork.IsConnected == false)
        {
            SetGameModeRPC(resultIndex);

            if (FindObjectOfType<LobbyManager>())
            {
                FindObjectOfType<LobbyManager>().SetGameModeRPC();
            }
        }
        else
        {
            if (PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient)
            {
                // RoomData의 SetGameModeRPC는 Data 변경 동기화
                GetComponent<PhotonView>().RPC("SetGameModeRPC", RpcTarget.AllBuffered, resultIndex);
                // NetworkController의 SetGameModeRPC는 UI 동기화
                if (FindObjectOfType<LobbyManager>())
                {
                    FindObjectOfType<LobbyManager>().GetComponent<PhotonView>().RPC("SetGameModeRPC", RpcTarget.AllBuffered);
                }
            }
        }
    }

    public void ToggleRandomGameMode(bool val)
    {
        if (PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient)
        {
            // RoomData의 SetGameModeRPC는 Data 변경 동기화
            GetComponent<PhotonView>().RPC("SetRandomGameModeRPC", RpcTarget.AllBuffered, val);
            // NetworkController의 SetGameModeRPC는 UI 동기화
            if (FindObjectOfType<LobbyManager>())
            {
                FindObjectOfType<LobbyManager>().GetComponent<PhotonView>().RPC("SetGameModeRPC", RpcTarget.AllBuffered);
            }
        }
    }

    [PunRPC]
    public void SetGameModeRPC(int _gameModeIndex)
    {
        gameMode = _gameModeIndex;
    }

    [PunRPC]
    public void SetRandomGameModeRPC(bool _val)
    {
        setSceneRandom = _val;
    }
    #endregion

    #region Scores
    public void SetFinalScore()
    {
        PV.RPC("SetFinalScoreRPC", RpcTarget.AllBuffered);
    }
    public void SetCurrScore(int _actorID, float _addScore)
    {
        PV.RPC("SetCurrScoreRPC", RpcTarget.AllBuffered, _actorID, currGameScores[_actorID] + (int)_addScore);
    }

    [PunRPC]
    public void InitializePlayerScoreRPC(int _actorID)
    {
        // Player가 추가되었을 때 초기화
        FinalScores[_actorID] = 0;
        currGameScores[_actorID] = 0;
    }

    public void SetCurrGameScoreToZero(int _actorID)
    {
        currGameScores[_actorID] = 0;
    }


    [PunRPC]
    public void SetFinalScoreRPC()
    {
        int addedScore = 0;
        for (int i = 0; i < FinalScores.Count; i++)
        {
            addedScore+= currGameScores[i];
            FinalScores[i] += currGameScores[i];
            GameManager.GetInstance().ActiveScoreEffect(i, addedScore);
            currGameScores[i] = 0;
        }
        GameManager.GetInstance().RefreshPlayeScore(true);
    }
    [PunRPC]
    public void SetCurrScoreRPC(int _actorID, int _score)
    {
        GameManager.GetInstance().ActiveScoreEffect(_actorID, _score - currGameScores[_actorID]);
        currGameScores[_actorID] = _score;
        GameManager.GetInstance().RefreshPlayeScore(false);
    }
    #endregion

    #region Rank
    public int GetPlayerFinalRank(int _actorID)
    {
        List<int> sortedScore = new List<int>();
        for (int i = 0; i < FinalScores.Count; i++)
        {
            sortedScore.Add(FinalScores[i]);
        }
        sortedScore.Sort(Compare);

        int rank = 1000;

        for (int i = 0; i < sortedScore.Count; i++)
        {
            if (sortedScore[i] == FinalScores[_actorID])
            {
                rank = i;
                break;
            }
        }
        return rank;
    }
    public int GetPlayerCurrentRank(int _actorID)
    {
        List<int> sortedScore = new List<int>();
        for (int i = 0; i < currGameScores.Count; i++)
        {
            sortedScore.Add(currGameScores[i]);
        }
        sortedScore.Sort(Compare);

        int rank = 1000;

        for (int i = 0; i < sortedScore.Count; i++)
        {
            if (sortedScore[i] == currGameScores[_actorID])
            {
                rank = i;
                break;
            }
        }
        return rank;
    }
    private int Compare(int a, int b)
    {
        if (a > b)
            return -1;
        else
            return 1;
    }
    #endregion

}
