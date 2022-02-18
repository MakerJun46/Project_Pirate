using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Item_Slot : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IDropHandler, IPointerDownHandler
{
    [SerializeField] Image img;

    public Vector2 defaultPosition;
    public GameObject itemObject;
    public Item_Inventory _item;

    public bool isCombinedItem;
    public bool isShipSlot;

    public Item_Inventory item
    {
        get { return _item; }
        set
        {
            _item = value;
            if(_item != null)
            {
                img.rectTransform.localPosition = Vector3.zero;
                img.sprite = item.itemImage;
                img.color = new Color(1, 1, 1, 1);

                if(isShipSlot && item.itemCode == 11)
                {
                    equipWeapon();
                }
            }
            else
            {
                img.sprite = null;
                img.color = new Color(1, 1, 1, 0);
            }
        }
    }

    public void equipWeapon()
    {
        CombatManager.instance.EquipCannon(0, 0);
    }

    public void ImgPositiontoZero()
    {
        img.rectTransform.localPosition = Vector3.zero;
    }

    private void Awake()
    {
        itemObject = transform.GetChild(0).gameObject;
        defaultPosition = this.transform.position;

        if (isShipSlot)
            item = null;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (isCombinedItem && item != null)
        {
            Debug.Log("Combine - itemslot");
            Item_Manager.instance.Combine(item);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if(_item != null && !Item_Manager.instance.onDraging)
        {
            Item_Manager.instance.onDraging = true;
            Item_Manager.instance.DragItem = this.gameObject;
            //itemObject.transform.parent = Item_Manager.instance.OnMouseDragParent.transform;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {

        Vector2 currentPos = Input.mousePosition;
        itemObject.transform.position = currentPos;
    }

    public void OnEndDrag(PointerEventData eventData)
    {

    }

    public void OnDrop(PointerEventData eventData)
    {
        CombineTable_Slot target_combineTable = Item_Manager.instance.NearSlot_CombineTable(itemObject.transform.position);
        Item_Slot target_inventory = Item_Manager.instance.NearSlot_inventory(itemObject.transform.position);
        Item_Slot target_Shipslot = Item_Manager.instance.NearSlot_ShipSlot(itemObject.transform.position);

        Debug.Log(target_inventory);
        Debug.Log(target_combineTable);
        Debug.Log(target_Shipslot);

        if(target_combineTable == null && target_inventory == null && target_Shipslot == null)
        {
            itemObject.transform.position = defaultPosition;
            img.rectTransform.localPosition = Vector3.zero;
        }
        else
        {
            itemObject.transform.position = defaultPosition;
            img.rectTransform.localPosition = Vector3.zero;

            if (target_Shipslot != null && item.itemCode < 10)
            {
                itemObject.transform.position = defaultPosition;
                img.rectTransform.localPosition = Vector3.zero;
            }
            else
            {
                Item_Manager.instance.Drop(target_combineTable, target_inventory, target_Shipslot);
            }
        }

        Item_Manager.instance.onDraging = false;
    }

}
