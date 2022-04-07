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
        
        RefreshPlayeScore(true);

        Invoke("ActiveResultPanel", 1f);
    }
    protected override void Update()
    {
        base.Update();

        RoomData currRoomData = RoomData.GetInstance();
        if (currRoomData)
        {
            GameModeTitleTxt.text = "GameMode : " + currRoomData.gameMode.ToString();
            GameModeInfoTxt.text = currRoomData.GetGameModeInfo(RoomData.GetInstance().gameMode);
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

    public void ActiveResultPanel()
    {
        RoomData currRoomData = RoomData.GetInstance();

        bool ForceQuit = false;
        if (PhotonNetwork.CurrentRoom != null && PhotonNetwork.CurrentRoom.PlayerCount <= 1)
            ForceQuit = true;

        if (currRoomData.PlayGameCountOvered() || ForceQuit)
        {
            List<int> sortedScore = new List<int>();
            for (int i = 0; i < currRoomData.FinalScores.Count; i++)
            {
                sortedScore.Add(currRoomData.FinalScores[i]);
            }
            sortedScore.Sort(Compare);

            int rank = 1000;

            for (int i = 0; i < sortedScore.Count; i++)
            {
                if (sortedScore[i] == currRoomData.FinalScores[PhotonNetwork.LocalPlayer.ActorNumber])
                {
                    rank = i;
                    break;
                }
            }

            List<int> bestPlayerNumbers = new List<int>();
            for (int i = 0; i < sortedScore.Count; i++)
            {
                for (int j = 0; j < currRoomData.FinalScores.Count; j++)
                {
                    if (sortedScore[i] == currRoomData.FinalScores[j] && bestPlayerNumbers.Contains(j) == false)
                    {
                        bestPlayerNumbers.Add(j);
                        break;
                    }
                }
            }

            ObserverModePanel.SetActive(PhotonNetwork.IsMasterClient);
            ObserverModePanel.GetComponentInChildren<Text>().text = "Game END\n";
            for (int i = 0; i < bestPlayerNumbers.Count; i++)
            {
                int tmpFounded = -1;
                for (int j = 0; j < PhotonNetwork.PlayerList.Length; j++)
                {
                    if (PhotonNetwork.PlayerList[j].IsMasterClient)
                        continue;
                    if (bestPlayerNumbers[i] == PhotonNetwork.PlayerList[j].ActorNumber)
                    {
                        tmpFounded = j;
                        break;
                    }
                }
                if (tmpFounded >= 0)
                {
                    ObserverModePanel.GetComponentInChildren<Text>().text += (i + 1) + "th : " + PhotonNetwork.PlayerList[tmpFounded].NickName + "\n";
                }
            }

            WinPanel.SetActive(PhotonNetwork.IsMasterClient == false && rank <= 0);
            LosePanel.SetActive(PhotonNetwork.IsMasterClient == false && rank > 0);
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
