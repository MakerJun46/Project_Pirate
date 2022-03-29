using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using UnityEngine.UI;
public class RoomGameManager : GameManager
{
    [SerializeField] Text GameModeTitleTxt;
    [SerializeField] Text GameModeInfoTxt;

    protected override void Start()
    {
        base.Start();

        bool ForceQuit = false;
        if (PhotonNetwork.CurrentRoom != null && PhotonNetwork.CurrentRoom.PlayerCount <= 1)
            ForceQuit = true;

        RoomData currRoomData = RoomData.GetInstance();
        if (currRoomData.PlayGameCountOvered() || ForceQuit)
        {
            List<int> sortedScore = new List<int>();
            foreach(int key in currRoomData.Scores.Keys)
            {
                sortedScore.Add(currRoomData.Scores[key]);
            }
            sortedScore.Sort(Compare);

            int rank=1000;
            for(int i = 0; i < sortedScore.Count; i++)
            {
                if(sortedScore[i] == currRoomData.Scores[PhotonNetwork.LocalPlayer.ActorNumber])
                {
                    rank = i;
                    break;
                }
            }

            WinPanel.SetActive(rank <= 0);
            LosePanel.SetActive(rank > 0);
        }
    }
    public override void StartGame()
    {
        base.StartGame();

        RoomData currRoomData = RoomData.GetInstance();
        Scene tmpScene = SceneManager.GetSceneByName(currRoomData.GetCurrSceneString());
        if (tmpScene != null)
        {
            SceneManager.LoadScene(currRoomData.GetCurrSceneString());
        }
        else
            SceneManager.LoadScene(0);
    }

    public override void EndGame()
    {
    }

    protected override void Update()
    {
        RoomData currRoomData = RoomData.GetInstance();
        if (currRoomData)
        {
            GameModeTitleTxt.text = "GameMode : " + currRoomData.gameMode.ToString();
            GameModeInfoTxt.text = currRoomData.GetGameModeInfo(RoomData.GetInstance().gameMode);
        }
    }

    private int Compare(int a, int b)
    {
        if (a > b)
            return -1;
        else
            return 1;
    }
}
