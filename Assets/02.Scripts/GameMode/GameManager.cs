using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;
using System.Linq;
using UnityEngine.EventSystems;
using TMPro;

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

    [SerializeField] protected TextMeshProUGUI TimeText;

    [SerializeField] float boosterCoolTime = 5f;

    public float steeringRot { get; set; }
    private Image SteeringImg;
    [SerializeField] protected GameObject ControllerUI;
    [SerializeField] private GameObject[] GoStopBtns;

    public GameObject BestPlayerContent;
    private List<GameObject> BestPlayerLists = new List<GameObject>();
    public int BestPlayerCount => BestPlayerLists.Count;
    List<PlayerScoreList> bestPlayerListBox = new List<PlayerScoreList>();


    public List<Player_Controller_Ship> AllShip;
    public Player_Controller_Ship MyShip;
    public CinemachineVirtualCamera VC_Top;
    protected bool topView = true;

    [SerializeField] private Vector3 camOffset = new Vector3(0, 372, -290);

    protected bool IsWinner;
    [SerializeField] protected GameObject WinPanel;
    [SerializeField] protected GameObject LosePanel;
    [SerializeField] protected GameObject ObserverModePanel;

    [SerializeField] protected GameObject UI_Observer;
    [SerializeField] protected GameObject ObserverCameras_Parent;

    public void InitializePlayerScore(bool isFinal)
    {
        bestPlayerListBox.Clear();
        for (int i = 0; i < BestPlayerContent.transform.childCount; i++)
        {
            bestPlayerListBox.Add(BestPlayerContent.transform.GetChild(i).GetComponent<PlayerScoreList>());
        }
        RefreshPlayeScore(isFinal);
    }

    protected virtual void Start()
    {
        instance = this;
        IsWinner = false;
        currPlayTime = 0;

        // SetButtonEvent
        EventTrigger.Entry entry_PointerDown = new EventTrigger.Entry();
        entry_PointerDown.eventID = EventTriggerType.PointerDown;
        entry_PointerDown.callback.AddListener((data) => { Turn_Right_Button_Down((PointerEventData)data); });
        ControllerUI.transform.GetChild(0).GetComponent<EventTrigger>().triggers.Add(entry_PointerDown);
        EventTrigger.Entry entry_PointerUp = new EventTrigger.Entry();
        entry_PointerUp.eventID = EventTriggerType.PointerUp;
        entry_PointerUp.callback.AddListener((data) => { Turn_Right_Button_Up((PointerEventData)data); });
        ControllerUI.transform.GetChild(0).GetComponent<EventTrigger>().triggers.Add(entry_PointerUp);

        EventTrigger.Entry entry_PointerDown2 = new EventTrigger.Entry();
        entry_PointerDown2.eventID = EventTriggerType.PointerDown;
        entry_PointerDown2.callback.AddListener((data) => { Turn_Left_Button_Down((PointerEventData)data); });
        ControllerUI.transform.GetChild(1).GetComponent<EventTrigger>().triggers.Add(entry_PointerDown2);
        EventTrigger.Entry entry_PointerUp2 = new EventTrigger.Entry();
        entry_PointerUp2.eventID = EventTriggerType.PointerUp;
        entry_PointerUp2.callback.AddListener((data) => { Turn_Left_Button_Up((PointerEventData)data); });
        ControllerUI.transform.GetChild(1).GetComponent<EventTrigger>().triggers.Add(entry_PointerUp2);

        GoStopBtns = new GameObject[2];
        GoStopBtns[0] = ControllerUI.transform.GetChild(2).gameObject;
        GoStopBtns[1] = ControllerUI.transform.GetChild(3).gameObject;
        GoStopBtns[0].GetComponent<Button>().onClick.AddListener(GoOrStop_Button);
        GoStopBtns[1].GetComponent<Button>().onClick.AddListener(GoOrStop_Button);
        ControllerUI.transform.GetChild(5).GetComponent<Button>().onClick.AddListener(Booster_Button);
        
        SteeringImg = ControllerUI.transform.GetChild(4).GetComponent<Image>();
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

            BestPlayerContent.SetActive(false);
            RefreshPlayeScore(true);
        }
    }
    public virtual void SetMyShip(Player_Controller_Ship _myShip, bool _setMyShip = true)
    {
        if (_setMyShip)
            MyShip = _myShip;
        VC_Top.m_Follow = _myShip.transform;
        VC_Top.m_LookAt = _myShip.transform;
    }
    #endregion

    #region GameFlow
    public virtual void StartGame()
    {
        GameStarted = true;
        IsWinner = false;
        ControllerUI.SetActive(true);

        CombatManager.instance.InitialLevelUp();
    }

    public virtual void EndGame()
    {
        if (PhotonNetwork.IsConnected == false || PhotonNetwork.IsMasterClient)
        {
            RoomData.GetInstance().SetFinalScore();
            RoomData.GetInstance().AddPlayedGameCount();

            FindObjectOfType<NetworkManager>().GoToLobby();
        }
    }

    public virtual void MasterChanged(bool _isMaster)
    {
    }


    /// <summary>
    /// 맨 마지막에 딱 한 번 실행되어야함
    /// </summary>
    /// <param name="_win"></param>
    public virtual void JudgeWinLose()
    {
        WinPanel.SetActive(IsWinner);
        LosePanel.SetActive(!IsWinner);
        if (IsWinner)
            OptionSettingManager.GetInstance().Play("Win", false);
        else
            OptionSettingManager.GetInstance().Play("Lose", false);

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

    bool endTimeTirgger = false;
    protected virtual void Update()
    {
        if (TimeText)
        {
            float calculatedTime = maxPlayTime - currPlayTime;
            TimeText.text = ((int)(calculatedTime / 60)) + ":" + ((int)(calculatedTime % 60));
        }
        if (GameStarted)
        {
            int countDownTime = 5;
            if (endTimeTirgger == false && maxPlayTime - countDownTime <= currPlayTime)
            {
                endTimeTirgger = true;
                FindObjectOfType<NetworkManager>().StartCountDown(countDownTime);
            }

            if (PhotonNetwork.IsMasterClient)
            {
                currPlayTime += Time.deltaTime;
            }
            else
            {
                // 방향 조정
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
        }
    }
    #endregion

    #region UI Control Button
    public void Turn_Left_Button_Down(PointerEventData data)
    {
        MyShip.is_Turn_Left = true;
    }

    public void Turn_Left_Button_Up(PointerEventData data)
    {
        MyShip.is_Turn_Left = false;
    }

    public void Turn_Right_Button_Down(PointerEventData data)
    {
        MyShip.is_Turn_Right = true;
    }
    public void Turn_Right_Button_Up(PointerEventData data)
    {
        MyShip.is_Turn_Right = false;
    }
    public void GoOrStop_Button()
    {
        MyShip.GoOrStop_Button();
    }
    public void ActiveGoOrStopBtn()
    {
        GoStopBtns[0].gameObject.SetActive(MyShip.goOrStop);
        GoStopBtns[1].gameObject.SetActive(!MyShip.goOrStop);
    }
    public void Booster_Button()
    {
        StartCoroutine(Booster_CoolTime());
    }

    IEnumerator Booster_CoolTime()
    {
        ControllerUI.transform.GetChild(6).gameObject.SetActive(true);
        MyShip.startBooster();

        Image boosterCoolTimeImg = ControllerUI.transform.GetChild(6).GetComponent<Image>();

        float fillAmount = 1f;

        while(fillAmount > 0)
        {
            fillAmount -= 1f / boosterCoolTime * Time.deltaTime;

            boosterCoolTimeImg.fillAmount = fillAmount;

            yield return null;
        }

        ControllerUI.transform.GetChild(6).gameObject.SetActive(false);
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
                if(bestPlayerListBox.Count> i - 1)
                    bestPlayerListBox[i - 1].AddScoreEffect(_score);
            }
        }
    }
    public virtual void RefreshPlayeScore(bool isFinal)
    {
        int maxScore = -1;
        for (int i = 1; i < PhotonNetwork.PlayerList.Length; i++)
        {
            int score=0;
            if (isFinal)
                score = RoomData.GetInstance().FinalScores[PhotonNetwork.PlayerList[i].ActorNumber];
            else
                score = RoomData.GetInstance().currGameScores[PhotonNetwork.PlayerList[i].ActorNumber];

            bestPlayerListBox[i-1].SetScore(score);

            string tmp = (string)PhotonNetwork.PlayerList[i].CustomProperties["ProfileIndex"];
            int profileIndex = int.Parse(tmp);
            bestPlayerListBox[i-1].SetInfoUI(profileIndex, PhotonNetwork.PlayerList[i].NickName + "  점수 :" + score);

            if (maxScore < score)
            {
                maxScore = score;
            }
        }

        for (int i = 0; i < bestPlayerListBox.Count; i++)
        {

            bestPlayerListBox[i].SetWinnerImg(bestPlayerListBox[i].score >= maxScore && bestPlayerListBox[i].score>0);

            /*
            if (PhotonNetwork.PlayerList.Length-1 <= i)
            {
                bestPlayerListBox[i].SetInfoUI(Color.black,  "XXX");
            }
            */
        }
    }

    public virtual void AddThisPlayerToPlayerList(GameObject viewer)
    {
        if (viewer && !BestPlayerLists.Contains(viewer))
            BestPlayerLists.Add(viewer);
    }
    #endregion

    #region Camera

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
