using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;
using System.Linq;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
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
    public bool GameStart;
    // Don't Judge Win or Lose
    [SerializeField] protected bool DebugMode;

    private float steeringRot;
    [SerializeField]private Image SteeringImg;


    public List<Player_Controller_Ship> AllShip;
    public Player_Controller_Ship MyShip;
    public Camera MainCamera;
    public CinemachineVirtualCamera VC_Top;
    public CinemachineVirtualCamera VC_TPS;
    protected bool topView=true;

    [SerializeField] private Vector3 camOffset = new Vector3(0, 372, -290);

    public GameObject BestPlayerContent;
    public List<GameObject> BestPlayerLists = new List<GameObject>();
    public Text[] bestPlayerTexts;
    public Text bestPlayerSortText;
    public int bestPlayerSortIndex = 0; // 0:most money

    public int PlayerCount;
    protected float playTime;

    private int ObserveIndex;

    [SerializeField]protected Text TimeText;

    protected bool IsWinner;
    [SerializeField] protected GameObject WinPanel;
    [SerializeField] protected GameObject LosePanel;

    protected virtual void Start()
    {
        instance = this;
        MainCamera = Camera.main;
        IsWinner = false;
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
        GameStart = true;
        IsWinner = false;
    }
    public virtual void EndGame()
    {
        GameStart = false;
        FindObjectOfType<NetworkManager>().GoToLobby();
    }

    public virtual void MasterChanged(bool _isMaster)
    {
    }

    /// <summary>
    /// 맨 마지막에 딱 한 번 실행되어야함
    /// </summary>
    /// <param name="_win"></param>
    public virtual void JudgeWinLose(bool _win)
    {
        IsWinner = _win;
        WinPanel.SetActive(_win);
        LosePanel.SetActive(!_win);
    }

    protected virtual void Update()
    {
        if(TimeText)
            TimeText.text = ((int)(playTime / 60)) + ":" + ((int)(playTime % 60));

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

        // 일단은 부모에서는 시간 제한만 둠(자식이 오버라이딩해서 새로운 조건 추가)
        if (GameStart==true && (playTime >= 60) && DebugMode==false)
        {
            if (IsWinner) {
                RoomData.GetInstance().GetComponent<PhotonView>().RPC("SetScoreRPC", RpcTarget.AllBuffered,
                    PhotonNetwork.LocalPlayer.ActorNumber, RoomData.GetInstance().Scores[PhotonNetwork.LocalPlayer.ActorNumber] + 1);
            }
            FindObjectOfType<NetworkManager>().StartEndGame(false);
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
}
