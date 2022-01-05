using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;
using System.Linq;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    public static GameManager GetIstance()
    {
        if (instance == null)
        {
            instance = FindObjectOfType<GameManager>();
        }
        return instance;
    }

    public Text UI_Wood_Count;
    public Text UI_Rock_Count;
    public Text UI_Sailor_Count;

    private float steeringRot;
    [SerializeField]private Image SteeringImg;

    public GameObject Island_Landing_UI;

    public List<Player_Controller_Ship> AllShip;
    public Player_Controller_Ship MyShip;
    public Camera MainCamera;
    public CinemachineVirtualCamera VC_Top;
    public CinemachineVirtualCamera VC_TPS;
    private bool topView=true;

    [SerializeField] private float deathFieldRadius = 100;
    [SerializeField] private ParticleSystem DeathFieldPS;
    [SerializeField] private LayerMask deathFieldLayer;

    [SerializeField] private Vector3 camOffset = new Vector3(0, 372, -290);

    /// <summary>
    /// 보유한 목재 수
    /// </summary>
    public int Resource_Wood_Count;
    /// <summary>
    /// 보유한 석재 수
    /// </summary>
    public int Resource_Rock_Count;
    /// <summary>
    /// 보유한 선원 수
    /// </summary>
    public int Resource_Sailor_Count;


    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        MainCamera = Camera.main;
        if (PhotonNetwork.IsMasterClient || PhotonNetwork.IsConnected==false)
        {
            StartCoroutine("DeathFieldCoroutine");
        }
    }

    public void SetMyShip(Player_Controller_Ship _myShip)
    {
        MyShip = _myShip;
        VC_Top.m_Follow = _myShip.transform;
        VC_Top.m_LookAt = _myShip.transform;
        VC_TPS.m_Follow = _myShip.transform;
        VC_TPS.m_LookAt = _myShip.transform;
    }
    public void ToggleGameView(bool _topView)
    {
        topView = _topView;
        VC_TPS.m_Priority = topView ? 9 : 11;
    }

    // Update is called once per frame
    void Update()
    {
        updateUI_Text();

        if (Input.GetKeyDown(KeyCode.Q))
        {
            topView = !topView;
            ToggleGameView(topView);
        }

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
        deathFieldRadius -= Time.deltaTime;
        deathFieldRadius = Mathf.Clamp(deathFieldRadius, 10, 10000);
        DeathFieldPS.gameObject.transform.localScale = Vector3.one * deathFieldRadius/250f;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(Vector3.zero, deathFieldRadius);
    }
    IEnumerator DeathFieldCoroutine()
    {
        yield return new WaitForSeconds(1f);
        Collider[] innerFieldShips = Physics.OverlapSphere(Vector3.zero, deathFieldRadius, deathFieldLayer);
        List<Player_Controller_Ship> tmpColls = new List<Player_Controller_Ship>();
        foreach(Collider c in innerFieldShips)
        {
            tmpColls.Add(c.GetComponent<Player_Controller_Ship>());
        }
        for (int i = 0; i < AllShip.Count; i++)
        {
            if (tmpColls.Contains(AllShip[i]) == false)
            {
                AllShip[i].GetComponent<PhotonView>().RPC("Attacked", RpcTarget.AllBuffered, 10f);
            }
        }
        StartCoroutine("DeathFieldCoroutine");
    }


    /// <summary>
    /// 플레이어 자원 표시 업데이트
    /// </summary>
    void updateUI_Text()
    {
        GameObject[] temp = GameObject.FindGameObjectsWithTag("Sailor");
        List<GameObject> my_Sailors = new List<GameObject>();

        foreach (GameObject go in temp)
        {
            if (go.GetComponent<PhotonView>().IsMine)
            {
                my_Sailors.Add(go);
            }
        }

        Debug.Log(my_Sailors.Count);

        UI_Wood_Count.text = Resource_Wood_Count.ToString();
        UI_Rock_Count.text = Resource_Rock_Count.ToString();
        UI_Sailor_Count.text = my_Sailors.Count.ToString();
    }

    public void island_Landing_Button()
    {
        Island_Landing_UI.SetActive(false);
        MyShip.Ship_MoveSpeed_Reset();
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

}
