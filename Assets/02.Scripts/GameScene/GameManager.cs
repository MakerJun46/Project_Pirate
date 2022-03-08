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

    public bool GameStart;

    PhotonView PV;
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

    public bool MyShip_On_Landing_Point;
    public GameObject Landing_Button_Blur;
    public int PlayerCount;
    protected float playTime;
    [SerializeField]protected Text TimeText;

    protected virtual void Start()
    {
        instance = this;
        MainCamera = Camera.main;
        if (PhotonNetwork.IsMasterClient || PhotonNetwork.IsConnected==false)
        {
            StartCoroutine("DeathFieldCoroutine");
        }
        PV = GetComponent<PhotonView>();
    }


    public virtual void SetMyShip(Player_Controller_Ship _myShip, bool _setMyShip=true)
    {
        if(_setMyShip)
            MyShip = _myShip;
        VC_Top.m_Follow = _myShip.transform;
        VC_Top.m_LookAt = _myShip.transform;
        VC_TPS.m_Follow = _myShip.transform;
        VC_TPS.m_LookAt = _myShip.transform;
    }

    public void ToggleGameView()
    {
        topView = !topView;
        VC_TPS.m_Priority = topView ? 9 : 11;
    }

    private int ObserveIndex;
    public void Observe(int index, bool add = false)
    {
        if (add)
        {
            ObserveIndex += index;
        }
        else
        {
            ObserveIndex = index;
        }

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

    [SerializeField] GameObject WinPanel;
    [SerializeField] GameObject LosePanel;


    public virtual void EndGame(bool _win)
    {
        if (_win)
        {
            WinPanel.SetActive(true);
            LosePanel.SetActive(false);
        }
        else
        {
            WinPanel.SetActive(false);
            LosePanel.SetActive(true);
        }
    }
    public virtual void MasterChanged(bool _isMaster)
    {
    }

    protected virtual void Update()
    {
        if (MyShip) {
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
        {
            MyShip.UpgradeShip();
        }
    }

    public virtual void RefreshBestPlayer(GameObject viewer)
    {
        if (viewer)
        {
            if (!BestPlayerLists.Contains(viewer))
            {
                BestPlayerLists.Add(viewer);
            }
        }

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

            if (bestPlayerSortIndex <= 1)
            {
                //Money로 정렬
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
                    {
                        return -1;
                    }
                    else if (A.deadTime < B.deadTime)
                    {
                        return 1;
                    }
                    else
                        return 0;
                }
            }
            else
            {
                //나중에 이름순으로 정렬?
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
                    {
                        return -1;
                    }
                    else if (A.deadTime < B.deadTime)
                    {
                        return 1;
                    }
                    else
                        return 0;
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

                    if (bestPlayerSortIndex == 1)
                    {
                        texts[i].color = Color.black;

                        if (BestPlayerLists.Count > j)
                        {
                            while (tmpB.myIndex < 0 || !tmpB.gameObject.activeSelf)
                            {
                                j++;
                                if (BestPlayerLists.Count <= j)
                                    break;
                            }
                        }
                    }
                    else
                    {
                        if (tmpA.myIndex < 0 || !tmpB.gameObject.activeInHierarchy)
                            texts[i].color = Color.red;
                        else
                            texts[i].color = Color.black;
                    }


                    if (bestPlayerSortIndex == 0)
                    {
                        texts[i].text = (i + 1) + " : " + tmpA.myName + "||  Index :" + tmpA.myIndex;
                    }
                    else if (bestPlayerSortIndex == 1)
                    {
                        if (BestPlayerLists.Count > j)
                            texts[i].text = (i + 1) + " : " + tmpB.myName + "||  Index :" + tmpB.myIndex;
                        else
                            texts[i].text = "";
                    }
                    else
                    {
                        texts[i].text = (i + 1) + " : " + tmpA.myName + "||  Index :" + tmpA.myIndex;
                    }
                }
            }
            else
            {
                texts[i].text = "null";
            }
            j++;
        }
    }
}
