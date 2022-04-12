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
            GameModeTitleTxt.text = "GameMode : " + currRoomData.GetCurrSceneString();
            GameModeInfoTxt.text = currRoomData.GetGameModeInfo();
        }
    }
    public override void StartGame()
    {
        base.StartGame();

        RoomData currRoomData = RoomData.GetInstance();
        Scene tmpScene = SceneManager.GetSceneByName("GameScene_" + currRoomData.GetCurrSceneString());
        if (tmpScene != null)
        {
            SceneManager.LoadScene("GameScene_" + currRoomData.GetCurrSceneString());
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
            WinPanel.SetActive(false);
            LosePanel.SetActive(false);
            ObserverModePanel.SetActive(false);

            if (PhotonNetwork.IsMasterClient)
            {
                // 자신의 등수가 아닌 전체 등수를 알려면 따로 계산이 필요함
                ObserverModePanel.SetActive(true);

                List<int> sortedScore = new List<int>();
                for (int i = 0; i < currRoomData.FinalScores.Count; i++)
                {
                    sortedScore.Add(currRoomData.FinalScores[i]);
                }
                sortedScore.Sort(Compare);

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
            }
            else
            {
                // 자신의 등수 알아내기
                int rank = currRoomData.GetPlayerFinalRank(PhotonNetwork.LocalPlayer.ActorNumber);

                WinPanel.SetActive(rank <= 0);
                LosePanel.SetActive(rank > 0);
            }
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
