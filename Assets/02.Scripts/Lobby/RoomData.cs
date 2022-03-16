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

    public GameMode gameMode = 0; //0:¹èÆ²·Î¾â 1:ÆøÅºµ¹¸®±â 2: ¸ó½ºÅÍÇÇÇÏ±â

    public int PlayedGameCount = 0;

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

    [PunRPC]
    public void StartLoading(bool _start)
    {
        FindObjectOfType<NetworkManager>().LoadingFunc(_start);
    }
}
