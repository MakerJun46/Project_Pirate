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
    [SerializeField] Image GameModeTitleImage;
    [SerializeField] Text GameModeTitleTxt;
    [SerializeField] Text GameModeInfoTxt;

    [SerializeField] Transform topPlayerListPanel;
    [SerializeField] Transform PlayerListPanel;
    [SerializeField] Transform PlayerListContainer;

    [SerializeField] List<Transform> winnerTRs;
    [SerializeField] List<Transform> loserTRs;
    [SerializeField] GameObject rankObjs;

    [SerializeField] GameObject PreviewObject_BattleRoyal;
    [SerializeField] GameObject PreviewObject_GhostShip;
    [SerializeField] GameObject PreviewObject_PassTheBomb;
    [SerializeField] GameObject PreviewObject_Treasure;
    [SerializeField] GameObject PreviewObject_HitTheTarget;
    [SerializeField] GameObject PreviewObject_Survivor;

    int rank = 0;

    protected override void Start()
    {
        base.Start();

        Invoke("ActiveResultPanel", 1f);
        
        if(RoomData.GetInstance())
            rank = RoomData.GetInstance().GetPlayerFinalRank(PhotonNetwork.LocalPlayer.ActorNumber);

    }
    protected override void Update()
    {
        base.Update();

        RoomData currRoomData = RoomData.GetInstance();
        if (currRoomData)
        {
            GameModeTitleTxt.text = currRoomData.GetGameModeTitle();
            GameModeTitleImage.sprite = currRoomData.GetCurrGameModeSprite();
            GameModeInfoTxt.text = currRoomData.GetCurrGameModeInfo();

            if ((GameManager.isObserver && !PhotonNetwork.IsMasterClient) || !GameManager.isObserver)
            {
                GameMode gm = (GameMode)currRoomData.gameMode;

                PreviewObject_BattleRoyal.SetActive(false);
                PreviewObject_GhostShip.SetActive(false);
                PreviewObject_PassTheBomb.SetActive(false);
                PreviewObject_Treasure.SetActive(false);
                PreviewObject_HitTheTarget.SetActive(false);
                PreviewObject_Survivor.SetActive(false);

                switch (gm)
                {
                    case GameMode.BattleRoyale:
                        PreviewObject_BattleRoyal.SetActive(true);
                        break;
                    case GameMode.GhostShip:
                        PreviewObject_GhostShip.SetActive(true);
                        break;
                    case GameMode.PassTheBomb:
                        PreviewObject_PassTheBomb.SetActive(true);
                        break;
                    case GameMode.Treasure:
                        PreviewObject_Treasure.SetActive(true);
                        break;
                    case GameMode.HitTheTarget:
                        PreviewObject_HitTheTarget.SetActive(true);
                        break;
                    case GameMode.Survivor:
                        PreviewObject_Survivor.SetActive(true);
                        break;
                }
            }
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
            topPlayerListPanel.gameObject.SetActive(false);
            PlayerListPanel.gameObject.SetActive(true);
            ControllerUI.gameObject.SetActive(false);
            GameInfoPanel.SetActive(false);
            rankObjs.SetActive(true);
            WinPanel.SetActive(false);
            LosePanel.SetActive(false);
            ObserverModePanel.SetActive(false);

            // ?????? ?????? ???? ???? ?????? ?????? ???? ?????? ??????
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

            for (int i = 0; i < bestPlayerNumbers.Count; i++)
            {
                int tmpFounded = -1;

                for (int j = 0; j < PhotonNetwork.PlayerList.Length; j++)
                {
                    if (GameManager.isObserver && PhotonNetwork.PlayerList[j].IsMasterClient)
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
                    tmpPlayerList.GetChild(1).GetComponent<Text>().text = "???? : " + currRoomData.FinalScores[PhotonNetwork.PlayerList[tmpFounded].ActorNumber];
                }
            }

            FindObjectOfType<NetworkManager>().StartFadeInOut(true);
            if ((GameManager.isObserver && PhotonNetwork.IsMasterClient==false) || !GameManager.isObserver)
            {
                // ?????? ???? ????????

                WinPanel.SetActive(rank <= 0);
                LosePanel.SetActive(rank > 0);
                WinPanel.transform.GetChild(1).gameObject.SetActive(false);
                LosePanel.transform.GetChild(1).gameObject.SetActive(false);
                StartCoroutine("RankCoroutine", rank + 2);
            }
        }
    }

    IEnumerator RankCoroutine(float _time)
    {
        yield return new WaitForSeconds(_time+Random.Range(0f,0.5f));

        if (rank <= 0)
        {
            int winnerIndex = RoomData.GetInstance().winnerIndex % winnerTRs.Count;
            PhotonNetwork.Instantiate("PlayerRankCharacter", winnerTRs[winnerIndex].position, winnerTRs[winnerIndex].rotation, 0, new object[] { rank});
            RoomData.GetInstance().GetComponent<PhotonView>().RPC("AddRankIndex", RpcTarget.AllBuffered, new object[] { 1, 0 });
        }
        else
        {
            int loserIndex = RoomData.GetInstance().loserIndex % loserTRs.Count;
            PhotonNetwork.Instantiate("PlayerRankCharacter", loserTRs[loserIndex].position, loserTRs[loserIndex].rotation, 0, new object[] { rank});
            RoomData.GetInstance().GetComponent<PhotonView>().RPC("AddRankIndex", RpcTarget.AllBuffered, new object[] { 0, 1 });
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
