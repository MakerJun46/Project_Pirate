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
    Text[] bestPlayerTexts;


    public List<Player_Controller_Ship> AllShip;
    public Player_Controller_Ship MyShip;
    public CinemachineVirtualCamera VC_Top;
    public CinemachineVirtualCamera VC_TPS;
    protected bool topView = true;

    [SerializeField] private Vector3 camOffset = new Vector3(0, 372, -290);

    protected bool IsWinner;
    [SerializeField] protected GameObject WinPanel;
    [SerializeField] protected GameObject LosePanel;

    [SerializeField] protected GameObject UI_Observer;
    [SerializeField] protected GameObject ObserverCameras_Parent;

    protected virtual void Start()
    {
        instance = this;
        IsWinner = false;
        currPlayTime = 0;

        bestPlayerTexts = BestPlayerContent.GetComponentsInChildren<Text>();
    }

    public virtual void SetObserverCamera()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            UI_Observer.SetActive(true);
            ObserverCameras_Parent.SetActive(true);

            for (int i = 0; i < AllShip.Count; i++)
            {
                ObserverCameras_Parent.transform.GetChild(i).GetComponent<CinemachineVirtualCamera>().LookAt = AllShip[i].gameObject.transform;
                ObserverCameras_Parent.transform.GetChild(i).GetComponent<CinemachineVirtualCamera>().Follow = AllShip[i].gameObject.transform;
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
        if (IsWinner)
        {
            RoomData.GetInstance().GetComponent<PhotonView>().RPC("SetScoreRPC", RpcTarget.AllBuffered,
                PhotonNetwork.LocalPlayer.ActorNumber, RoomData.GetInstance().Scores[PhotonNetwork.LocalPlayer.ActorNumber] + 1);
        }

        if (PhotonNetwork.IsConnected == false || PhotonNetwork.IsMasterClient)
            RoomData.GetInstance().GetComponent<PhotonView>().RPC("AddPlayedGameCount", RpcTarget.AllBuffered);
        FindObjectOfType<NetworkManager>().GoToLobby();
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
        print("End : " + IsWinner);
        GameStarted = false;


        //if (MyShip)
        //{
        //    MyShip.ActiveWinLoseEffect(IsWinner);
        //    Change_VC_Lookat(MyShip.GetComponent<PhotonView>().ViewID);
        //}
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
    public virtual void RefreshBestPlayer(GameObject viewer)
    {
        if (viewer && !BestPlayerLists.Contains(viewer))
            BestPlayerLists.Add(viewer);

        int forCount = BestPlayerLists.Count;
        for (int i = forCount - 1; i >= 0; i--)
        {
            if (BestPlayerLists[i] == null)
                BestPlayerLists.RemoveAt(i);
        }

        BestPlayerLists.Sort(delegate (GameObject _A, GameObject _B)
        {
            Player_Controller_Ship A = _A.GetComponent<Player_Controller_Ship>();
            Player_Controller_Ship B = _B.GetComponent<Player_Controller_Ship>();

            int aScore = RoomData.GetInstance().Scores[A.GetComponent<PhotonView>().Owner.ActorNumber];
            int bScore = RoomData.GetInstance().Scores[B.GetComponent<PhotonView>().Owner.ActorNumber];
            if (aScore > bScore)
            {
                return -1;
            }
            else if (aScore < bScore)
            {
                return 1;
            }
            else
            {
                if (A.myIndex > B.myIndex)
                {
                    return -1;
                }
                else if (A.myIndex < B.myIndex)
                {
                    return 1;
                }
                else
                {
                    if (A.deadTime > B.deadTime)
                        return -1;
                    else
                        return 1;
                }
            }
        });
        SetBestPlayerListTexts(bestPlayerTexts);
    }

    public virtual void SetBestPlayerListTexts(Text[] texts)
    {
        int j = 0;
        for (int i = 0; i < texts.Length; i++)
        {
            if (BestPlayerLists.Count > i)
            {
                if (BestPlayerLists[i] != null)
                {
                    Player_Controller_Ship tmpA = BestPlayerLists[i].GetComponent<Player_Controller_Ship>();
                    Player_Controller_Ship tmpB = BestPlayerLists[j].GetComponent<Player_Controller_Ship>();

                    texts[i].color = (tmpA.myIndex < 0 || !tmpB.gameObject.activeInHierarchy) ? Color.red : Color.black;

                    if (RoomData.GetInstance().Scores != null)
                        texts[i].text = (i + 1) + " : " + tmpA.myName + "||  Score :" + RoomData.GetInstance().Scores[tmpA.GetComponent<PhotonView>().Owner.ActorNumber];
                }
            }
            else
            {
                texts[i].text = "null";
            }
            j++;
        }
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
