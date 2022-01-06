using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item_Manager : MonoBehaviour
{
    public List<Item_Inventory> items;

    [SerializeField] Transform slotParent;
    [SerializeField] Item_Slot[] slots;


    private void OnValidate()
    {
        slots = slotParent.GetComponentsInChildren<Item_Slot>();
    }

    private void Awake()
    {
        FreshSlots();
    }

    public void FreshSlots()
    {
        int i = 0;
        for(; i < items.Count && i < slots.Length; i++)
        {
            slots[i].item = items[i];
        }
        for(; i < slots.Length; i++)
        {
            slots[i].item = null;
        }
    }

    public void AddItem(Item_Inventory _item)
    {
        if(items.Count < slots.Length)
        {
            items.Add(_item);
            FreshSlots();
        }
        else
        {
            Debug.Log("½½·Ô¿¡ ºó ÀÚ¸®°¡ ¾øÀ½");
        }
    }

}
