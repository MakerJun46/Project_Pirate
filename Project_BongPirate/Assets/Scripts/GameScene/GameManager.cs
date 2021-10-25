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
    }

    // Update is called once per frame
    void Update()
    {
        updateUI_Text();
    }


    /// <summary>
    /// 플레이어 자원 표시 업데이트
    /// </summary>
    void updateUI_Text()
    {
        UI_Wood_Count.text = Resource_Wood_Count.ToString();
        UI_Rock_Count.text = Resource_Rock_Count.ToString();
        UI_Sailor_Count.text = Resource_Sailor_Count.ToString();
    }

    public void island_Landing_Button()
    {
        Island_Landing_UI.SetActive(false);
        MyShip.GetComponent<Player_Controller_Ship>().Ship_MoveSpeed_Reset();
    }
}
