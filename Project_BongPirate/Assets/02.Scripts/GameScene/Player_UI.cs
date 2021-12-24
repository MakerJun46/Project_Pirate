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
        UI_Update();
    }

    public void UI_Update()
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

        SailorCount_Text.text = "absdfasdf";
        //WoodCount_Text.text = WoodCount.ToString();
        //RockCount_Text.text = RockCount.ToString();
    }
}
