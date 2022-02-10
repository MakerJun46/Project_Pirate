using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Combine_Item : MonoBehaviour
{
    public static Combine_Item instance;

    public List<string> CombineFolmula = new List<string>();

    public List<Item_Inventory> Combined_items_list; // 모든 조합 아이템

    public CombineTable_Slot[] slots;
    public GameObject slotParent;

    public Item_Slot Combine_Target_Item;

    private void OnValidate()
    {
        slots = slotParent.GetComponentsInChildren<CombineTable_Slot>();
    }

    void Start()
    {
        instance = this;
        Combine_Target_Item.item = null;
    }

    public void ClearTable()
    {
        for(int i = 0; i < slots.Length; i++)
        {
            slots[i].item = null;
        }
        Combine_Target_Item.item = null;
    }

    public void DetectCombine()
    {
        string tmp = "";

        for(int i = 0; i < slots.Length; i++)
        {
            tmp += slots[i].item == null ? "0" : slots[i].item.itemCode.ToString();
        }

        int index = CombineFolmula.FindIndex(i => i == tmp);

        Debug.Log("tmp : " + tmp);
        Debug.Log("index : " + index);

        if(index != -1)
        {
            Combine_Target_Item.item = Combined_items_list[index];
        }
        else
        {
            Combine_Target_Item.item = null;
        }
    }

}
