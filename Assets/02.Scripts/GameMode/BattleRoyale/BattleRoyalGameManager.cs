using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class BattleRoyalGameManager : GameManager
{
    #region Variables & Initializer
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

    /*
    [Header("[MiniMap]")]
    [SerializeField] GameObject miniMap;
    [SerializeField] protected GameObject WorldMap;
    [SerializeField] MinimapCamera minimapCam;
    [SerializeField] Material MyshipColor;
    */

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

        //getStartResource();
    }

    public override void StartGame()
    {
        base.StartGame();

    }


    public override void SetMyShip(Player_Controller_Ship _myShip,bool _SetMyShip)
    {
        base.SetMyShip(_myShip, _SetMyShip);

        /* Minimap
        if (_SetMyShip)
        {
            minimapCam.Player = _myShip.gameObject;
        }
        */
    }

    #endregion

    protected override void Update()
    {
        base.Update();

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
                FindObjectOfType<NetworkManager>().EndGame();
            }
        }

        /* Minimap
        if (Input.GetKeyDown(KeyCode.M))
        {
            miniMap.SetActive(WorldMap.activeInHierarchy);
            WorldMap.SetActive(!WorldMap.activeInHierarchy);
        }
        */

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
}
