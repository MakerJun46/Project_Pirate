using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;
using System.Linq;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour, IPunObservable
{
    protected static GameManager instance;
    public static GameManager GetInstance()
    {
        if (instance == null)
        {
            instance = FindObjectOfType<GameManager>();
        }
        return instance;
    }

    #region Variables & Initializer
    public bool GameStarted;

    public int PlayerCount;
    private int ObserveIndex;

    protected float currPlayTime;
    public float maxPlayTime = 60f;

    [SerializeField] protected Text TimeText;

    private float steeringRot;
    [SerializeField] private Image SteeringImg;

    public GameObject BestPlayerContent;
    private List<GameObject> BestPlayerLists = new List<GameObject>();
    public int BestPlayerCount => BestPlayerLists.Count;
    List<PlayerScoreList> bestPlayerListBox = new List<PlayerScoreList>();


    public List<Player_Controller_Ship> AllShip;
    public Player_Controller_Ship MyShip;
    public CinemachineVirtualCamera VC_Top;
    public CinemachineVirtualCamera VC_TPS;
    protected bool topView = true;

    [SerializeField] private Vector3 camOffset = new Vector3(0, 372, -290);

    protected bool IsWinner;
    [SerializeField] protected GameObject WinPanel;
    [SerializeField] protected GameObject LosePanel;
    [SerializeField] protected GameObject ObserverModePanel;

    [SerializeField] protected GameObject UI_Observer;
    [SerializeField] protected GameObject ObserverCameras_Parent;

    protected virtual void Start()
    {
        instance = this;
        IsWinner = false;
        currPlayTime = 0;

        bestPlayerListBox.Clear();
        for (int i=0;i< BestPlayerContent.transform.childCount; i++)
        {
            bestPlayerListBox.Add(BestPlayerContent.transform.GetChild(i).GetComponent<PlayerScoreList>());
        }

        RefreshPlayeScore(false);
    }

    public virtual void SetObserverCamera()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            UI_Observer.SetActive(true);
            ObserverCameras_Parent.SetActive(true);

            for (int i = 0; i < AllShip.Count; i++)
            {
                if (AllShip[i] != null)
                {
                    ObserverCameras_Parent.transform.GetChild(i).GetComponent<CinemachineVirtualCamera>().LookAt = AllShip[i].gameObject.transform;
                    ObserverCameras_Parent.transform.GetChild(i).GetComponent<CinemachineVirtualCamera>().Follow = AllShip[i].gameObject.transform;
                }
            }
        }
    }
    public virtual void SetMyShip(Player_Controller_Ship _myShip, bool _setMyShip = true)
    {
        if (_setMyShip)
            MyShip = _myShip;
        VC_Top.m_Follow = _myShip.transform;
        VC_Top.m_LookAt = _myShip.transform;
        VC_TPS.m_Follow = _myShip.transform;
        VC_TPS.m_LookAt = _myShip.transform;
    }
    #endregion

    #region GameFlow
    public virtual void StartGame()
    {
        GameStarted = true;
        IsWinner = false;
    }
    public virtual void EndGame()
    {
        if (PhotonNetwork.IsConnected == false || PhotonNetwork.IsMasterClient)
        {
            RoomData.GetInstance().SetFinalScore();
            RoomData.GetInstance().AddPlayedGameCount();
        }

        FindObjectOfType<NetworkManager>().GoToLobby();
    }

    public virtual void MasterChanged(bool _isMaster)
    {
    }

    /// <summary>
    /// �� �������� �� �� �� ����Ǿ����
    /// </summary>
    /// <param name="_win"></param>
    public virtual void JudgeWinLose()
    {
        WinPanel.SetActive(IsWinner);
        LosePanel.SetActive(!IsWinner);
        print("End : " + IsWinner);
        GameStarted = false;


        if (MyShip)
        {
            MyShip.ActiveWinLoseEffect(IsWinner);
            Change_VC_Lookat(MyShip.GetComponent<PhotonView>().ViewID);
        }
        //else
        //{
        //    if (BestPlayerLists.Count > 0)
        //    {
        //        BestPlayerLists[0].GetComponent<Player_Controller_Ship>().ActiveWinLoseEffect(true);
        //        Change_VC_Lookat(BestPlayerLists[0].GetComponent<PhotonView>().ViewID);
        //    }
        //}
    }
    public Cinemachine.CinemachineVirtualCamera VC_Winner;
    public void Change_VC_Lookat(int ViewID)
    {
        VC_Winner.Priority = 15;
        if (PhotonView.Find(ViewID) && PhotonView.Find(ViewID).gameObject)
        {
            VC_Winner.LookAt = PhotonView.Find(ViewID).gameObject.transform;
            VC_Winner.Follow = PhotonView.Find(ViewID).gameObject.transform;
        }
    }

    protected virtual void Update()
    {
        if (TimeText)
            TimeText.text = ((int)(currPlayTime / 60)) + ":" + ((int)(currPlayTime % 60));
        if (PhotonNetwork.IsMasterClient)
        {
            currPlayTime += Time.deltaTime;
        }else
        {
            // ���� ����
            if (MyShip)
            {
                if (MyShip.is_Turn_Left)
                    steeringRot += 180 * Time.deltaTime;
                else if (MyShip.is_Turn_Right)
                    steeringRot += -180 * Time.deltaTime;
                else
                    steeringRot = Mathf.Lerp(steeringRot, 0, Time.deltaTime);

                steeringRot = Mathf.Clamp(steeringRot, -720, 720);
                SteeringImg.transform.rotation = Quaternion.Euler(0, 0, steeringRot);
            }
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            ToggleGameView();
        }
    }
    #endregion

    #region UI Control Button
    public void Turn_Left_Button_Down()
    {
        MyShip.is_Turn_Left = true;
    }

    public void Turn_Left_Button_Up()
    {
        MyShip.is_Turn_Left = false;
    }

    public void Turn_Right_Button_Down()
    {
        MyShip.is_Turn_Right = true;
    }
    public void Turn_Right_Button_Up()
    {
        MyShip.is_Turn_Right = false;
    }
    public void GoOrStop_Button()
    {
        MyShip.goOrStop = !MyShip.goOrStop;
    }

    public void TryUpgradeShip()
    {
        if (MyShip)
            MyShip.UpgradeShip();
    }
    #endregion

    #region Best Player
    public void ActiveScoreEffect(int _actorID, int _score)
    {
        for (int i = 1; i < PhotonNetwork.PlayerList.Length; i++)
        {
            if(PhotonNetwork.PlayerList[i].ActorNumber == _actorID){
                bestPlayerListBox[i - 1].AddScoreEffect(_score);
            }
        }
    }
    public virtual void RefreshPlayeScore(bool isFinal)
    {
        int maxScore = -1;
        for (int i = 1; i < PhotonNetwork.PlayerList.Length; i++)
        {
            int score;
            if (isFinal)
                score = RoomData.GetInstance().FinalScores[PhotonNetwork.PlayerList[i].ActorNumber];
            else
                score = RoomData.GetInstance().currGameScores[PhotonNetwork.PlayerList[i].ActorNumber];
            bestPlayerListBox[i - 1].SetScore(score);
            //bestPlayerListBox[currIndex].GetComponentInChildren<Text>().color = (tmpA.myIndex < 0 || !tmpA.gameObject.activeInHierarchy) ? Color.red : Color.black;
            bestPlayerListBox[i-1].SetInfoUI(RoomData.GetInstance().playerColor[i - 1], Color.black, (i) + " : " + PhotonNetwork.PlayerList[i].NickName + "||  Score :" + score);

            if (maxScore < score)
            {
                maxScore = score;
            }
        }
        for (int i = 0; i < bestPlayerListBox.Count; i++)
        {
            bestPlayerListBox[i].SetWinnerImg(bestPlayerListBox[i].score >= maxScore);

            if (PhotonNetwork.PlayerList.Length-1 <= i)
            {
                bestPlayerListBox[i].SetInfoUI(Color.black, Color.white, "XXX");
            }
        }
    }

    public virtual void AddThisPlayerToPlayerList(GameObject viewer)
    {
        if (viewer && !BestPlayerLists.Contains(viewer))
            BestPlayerLists.Add(viewer);
    }
    #endregion

    #region Camera
    public void ToggleGameView()
    {
        topView = !topView;
        VC_TPS.m_Priority = topView ? 9 : 11;
    }

    public void Observe(int _shipIndex, bool _isSet = true)
    {
        if (_isSet)
            ObserveIndex = _shipIndex;
        else
            ObserveIndex += _shipIndex;
        ObserveIndex = Mathf.Clamp(ObserveIndex, 0, AllShip.Count);

        int foundedIndex = -1;
        for (int i = ObserveIndex; i < ObserveIndex + AllShip.Count; i++)
        {
            if ((AllShip[i % AllShip.Count] == null || AllShip[i % AllShip.Count].GetComponent<Player_Combat_Ship>().health <= 0) == false)
            {
                foundedIndex = i;
                break;
            }
        }
        if (foundedIndex >= 0)
            SetMyShip(AllShip[foundedIndex], false);
    }
    #endregion


    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(currPlayTime);
        }
        else
        {
            currPlayTime = (float)stream.ReceiveNext();
        }
    }
}