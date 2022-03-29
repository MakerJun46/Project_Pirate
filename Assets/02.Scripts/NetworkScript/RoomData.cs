using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public enum GameMode
{
    BattleRoyale,
    PassTheBomb,
    Survivor
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

    public Dictionary<int, int> Scores{ get; private set; }

    public GameMode gameMode = 0; //0:배틀로얄 1:폭탄돌리기 2: 몬스터피하기

    public int PlayedGameCount { get; private set; }
    private int MaxPlayGameCount = 3;

    public bool PlayGameCountOvered()
    {
        return PlayedGameCount >= MaxPlayGameCount;
    }

    void Start()
    {
        DontDestroyOnLoad(this.gameObject);
        GetComponent<PhotonView>().RPC("AddPlayerRPC", RpcTarget.AllBuffered, PhotonNetwork.LocalPlayer.ActorNumber);
        PlayedGameCount = 0;
        if (Scores == null)
            Scores = new Dictionary<int, int>();
    }

    public string GetCurrSceneString()
    {
        string nextSceneString= "GameScene_";
        nextSceneString+=gameMode.ToString();

        return nextSceneString;
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
        }
        return info;
    }

    [PunRPC]
    public void AddPlayerRPC(int _actorID)
    {
        if (Scores == null)
            Scores = new Dictionary<int, int>();
        if (Scores.ContainsKey(_actorID))
        {
            Scores[_actorID] = 0;
        }
        else
        {
            Scores.Add(_actorID, 0);
        }
    }


    [PunRPC]
    public void AddPlayedGameCount()
    {
        PlayedGameCount++;
    }

    [PunRPC]
    public void SetGameModeRPC(int _gameModeIndex)
    {
        gameMode = (GameMode)_gameModeIndex;
    }

    [PunRPC]
    public void SetScoreRPC(int _actorID, int _score)
    {
        if (Scores == null)
            Scores = new Dictionary<int, int>();
        Scores[_actorID] = _score;
    }

    [PunRPC]
    public void DestroyRoomData()
    {
        Destroy(this.gameObject);
    }

}
