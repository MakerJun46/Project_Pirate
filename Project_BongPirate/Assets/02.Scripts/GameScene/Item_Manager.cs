using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item_Manager : MonoBehaviour
{
    public static Item_Manager instance;

    public static Item_Manager GetInstance()
    {
        if(instance == null)
        {
            instance = FindObjectOfType<Item_Manager>();
        }
        return instance;
    }

    public List<Item_Inventory> item_list;
    public List<Item_Inventory> Player_items;

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
        for(; i < Player_items.Count && i < slots.Length; i++)
        {
            slots[i].item = Player_items[i];
        }
        for(; i < slots.Length; i++)
        {
            slots[i].item = null;
        }
    }

    public void AddItem(Item_Inventory _item)
    {
        if(Player_items.Count < slots.Length)
        {
            Player_items.Add(_item);
            FreshSlots();
        }
        else
        {
            Debug.Log("½½·Ô¿¡ ºó ÀÚ¸®°¡ ¾øÀ½");
        }
    }

}
