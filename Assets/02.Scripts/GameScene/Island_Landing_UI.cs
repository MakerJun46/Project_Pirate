using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Island_Landing_UI : MonoBehaviour
{
    public Text SailorCount;
    public int Count;

    public Text islandName;
    public Text islandDiscription;
    public Text WoodCount;
    public Text RockCount;

    private void Start()
    {
        Count = 0;
    }
    public void countReset()
    {
        Count = 0;
    }

    public void Load_island_Info(Island_Info island)
    {
        switch(island.type)
        {
            case Island_Info.Island_Type.normal:
                islandName.text = "보통 섬";
                break;
            case Island_Info.Island_Type.cake:
                islandName.text = "케이크 섬";
                break;
            case Island_Info.Island_Type.ice:
                islandName.text = "빙하 섬";
                break;
            case Island_Info.Island_Type.mushroom:
                islandName.text = "버섯 섬";
                break;
            case Island_Info.Island_Type.ruins:
                islandName.text = "유적 섬";
                break;
            case Island_Info.Island_Type.toy:
                islandName.text = "장난감 섬";
                break;
            default:
                break;
        }

        WoodCount.text = "벌목 가능한 목재 수 : " + island.Wood_Object.Count.ToString();
        RockCount.text = "채석 가능한 석재 수 : " + island.Rock_Object.Count.ToString();
        SailorCount.text = "선원 선택 : " + Count;
        countReset();
    }


    public void UpButton()
    {
        if(GameManager.GetIstance().My_Sailor_Count > Count)
        {
            Count++;
            SailorCount.text = "선원 선택 : " + Count;
        }
    }

    public void DownButton()
    {
        if(Count > 0)
        {
            Count--;
            SailorCount.text = "선원 선택 : " + Count;
        }
    }

    public void Landing()
    {
        for(int i = 0; i < Count; i++)
        {
            GameManager.GetIstance().MySailors[i].GetComponent<Sailor>().status = Sailor.Sailor_Status.Landing;
        }
    }
}