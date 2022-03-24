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

        if (RoomData.GetInstance().PlayedGameCount >= 3 || ForceQuit)
        {
            List<int> sortedScore = new List<int>();
            foreach(int key in RoomData.GetInstance().Scores.Keys)
            {
                sortedScore.Add(RoomData.GetInstance().Scores[key]);
            }
            sortedScore.Sort(Compare);

            int rank=1000;
            for(int i = 0; i < sortedScore.Count; i++)
            {
                if(sortedScore[i] == RoomData.GetInstance().Scores[PhotonNetwork.LocalPlayer.ActorNumber])
                {
                    rank = i;
                }
            }

            WinPanel.SetActive(rank <= 0);
            LosePanel.SetActive(rank > 0);
        }
    }
    public override void StartGame()
    {
        base.StartGame();

        Scene tmpScene = SceneManager.GetSceneByName(RoomData.GetInstance().GetCurrSceneString());
        if (tmpScene != null)
        {
            SceneManager.LoadScene(RoomData.GetInstance().GetCurrSceneString());
            if(PhotonNetwork.IsConnected==false || PhotonNetwork.IsMasterClient)
                RoomData.GetInstance().GetComponent<PhotonView>().RPC("AddPlayedGameCount", RpcTarget.AllBuffered);
        }
        else
            SceneManager.LoadScene(0);
    }

    protected override void Update()
    {
        if (RoomData.GetInstance())
        {
            GameModeTitleTxt.text = "GameMode : " + RoomData.GetInstance().gameMode.ToString();
            GameModeInfoTxt.text = RoomData.GetInstance().GetGameModeInfo(RoomData.GetInstance().gameMode);
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
