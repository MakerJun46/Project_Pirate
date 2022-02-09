using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Item_Slot : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IDropHandler
{
    [SerializeField] Image img;

    public Vector2 defaultPosition;
    public GameObject itemObject;
    public Item_Inventory _item;
    public Item_Inventory item
    {
        get { return _item; }
        set
        {
            _item = value;
            if(_item != null)
            {
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
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("Drag Start");

        if(_item != null && !Item_Manager.instance.onDraging)
        {
            Item_Manager.instance.onDraging = true;
            Item_Manager.instance.DragItem = itemObject;
            //itemObject.transform.parent = Item_Manager.instance.OnMouseDragParent.transform;
        }

    }

    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log("Dragingg...");

        Vector2 currentPos = Input.mousePosition;
        itemObject.transform.position = currentPos;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("Drag End");
    }

    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("Drop");
        CombineTable_Slot target = Item_Manager.instance.NearSlot(itemObject.transform.position);

        if(target != null)
        {
            itemObject.transform.position = defaultPosition;
            Item_Manager.instance.Drop(target);
        }
        else
        {
            itemObject.transform.position = defaultPosition;
        }

        Item_Manager.instance.onDraging = false;
    }
}
