using System;
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
        if (instance == null)
        {
            instance = FindObjectOfType<Item_Manager>();
        }
        return instance;
    }

    public List<Item_Inventory> Resource_item_list;
    public List<Item_Inventory> Player_items;
    public List<Item_Inventory> CombineTable_items;
    public List<Item_Inventory> equip_items;

    public Image dragImage;
    public GameObject DragItem;
    public bool onDraging;
    public Transform OnMouseDragParent; // UGUI에서 드래그 중인 아이템이 다른 오브젝트에 가리지 않기 위해 부모 변경

    [SerializeField] Transform InventorySlotParent;
    [SerializeField] Item_Slot[] inventorySlots;

    [SerializeField] Transform CombineSlotParent;
    [SerializeField] CombineTable_Slot[] CombineSlots;

    [SerializeField] Transform TreasureChestSlotParent;
    [SerializeField] Item_Slot[] TreasureChestSlots;

    [SerializeField] Transform MyInventorySlotParent;
    [SerializeField] Item_Slot[] MyInventorySlots;

    [SerializeField] Transform ShipSlotParent;
    [SerializeField] Item_Slot[] ShipSlots;

    private void OnValidate()
    {
        inventorySlots = InventorySlotParent.GetComponentsInChildren<Item_Slot>();
        CombineSlots = CombineSlotParent.GetComponentsInChildren<CombineTable_Slot>();
        TreasureChestSlots = TreasureChestSlotParent.GetComponentsInChildren<Item_Slot>();
        MyInventorySlots = MyInventorySlotParent.GetComponentsInChildren<Item_Slot>();
        ShipSlots = ShipSlotParent.GetComponentsInChildren<Item_Slot>();
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
        for (; i < Player_items.Count && i < inventorySlots.Length; i++)
        {
            inventorySlots[i].item = Player_items[i];
        }
        for (; i < inventorySlots.Length; i++)
        {
            inventorySlots[i].item = null;
        }

        //i = 0;
        //for (; i < CombineTable_items.Count && i < CombineSlots.Length; i++)
        //{
        //    CombineSlots[i].item = CombineTable_items[i];
        //}
        //for (; i < CombineSlots.Length; i++)
        //{
        //    CombineSlots[i].item = null;
        //}
    }

    public void FreshSlots_TreasureChest(TreasureChest TC)
    {
        Debug.Log("Fresh TresureChest");

        int i = 0;
        for (; i < TC.items.Count; i++)
        {
            TreasureChestSlots[i].item = TC.items[i];
        }
        for (; i < TreasureChestSlots.Length; i++)
        {
            TreasureChestSlots[i].item = null;
        }

        i = 0;
        for (; i < Player_items.Count && i < MyInventorySlots.Length; i++)
        {
            MyInventorySlots[i].item = Player_items[i];
        }
        for (; i < inventorySlots.Length; i++)
        {
            MyInventorySlots[i].item = null;
        }
    }

    public void AddItem(Item_Inventory _item)
    {
        if (Player_items.Count < inventorySlots.Length)
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
        if (CombineTable_items.Count < CombineSlots.Length)
        {
            CombineTable_items.Add(_item);
            FreshSlots();
        }
        else
        {
            Debug.Log("조합 테이블 슬롯에 빈 자리가 없음");
        }
    }
    public void Combine(Item_Inventory item)
    {
        Debug.Log("combine - itemmanager");
        Debug.Log("combinedItem Count : " + CombineTable_items.Count);
        for (int i = 0; i < CombineTable_items.Count; i++)
        {
            var target = Player_items.Find(x => x.itemCode == CombineTable_items[i].itemCode);
            Debug.Log("target : " + target);
            Player_items.Remove(target);
        }
        Player_items.Add(item);
        ResetCombineTable();
    }

    public void ResetCombineTable()
    {
        Debug.Log("ResetTable");
        Combine_Item.instance.ClearTable();
        CombineTable_items.Clear();
        FreshSlots();
    }

    public void DeleteItem(Item_Inventory _item)
    {
        Player_items.Remove(_item);
        FreshSlots();
    }

    public CombineTable_Slot NearSlot_CombineTable(Vector3 Pos)
    {
        float min_dis = 50f;
        float Min = 10000f;
        int index = -1;

        for (int i = 0; i < CombineSlots.Length; i++)
        {
            Vector2 slotPos = CombineSlots[i].gameObject.transform.position;
            float Dis = Vector2.Distance(slotPos, Pos);

            if (Dis < Min)
            {
                Min = Dis;
                index = i;
            }
        }

        return Min < min_dis ? CombineSlots[index] : null;
    }

    public Item_Slot NearSlot_inventory(Vector3 Pos)
    {
        float min_dis = 20f;
        float Min = 10000f;
        int index = -1;

        if(GameManager.GetInstance().GetComponent<BattleRoyalGameManager>().TreasureChest_UI_Panel.activeInHierarchy)
        {
            for (int i = 0; i < MyInventorySlots.Length; i++)
            {
                Vector2 slotPos = MyInventorySlots[i].gameObject.transform.position;
                float Dis = Vector2.Distance(slotPos, Pos);

                if (Dis < Min)
                {
                    Min = Dis;
                    index = i;
                }
            }

            return Min < min_dis ? MyInventorySlots[index] : null;
        }
        else
        {
            for (int i = 0; i < inventorySlots.Length; i++)
            {
                Vector2 slotPos = inventorySlots[i].gameObject.transform.position;
                float Dis = Vector2.Distance(slotPos, Pos);

                if (Dis < Min)
                {
                    Min = Dis;
                    index = i;
                }
            }

            return Min < min_dis ? inventorySlots[index] : null;
        }
    }

    public Item_Slot NearSlot_ShipSlot(Vector3 Pos)
    {
        float min_dis = 40f;
        float Min = 10000f;
        int index = -1;

        for (int i = 0; i < ShipSlots.Length; i++)
        {
            Vector2 slotPos = ShipSlots[i].gameObject.transform.position;
            float Dis = Vector2.Distance(slotPos, Pos);

            if (Dis < Min)
            {
                Min = Dis;
                index = i;
            }
        }

        return Min < min_dis ? ShipSlots[index] : null;
    }

    public void SwapPosition(Item_Slot target_inventory)
    {
        Item_Inventory tmp;
        tmp = target_inventory.item;
        target_inventory.item = DragItem.GetComponent<Item_Slot>().item;
        DragItem.GetComponent<Item_Slot>().item = tmp;
    }

    public void init_Inventory(Item_Slot slot)
    {
        slot.item = DragItem.GetComponent<Item_Slot>().item;

        if(GameManager.GetInstance().GetComponent<BattleRoyalGameManager>().TreasureChest_UI_Panel.activeInHierarchy)
        {
            Player_items.Add(DragItem.GetComponent<Item_Slot>().item);
        }

        DragItem.GetComponent<Item_Slot>().item = null;
    }

    public void init_CombineTable(CombineTable_Slot slot)
    {
        slot.item = DragItem.GetComponent<Item_Slot>().item;
    }

    public void equip_Item(Item_Slot slot)
    {
        slot.item = DragItem.GetComponent<Item_Slot>().item;
        slot.ImgPositiontoZero();
        equip_items.Add(DragItem.GetComponent<Item_Slot>().item);

        var tmp = Player_items.Find(x => x == DragItem.GetComponent<Item_Slot>().item);
        Debug.Log("DragItem : " + DragItem.GetComponent<Item_Slot>().item);
        Debug.Log("Finded" + tmp);

        Player_items.Remove(Player_items.Find(x => x == DragItem.GetComponent<Item_Slot>().item));

        FreshSlots();
    }

    public void Drop(CombineTable_Slot target_CombineTable, Item_Slot target_Inevntory, Item_Slot target_ShipSlot = null, bool isCombineTableItem = false)
    {
        if (target_CombineTable != null && !isCombineTableItem)
        {
            AddItem_CombineTable(DragItem.GetComponentInParent<Item_Slot>()._item);
            init_CombineTable(target_CombineTable);
            Combine_Item.instance.DetectCombine();
        }
        else if(target_Inevntory != null)
        {
            if (target_Inevntory._item != null)
            {
                SwapPosition(target_Inevntory);
            }
            else
            {
                init_Inventory(target_Inevntory);
            }
        }
        else if(target_ShipSlot != null)
        {
            if(target_ShipSlot._item != null)
            {
                // 장착한 아이템과 위치 바꾸는 코드 작성
            }
            else
            {
                equip_Item(target_ShipSlot);
            }
        }
    }
}
