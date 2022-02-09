using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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
    public List<Item_Inventory> CombineTable_items;

    public Image dragImage;
    public GameObject DragItem;
    public bool onDraging;
    public Transform OnMouseDragParent; // UGUI에서 드래그 중인 아이템이 다른 오브젝트에 가리지 않기 위해 부모 변경

    [SerializeField] Transform InventorySlotParent;
    [SerializeField] Item_Slot[] inventorySlots;

    [SerializeField] Transform CombineSlotParent;
    [SerializeField] CombineTable_Slot[] CombineSlots;

    private void OnValidate()
    {
        inventorySlots = InventorySlotParent.GetComponentsInChildren<Item_Slot>();
        CombineSlots = CombineSlotParent.GetComponentsInChildren<CombineTable_Slot>();
    }

    private void Update()
    {
        
    }

    private void Awake()
    {
        FreshSlots();
        onDraging = false;
    }

    public void FreshSlots()
    {
        int i = 0;
        for(; i < Player_items.Count && i < inventorySlots.Length; i++)
        {
            inventorySlots[i].item = Player_items[i];
        }
        for(; i < inventorySlots.Length; i++)
        {
            inventorySlots[i].item = null;
        }

        i = 0;
        for (; i < CombineTable_items.Count && i < CombineSlots.Length; i++)
        {
            CombineSlots[i].item = CombineTable_items[i];
        }
        for (; i < CombineSlots.Length; i++)
        {
            CombineSlots[i].item = null;
        }
    }

    public void AddItem(Item_Inventory _item)
    {
        if(Player_items.Count < inventorySlots.Length)
        {
            Player_items.Add(_item);
            FreshSlots();
        }
        else
        {
            Debug.Log("슬롯에 빈 자리가 없음");
        }
    }

    public void AddItem_CombineTable(Item_Inventory _item)
    {
        if(CombineTable_items.Count < CombineSlots.Length)
        {
            CombineTable_items.Add(_item);
            FreshSlots();
        }
        else
        {
            Debug.Log("조합 테이블 슬롯에 빈 자리가 없음");
        }
    }

    public void ResetCombineTable()
    {
        CombineTable_items.Clear();
        FreshSlots();
    }

    public void DeleteItem(Item_Inventory _item)
    {
        Player_items.Remove(_item);
        FreshSlots();
    }

    public CombineTable_Slot NearSlot(Vector3 Pos)
    {
        float min_dis = 50f;
        float Min = 10000f;
        int index = -1;

        for(int i = 0; i < CombineSlots.Length; i++)
        {
            Vector2 slotPos = CombineSlots[i].gameObject.transform.position;
            float Dis = Vector2.Distance(slotPos, Pos);

            if(Dis < Min)
            {
                Min = Dis;
                index = i;
            }
        }

        return Min < min_dis ? CombineSlots[index] : null;
    }

    public void Drop(CombineTable_Slot target)
    {
        Debug.Log("Target cell : " + target);
        Debug.Log("_item : " + DragItem.GetComponentInParent<Item_Slot>()._item);

        AddItem_CombineTable(DragItem.GetComponentInParent<Item_Slot>()._item);
    }
}
