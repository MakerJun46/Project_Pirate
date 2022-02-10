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

    public Text LandingEscape_Button_Text;

    private float steeringRot;
    [SerializeField]private Image SteeringImg;

    public GameObject Island_Landing_UI;
    public GameObject PlayerInfo_UI_Panel;
    bool PlayerInfo_UI_Opened = false;

    public List<Island_Info> All_Island;
    public List<Player_Controller_Ship> AllShip;
    public List<GameObject> MySailors;
    public Player_Controller_Ship MyShip;
    public Camera MainCamera;
    public CinemachineVirtualCamera VC_Top;
    public CinemachineVirtualCamera VC_TPS;
    private bool topView=true;

    [SerializeField] private float deathFieldRadius = 100;
    [SerializeField] private ParticleSystem DeathFieldPS;
    [SerializeField] private LayerMask deathFieldLayer;

    [SerializeField] private Vector3 camOffset = new Vector3(0, 372, -290);

    public int Resource_Wood_Count;
    public int Resource_Rock_Count;
    public int My_Sailor_Count;

    public bool MyShip_On_Landing_Point;
    public GameObject Landing_Button_Blur;


    [SerializeField] GameObject[] ObstaclePrefabs;
    [SerializeField] LayerMask WaterLayer;


    [SerializeField] Material WallMaterial;
    [SerializeField] LayerMask WallThroughLayer;


    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        MainCamera = Camera.main;
        if (PhotonNetwork.IsMasterClient || PhotonNetwork.IsConnected==false)
        {
            StartCoroutine("DeathFieldCoroutine");
        }

        MyShip_On_Landing_Point = false;
        getStartResource();
    }

    public void getStartResource()
    {
        Item_Inventory _item = Item_Manager.GetInstance().Resource_item_list[0];
        Item_Manager.GetInstance().AddItem(_item);
        Item_Manager.GetInstance().AddItem(_item);
        Item_Manager.GetInstance().AddItem(_item);
        Item_Manager.GetInstance().AddItem(_item);
        Item_Manager.GetInstance().AddItem(_item);

        _item = Item_Manager.GetInstance().Resource_item_list[1];
        Item_Manager.GetInstance().AddItem(_item);
        Item_Manager.GetInstance().AddItem(_item);
        Item_Manager.GetInstance().AddItem(_item);
        Item_Manager.GetInstance().AddItem(_item);
        Item_Manager.GetInstance().AddItem(_item);
    }
    public void GenerateObstacles()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            for (int i = 0; i < 20;)
            {
                Vector3 radomPos = new Vector3(Random.Range(-1f, 1f),0, Random.Range(-1f, 1f)) * 200f;
                RaycastHit hit;
                if (Physics.SphereCast(radomPos + Vector3.up * 100, 10f, Vector3.down, out hit, 200f))
                {
                    if (hit.transform.CompareTag("Sea"))
                    {
                        i++;
                        GameObject gameObj= PhotonNetwork.Instantiate("Obstacle_Rock", radomPos, Quaternion.identity);
                    }
                }
            }
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
        UI_Resources_Text_Update();
        UI_Panel_Update();

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

            var dir = Camera.main.transform.position - MyShip.transform.position;
            var ray = new Ray(MyShip.transform.position, dir.normalized);
            Debug.DrawRay(MyShip.transform.position, dir.normalized * 1000f, Color.blue);
            //if (Physics.Raycast(ray, 10000f, WallThroughLayer))
            //{
            //    WallMaterial.SetFloat(Shader.PropertyToID("_Size"), 1);
            // }
            //else
            //    WallMaterial.SetFloat(Shader.PropertyToID("_Size"), 0);
            var view = Camera.main.WorldToViewportPoint(MyShip.transform.position);
            WallMaterial.SetVector(Shader.PropertyToID("_Position"), view);
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
                AllShip[i].GetComponent<PhotonView>().RPC("Attacked", RpcTarget.AllBuffered, new object[] {10f, Vector3.zero });
            }
        }
        StartCoroutine("DeathFieldCoroutine");
    }


    /// <summary>
    /// 플레이어 자원 표시 업데이트
    /// </summary>
    public void UI_Resources_Text_Update()
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

        My_Sailor_Count = my_Sailors.Count;
        MySailors = my_Sailors;

        Resource_Wood_Count = 0;
        Resource_Rock_Count = 0;

        foreach(Item_Inventory _item in Item_Manager.GetInstance().Player_items)
        {
            if (_item.name == "Wood")
                Resource_Wood_Count++;
            else if (_item.name == "Rock")
                Resource_Rock_Count++;
        }

        UI_Wood_Count.text = Resource_Wood_Count.ToString();
        UI_Rock_Count.text = Resource_Rock_Count.ToString();
        UI_Sailor_Count.text = My_Sailor_Count.ToString();
    }
    public void UI_Panel_Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            PlayerInfo_UI_Opened = !PlayerInfo_UI_Opened;
            Item_Manager.instance.ResetCombineTable();
        }

        if (PlayerInfo_UI_Opened)
        {
            PlayerInfo_UI_Panel.SetActive(true);
        }
        else
        {
            PlayerInfo_UI_Panel.SetActive(false);
        }

        if(MyShip_On_Landing_Point)
        {
            Landing_Button_Blur.SetActive(false);
        }
        else
        {
            Landing_Button_Blur.SetActive(true);
        }
    }

    public void island_LandingEscape_Button()
    {
        if (MyShip_On_Landing_Point && !MyShip.is_Landing)
        {
            Island_Landing_UI.SetActive(true);
            Island_Landing_UI.GetComponent<Island_Landing_UI>().Load_island_Info(All_Island[MyShip.Landed_island_ID]);
            MyShip.Ship_Stop();
        }
        else if (MyShip.is_Landing)
        {
            foreach(GameObject go in MySailors)
            {
                go.GetComponent<Sailor>().status = Sailor.Sailor_Status.Escaping;
            }

            MyShip.is_Landing = false;
            LandingEscape_Button_Text.text = "Landing";
        }
    }

    public void island_Landing_Accept_Button()
    {
        Island_Landing_UI.GetComponent<Island_Landing_UI>().Landing();
        Island_Landing_UI.SetActive(false);
        MyShip.is_Landing = true;
        LandingEscape_Button_Text.text = "Escape";
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
}
