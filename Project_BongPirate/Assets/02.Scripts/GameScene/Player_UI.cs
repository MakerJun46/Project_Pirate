using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player_UI : MonoBehaviour
{
    // 나의 자원 UI ===============================
    public Text SailorCount_Text;
    public Text WoodCount_Text;
    public Text RockCount_Text;
    public int SailorCount;
    public int WoodCount;
    public int RockCount;
    // 나의 자원 UI ===============================

    // UI
    public GameObject Player_UI_Panel;
    bool player_UI_Open = false;


    // Start is called before the first frame update
    void Start()
    {
        SailorCount = 1;
        WoodCount = 0;
        RockCount = 0;
    }

    // Update is called once per frame
    void Update()
    {
        OpenPlayer_Info();
    }
    
    public void OpenPlayer_Info()
    {
        if(Input.GetKeyDown(KeyCode.Tab))
        {
            player_UI_Open = !player_UI_Open;
        }

        if (player_UI_Open)
            Player_UI_Panel.SetActive(true);
        else
            Player_UI_Panel.SetActive(false);
    }
}

