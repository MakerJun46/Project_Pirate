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

    [SerializeField] List<Transform> winnerTRs;
    [SerializeField] List<Transform> loserTRs;
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
            GameModeInfoTxt.text = currRoomData.GetCurrGameModeInfo();
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

                for (int i = 1; i < PhotonNetwork.PlayerList.Length; i++)
                {
                    rank = RoomData.GetInstance().GetPlayerFinalRank(PhotonNetwork.PlayerList[i].ActorNumber);
                }

                if (rank <= 0)
                {
                    int winnerIndex = RoomData.GetInstance().winnerIndex % winnerTRs.Count;
                    PhotonNetwork.Instantiate("PlayerRankCharacter", winnerTRs[winnerIndex].position, winnerTRs[winnerIndex].rotation, 0, new object[] { rank });
                    RoomData.GetInstance().GetComponent<PhotonView>().RPC("AddRankIndex", RpcTarget.AllBuffered, new object[] { 1, 0 });
                }
                else
                {
                    int loserIndex = RoomData.GetInstance().loserIndex % loserTRs.Count;
                    PhotonNetwork.Instantiate("PlayerRankCharacter", loserTRs[loserIndex].position, loserTRs[loserIndex].rotation, 0, new object[] { rank });
                    RoomData.GetInstance().GetComponent<PhotonView>().RPC("AddRankIndex", RpcTarget.AllBuffered, new object[] { 0, 1 });
                }
            }
        }
    }

    public Transform GetRankTransform(bool isWinner,int index)
    {
        if (isWinner)
        {
            return winnerTRs[index];
        }
        else
        {
            return loserTRs[index];
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
