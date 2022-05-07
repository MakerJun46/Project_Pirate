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
                info = "��Ʋ�ξ��� ������ 1���� �¸��ϴ� �����Դϴ�.\n �ڿ��� �����ϰ� ��� �����Ͽ� �����ڿ� �ο켼��.";
                break;
            case GameMode.PassTheBomb:
                info = "��ź������� ���� �ð� �ڿ� ��ź�� ���� ������ Ż���ϴ� �����Դϴ�.\n Ż���ϱ� �ȴٸ� ��ź���� �ִ��� �־�������!.";
                break;
            case GameMode.Survivor:
                info = "�����̹��� ���� ������ �ؾ� ������ ������ ���� ��Ƴ��� �����Դϴ�.";
                break;
            case GameMode.HitTheTarget:
                info = "Ÿ�� ���߱�� ���� ���� ������ ���ߴ� �÷��̾ �¸��ϴ� �����Դϴ�.";
                break;
            case GameMode.Treasure:
                info = "����ã��� ���� ���� ������ ���� �÷��̾ �¸��ϴ� �����Դϴ�.";
                break;
            case GameMode.GhostShip:
                info = "���ɼ��� ���ɼ��� ���� ������ ����ģ �÷��̾ �¸��ϴ� �����Դϴ�.";
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
                info = "��Ʋ�ξ�";
                break;
            case GameMode.PassTheBomb:
                info = "��ź������";
                break;
            case GameMode.Survivor:
                info = "�����̹�";
                break;
            case GameMode.HitTheTarget:
                info = "Ÿ�ٸ��߱�";
                break;
            case GameMode.Treasure:
                info = "����ã��";
                break;
            case GameMode.GhostShip:
                info = "���ɼ����ϱ�";
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
        // Random Mode�� �ֱ⿡ +1
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
                // RoomData�� SetGameModeRPC�� Data ���� ����ȭ
                GetComponent<PhotonView>().RPC("SetGameModeRPC", RpcTarget.AllBuffered, resultIndex);
                // NetworkController�� SetGameModeRPC�� UI ����ȭ
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
            // RoomData�� SetGameModeRPC�� Data ���� ����ȭ
            GetComponent<PhotonView>().RPC("SetRandomGameModeRPC", RpcTarget.AllBuffered, val);
            // NetworkController�� SetGameModeRPC�� UI ����ȭ
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
        // Player�� �߰��Ǿ��� �� �ʱ�ȭ
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
