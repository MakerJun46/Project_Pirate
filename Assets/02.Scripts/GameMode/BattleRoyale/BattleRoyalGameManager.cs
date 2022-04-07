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

    float levelUpTime;
    [SerializeField] GameObject LevelUpPanel;
    [SerializeField] Transform LevelUpBtnContainer;


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
    public override void StartGame()
    {
        base.StartGame();

        LevelUp();
    }

    public void LevelUp()
    {
        LevelUpPanel.SetActive(true);

        if (PhotonNetwork.IsMasterClient == false)
        {
            List<Vector2> randomRoullet = new List<Vector2>();
            Player_Combat_Ship currShip = MyShip.GetComponent<Player_Combat_Ship>();
            int spotIndex = currShip.GetLastSailIndex();
            if (spotIndex >= 0)
            {
                randomRoullet.Add(new Vector2(0, 0));
                randomRoullet.Add(new Vector2(0, 1));
            }
            spotIndex = currShip.GetLastAutoCannonIndex();
            if (spotIndex >= 0)
            {
                randomRoullet.Add(new Vector2(1, 0));
                randomRoullet.Add(new Vector2(1, 1));
                randomRoullet.Add(new Vector2(1, 2));
                randomRoullet.Add(new Vector2(1, 3));
            }
            if (MyShip.upgradeIndex <= 1)
            {
                randomRoullet.Add(new Vector2(3, 0));
            }
            if (MyShip.upgradeIndex >= 1)
            {
                spotIndex = currShip.GetLastmySpecialCannonsIndex();
                if (spotIndex >= 0)
                {
                    randomRoullet.Add(new Vector2(2, 0));
                    randomRoullet.Add(new Vector2(2, 1));
                    randomRoullet.Add(new Vector2(2, 2));
                    randomRoullet.Add(new Vector2(2, 3));
                }
            }


            for (int i = 0; i < LevelUpBtnContainer.childCount; i++)
            {
                GameObject levelUpBtn = LevelUpBtnContainer.GetChild(i).gameObject;


                if (randomRoullet.Count > 0)
                {
                    int selectIndex = Random.Range(0, randomRoullet.Count);
                    Vector2 selectedRoullet = randomRoullet[selectIndex];
                    randomRoullet.RemoveAt(selectIndex);

                    levelUpBtn.SetActive(true);
                    levelUpBtn.GetComponentInChildren<Text>().text = "";
                    levelUpBtn.GetComponent<Button>().onClick.RemoveAllListeners();


                    int levelUpIndex = (int)selectedRoullet.y;

                    switch ((int)selectedRoullet.x)
                    {
                        case 0:
                            levelUpBtn.GetComponentInChildren<Text>().text = "Get Sail " + levelUpIndex;
                            levelUpBtn.GetComponent<Button>().onClick.AddListener(() => CombatManager.instance.EquipSail(currShip.GetLastSailIndex(), levelUpIndex));
                            break;
                        case 1:
                            levelUpBtn.GetComponentInChildren<Text>().text = "Get Cannon " + levelUpIndex;
                            levelUpBtn.GetComponent<Button>().onClick.AddListener(() => CombatManager.instance.EquipCannon(currShip.GetLastAutoCannonIndex(), levelUpIndex));
                            break;
                        case 2:
                            levelUpBtn.GetComponentInChildren<Text>().text = "Get SpecialCannon " + levelUpIndex;
                            levelUpBtn.GetComponent<Button>().onClick.AddListener(() => CombatManager.instance.EquipSpecialCannon(currShip.GetLastmySpecialCannonsIndex(), levelUpIndex));
                            break;
                        case 3:
                            levelUpBtn.GetComponentInChildren<Text>().text = "Upgrade";
                            levelUpBtn.GetComponent<Button>().onClick.AddListener(() => TryUpgradeShip());
                            break;
                    }
                    levelUpBtn.GetComponent<Button>().onClick.AddListener(() => LevelUpPanel.SetActive(false));
                }
                else
                {
                    levelUpBtn.SetActive(false);
                }
            }
        }
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

    #endregion

    protected override void Update()
    {
        base.Update();

        //UI_Resources_Text_Update();
        //UI_Panel_Update();

        if (GameStarted)
        {
            bool shouldGameEnd = false;

            if (currPlayTime >= maxPlayTime)
            {
                shouldGameEnd = true;
            }
            else
            {
                int count = 0;
                for (int i = 0; i < AllShip.Count; i++)
                    if (AllShip[i] != null && AllShip[i].GetComponent<Player_Combat_Ship>().health > 0)
                        count++;
                if (count <= 1) shouldGameEnd = true;
            }

            if (shouldGameEnd)
            {
                FindObjectOfType<NetworkManager>().StartEndGame(false);
            }


            levelUpTime += Time.deltaTime;
            if (levelUpTime >= 10)
            {
                levelUpTime -= 10;
                LevelUp();
            }
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

    public override void JudgeWinLose()
    {
        List<Player_Controller_Ship> survivedShips = new List<Player_Controller_Ship>();
        for (int i = 0; i < AllShip.Count; i++)
        {
            if (AllShip[i] != null && AllShip[i].GetComponent<Player_Combat_Ship>().health > 0)
            {
                survivedShips.Add(AllShip[i]);
            }
        }
        // 마지막에 플레이어가 살아있다면 승리
        IsWinner = survivedShips.Contains(MyShip) && MyShip!=null;

        if(IsWinner)
            RoomData.GetInstance().SetCurrScore(PhotonNetwork.LocalPlayer.ActorNumber, 100);

        base.JudgeWinLose();
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

}
