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

    public GameMode gameMode = 0; //0:배틀로얄 1:폭탄돌리기 2: 몬스터피하기 3: target 맞추기

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
        // Player가 추가되었을 때 초기화
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
                // RoomData의 SetGameModeRPC는 Data 변경 동기화
                GetComponent<PhotonView>().RPC("SetRandomGameModeRPC", RpcTarget.AllBuffered, val);
                // NetworkController의 SetGameModeRPC는 UI 동기화
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
    }
}
