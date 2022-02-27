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
                islandName.text = "���� ��";
                break;
            case Island_Info.Island_Type.cake:
                islandName.text = "����ũ ��";
                break;
            case Island_Info.Island_Type.ice:
                islandName.text = "���� ��";
                break;
            case Island_Info.Island_Type.mushroom:
                islandName.text = "���� ��";
                break;
            case Island_Info.Island_Type.ruins:
                islandName.text = "���� ��";
                break;
            case Island_Info.Island_Type.toy:
                islandName.text = "�峭�� ��";
                break;
            default:
                break;
        }

        WoodCount.text = "���� ������ ���� �� : " + island.Wood_Object.Count.ToString();
        RockCount.text = "ä�� ������ ���� �� : " + island.Rock_Object.Count.ToString();
        SailorCount.text = "���� ���� : " + Count;
        countReset();
    }


    public void UpButton()
    {
        if(GameManager.GetIstance().My_Sailor_Count > Count)
        {
            Count++;
            SailorCount.text = "���� ���� : " + Count;
        }
    }

    public void DownButton()
    {
        if(Count > 0)
        {
            Count--;
            SailorCount.text = "���� ���� : " + Count;
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