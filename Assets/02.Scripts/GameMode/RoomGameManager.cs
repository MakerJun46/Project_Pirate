using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Cinemachine;
using UnityEngine.UI;
public class RoomGameManager : GameManager
{
    [SerializeField] GameObject GameInfoPanel;
    [SerializeField] Text GameModeTitleTxt;
    [SerializeField] Text GameModeInfoTxt;

    [SerializeField] Transform PlayerListPanel;
    [SerializeField] Transform PlayerListContainer;

    [SerializeField] List<Transform> rankTRs;
    [SerializeField] GameObject rankObjs;

    int rank = 0;

    protected override void Start()
    {
        base.Start();

        Invoke("ActiveResultPanel", 1f);
        
        rank = RoomData.GetInstance().GetPlayerFinalRank(PhotonNetwork.LocalPlayer.ActorNumber);

    }
    protected override void Update()
    {
        base.Update();
        
        RoomData currRoomData = RoomData.GetInstance();
        if (currRoomData)
        {
            GameModeTitleTxt.text = currRoomData.GetGameModeTitle();
            GameModeInfoTxt.text = currRoomData.GetGameModeInfo();
        }
    }
    public override void StartGame()
    {
        base.StartGame();

        RoomData currRoomData = RoomData.GetInstance();
        Scene tmpScene = SceneManager.GetSceneByName("GameScene_" + currRoomData.GetCurrSceneString());
        if (tmpScene != null)
            SceneManager.LoadScene("GameScene_" + currRoomData.GetCurrSceneString());
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
            PlayerListPanel.gameObject.SetActive(true);
            ControllerUI.gameObject.SetActive(false);
            GameInfoPanel.SetActive(false);
            rankObjs.SetActive(true);
            WinPanel.SetActive(false);
            LosePanel.SetActive(false);
            ObserverModePanel.SetActive(false);

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
                    Transform tmpPlayerList = Instantiate(PlayerListContainer.transform.GetChild(0), PlayerListContainer);
                    tmpPlayerList.gameObject.SetActive(true);
                    tmpPlayerList.GetChild(0).GetComponent<Text>().text = (i + 1) + "th  " + PhotonNetwork.PlayerList[tmpFounded].NickName;
                    tmpPlayerList.GetChild(1).GetComponent<Text>().text = "점수 : " + currRoomData.FinalScores[PhotonNetwork.PlayerList[tmpFounded].ActorNumber];
                }
            }

            FindObjectOfType<NetworkManager>().StartFadeInOut(true);
            if (PhotonNetwork.IsMasterClient==false)
            {
                // 자신의 등수 알아내기

                WinPanel.SetActive(rank <= 0);
                LosePanel.SetActive(rank > 0);

                int playerIndex = 0;
                for (int i = 1; i < PhotonNetwork.PlayerList.Length; i++)
                {
                    if (PhotonNetwork.PlayerList[i] == PhotonNetwork.LocalPlayer)
                    {
                        playerIndex = i;
                        break;
                    }
                }
                PhotonNetwork.Instantiate("PlayerRankCharacter", rankTRs[playerIndex-1].position, rankTRs[playerIndex-1].rotation, 0, new object[] { rank});
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
