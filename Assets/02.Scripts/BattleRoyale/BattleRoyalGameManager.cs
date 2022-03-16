using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class BattleRoyalGameManager : GameManager
{
    #region Variables & Initializer
    public List<Island_Info> All_Island;
    public List<GameObject> MySailors;

    public int Resource_Wood_Count;
    public int Resource_Rock_Count;
    public int My_Sailor_Count;

    [Header("[DeathField]")]
    [SerializeField] private float deathFieldRadius = 100;
    [SerializeField] private GameObject DeathFieldObj;
    [SerializeField] private LayerMask deathFieldLayer;

    [Header("[Info UI]")]
    bool PlayerInfo_UI_Opened = false;
    public GameObject PlayerInfo_UI_Panel;
    public GameObject Island_Landing_UI;
    public GameObject TreasureChest_UI_Panel;

    public Text UI_Wood_Count;
    public Text UI_Rock_Count;
    public Text UI_Sailor_Count;

    public bool MyShip_On_Landing_Point;
    public GameObject Landing_Button_Blur;
    public Text LandingEscape_Button_Text;

    [Header("[MiniMap]")]
    [SerializeField] GameObject miniMap;
    [SerializeField] protected GameObject WorldMap;
    [SerializeField] MinimapCamera minimapCam;
    [SerializeField] Material MyshipColor;

    /* 벽 투명하게 보이도록
    [SerializeField] Material WallMaterial;
    [SerializeField] LayerMask WallThroughLayer;
    */

    protected override void Start()
    {
        base.Start();
        if (PhotonNetwork.IsMasterClient || PhotonNetwork.IsConnected == false)
        {
            StartCoroutine("DeathFieldCoroutine");
        }

        MyShip_On_Landing_Point = false;
        Resource_Wood_Count = 0;
        Resource_Rock_Count = 0;
        My_Sailor_Count = 0;

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

    public override void SetMyShip(Player_Controller_Ship _myShip,bool _SetMyShip)
    {
        base.SetMyShip(_myShip, _SetMyShip);

        if (_SetMyShip)
        {
            minimapCam.Player = _myShip.gameObject;
            MyShip.transform.Find("MinimapCircle").GetComponent<Renderer>().sharedMaterial = MyshipColor;
        }
    }
    IEnumerator DeathFieldCoroutine()
    {
        yield return new WaitForSeconds(1f);

        // 범위 내 존재하지 않는 플레이어를 찾아서 Attack
        Collider[] innerFieldShips = Physics.OverlapSphere(Vector3.zero, deathFieldRadius, deathFieldLayer);
        List<Player_Controller_Ship> tmpColls = new List<Player_Controller_Ship>();
        foreach (Collider c in innerFieldShips)
        {
            tmpColls.Add(c.GetComponent<Player_Controller_Ship>());
        }
        for (int i = 0; i < AllShip.Count; i++)
        {
            if (tmpColls.Contains(AllShip[i]) == false)
            {
                AllShip[i].GetComponent<PhotonView>().RPC("Attacked", RpcTarget.AllBuffered, new object[] { 5f, Vector3.zero });
            }
        }
        StartCoroutine("DeathFieldCoroutine");
    }

    #endregion

    protected override void Update()
    {
        base.Update();


        UI_Resources_Text_Update();
        UI_Panel_Update();


        deathFieldRadius -= Time.deltaTime;
        deathFieldRadius = Mathf.Clamp(deathFieldRadius, 10, 10000);
        DeathFieldObj.gameObject.transform.localScale = Vector3.one * deathFieldRadius / 250f;

        if (GameStart)
        {
            if (playTime >= 60)
            {
                // 시간이 끝났을 때,
                // 내 배가 없거나 health가 0이라면 패배, 아니라면 승리
                bool Win = !(MyShip == null || MyShip.GetComponent<Player_Combat_Ship>().health <= 0);
                JudgeWinLose(Win);
            }
            else
            {
                // 시간이 아직 끝나지 않았으면
                playTime += Time.deltaTime;

                // 남아있는 player와 그 index를 확인
                List<Player_Controller_Ship> survivedShips = new List<Player_Controller_Ship>();
                for(int i = 0; i < AllShip.Count; i++)
                {
                    if (AllShip[i] != null && AllShip[i].GetComponent<Player_Combat_Ship>().health > 0)
                    {
                        survivedShips.Add(AllShip[i]);
                    }
                }

                // 남아있는 플레이어가 1명 이하일 때, 그 플레이어가 내 플레이어라면 승리, 아니라면 패배
                if (survivedShips.Count <= 1)
                {
                    bool Win = false;
                    for (int i=0;i< survivedShips.Count; i++)
                    {
                        if (survivedShips[i] == MyShip)
                        {
                            Win = true;
                            break;
                        }
                    }
                    JudgeWinLose(Win);
                    if (IsWinner)
                    {
                        RoomData.GetInstance().GetComponent<PhotonView>().RPC("SetScoreRPC", RpcTarget.AllBuffered,
                            PhotonNetwork.LocalPlayer.ActorNumber, RoomData.GetInstance().Scores[PhotonNetwork.LocalPlayer.ActorNumber] + 1);
                    }
                    FindObjectOfType<NetworkManager>().StartEndGame(false);
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            ToggleGameView();
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            miniMap.SetActive(WorldMap.activeInHierarchy);
            WorldMap.SetActive(!WorldMap.activeInHierarchy);
        }

        /* See Through Wall  
        if (MyShip)
        {
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
        }*/

    }

    public override void JudgeWinLose(bool _win)
    {
        base.JudgeWinLose(_win);
        print("Judge This Game : " + _win);
    }

    public void SpawnSailor(int count, Transform _ship)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            GameObject go = PhotonNetwork.Instantiate("Sailor", Vector3.zero, Quaternion.identity);
            go.transform.parent = _ship.transform.Find("SailorSpawnPos");
            go.transform.localPosition = Vector3.zero;
        }
    }

    #region UI
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

        foreach (Item_Inventory _item in Item_Manager.GetInstance().Player_items)
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
        if (Input.GetKeyDown(KeyCode.Tab) && !TreasureChest_UI_Panel.activeInHierarchy)
        {
            PlayerInfo_UI_Opened = !PlayerInfo_UI_Opened;
            Item_Manager.instance.ResetCombineTable();
        }

        PlayerInfo_UI_Panel.SetActive(PlayerInfo_UI_Opened);
        Landing_Button_Blur.SetActive(!MyShip_On_Landing_Point);
    }
    #endregion

    #region Island Landing
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
            foreach (GameObject go in MySailors)
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
    #endregion

    void OnDrawGizmos()
    {
        // Death Field Sphere
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(Vector3.zero, deathFieldRadius);
    }
}
