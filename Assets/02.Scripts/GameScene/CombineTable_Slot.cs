using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CombineTable_Slot : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IDropHandler
{
    [SerializeField] Image img;

    public Vector2 defaultPosition;
    public GameObject itemObject;
    private Item_Inventory _item;
    public Item_Inventory item
    {
        get { return _item; }
        set
        {
            _item = value;
            if (_item != null)
            {
                img.rectTransform.localPosition = Vector3.zero;
                img.sprite = item.itemImage;
                img.color = new Color(1, 1, 1, 1);
            }
            else
            {
                img.color = new Color(1, 1, 1, 0);
            }
        }
    }
    private void Awake()
    {
        itemObject = transform.GetChild(0).gameObject;
        defaultPosition = this.transform.position;

        item = null;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_item != null && !Item_Manager.instance.onDraging)
        {
            Item_Manager.instance.onDraging = true;
            Item_Manager.instance.DragItem = this.gameObject;
            Debug.Log("Drag Item : " + this.gameObject);
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

        Debug.Log("combineTabel : " + target_combineTable);
        Debug.Log("InventoryTabel : " + target_inventory);

        if (target_combineTable == null && target_inventory == null)
        {
            itemObject.transform.position = defaultPosition;
        }
        else
        {
            itemObject.transform.position = defaultPosition;
            Item_Manager.instance.Drop(target_combineTable, target_inventory);
        }

        Item_Manager.instance.onDraging = false;
    }
}
