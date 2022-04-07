using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public enum GameMode
{
    BattleRoyale,
    PassTheBomb,
    Survivor,
    HitTheTarget
}

public class RoomData : MonoBehaviourPunCallbacks
{
    public bool setSceneRandom;

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

    public List<int> FinalScores = new List<int>(10000);
    public List<int> currGameScores = new List<int>(10000);

    public GameMode gameMode = 0; //0:��Ʋ�ξ� 1:��ź������ 2: �������ϱ� 3: target ���߱�

    public int PlayedGameCount { get; private set; }
    private int MaxPlayGameCount = 3;

    public Color[] playerColor;

    public bool PlayGameCountOvered()
    {
        return PlayedGameCount >= MaxPlayGameCount;
    }

    void Start()
    {
        PV = GetComponent<PhotonView>();
        PV.RPC("InitializePlayerScoreRPC", RpcTarget.AllBuffered, PhotonNetwork.LocalPlayer.ActorNumber);
        PlayedGameCount = 0;

        DontDestroyOnLoad(this.gameObject);
    }

    public string GetCurrSceneString()
    {
        return "GameScene_" + gameMode.ToString();
    }

    public string GetGameModeInfo(GameMode gameMode)
    {
        string info="";
        switch (gameMode)
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
        }
        return info;
    }
    public void AddPlayedGameCount()
    {
        PV.RPC("AddPlayedGameCountRPC", RpcTarget.AllBuffered);
    }
    public void SetFinalScore()
    {
        PV.RPC("SetFinalScoreRPC", RpcTarget.AllBuffered);
    }
    public void SetCurrScore(int _actorID, float _addScore)
    {
        PV.RPC("SetCurrScoreRPC", RpcTarget.AllBuffered, _actorID, currGameScores[_actorID]+ (int)_addScore);
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
    public void AddPlayedGameCountRPC()
    {
        PlayedGameCount++;
    }

    [PunRPC]
    public void SetGameModeRPC(int _gameModeIndex)
    {
        gameMode = (GameMode)_gameModeIndex;
    }

    [PunRPC]
    public void SetFinalScoreRPC()
    {
        for(int i = 0; i < FinalScores.Count; i++)
        {
            FinalScores[i] += currGameScores[i];
            currGameScores[i] = 0;
        }
        GameManager.GetInstance().RefreshPlayeScore(true);
    }
    [PunRPC]
    public void SetCurrScoreRPC(int _actorID, int _score)
    {
        GameManager.GetInstance().ActiveScoreEffect(_actorID, _score- currGameScores[_actorID]);
        currGameScores[_actorID] = _score;
        GameManager.GetInstance().RefreshPlayeScore(false);
    }
    [PunRPC]
    public void SetRandomGameModeRPC(bool _val)
    {
        setSceneRandom = _val;
    }

    public void ToggleRandomGameMode(bool val)
    {
        if (PhotonNetwork.InRoom)
        {
            if (PhotonNetwork.IsMasterClient)
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
    }

    public void AddGameModeIndex(int addAmount)
    {
        int resultIndex = (addAmount + (int)gameMode)%4;
        if (resultIndex < 0)
        {
            resultIndex += 4;
        }
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
            if (PhotonNetwork.InRoom)
            {
                if (PhotonNetwork.IsMasterClient)
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
    }
}
