using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public GameManager instance;

    public Text UI_Wood_Count;
    public Text UI_Rock_Count;
    public Text UI_Sailor_Count;

    public GameObject Island_Landing_UI;

    public GameObject MyShip;
    public Camera MainCamera;
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
    }

    // Update is called once per frame
    void Update()
    {
        updateUI_Text();
        if(MyShip)
            MainCamera.transform.position = MyShip.transform.position + camOffset;
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
        MyShip.GetComponent<Player_Controller_Ship>().Ship_MoveSpeed_Reset();
    }

    public void Turn_Left_Button_Down()
    {
        MyShip.GetComponent<Player_Controller_Ship>().is_Turn_Left = true;
    }

    public void Turn_Left_Button_Up()
    {
        MyShip.GetComponent<Player_Controller_Ship>().is_Turn_Left = false;
    }

    public void Turn_Right_Button_Down()
    {
        MyShip.GetComponent<Player_Controller_Ship>().is_Turn_Right = true;
    }
    public void Turn_Right_Button_Up()
    {
        MyShip.GetComponent<Player_Controller_Ship>().is_Turn_Right = false;
    }
    public void GoOrStop_Button()
    {
        MyShip.GetComponent<Player_Controller_Ship>().goOrStop = !MyShip.GetComponent<Player_Controller_Ship>().goOrStop;
    }

}
