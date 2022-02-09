using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CombineTable_Slot : MonoBehaviour
{
    [SerializeField] Image img;

    private Item_Inventory _item;
    public Item_Inventory item
    {
        get { return _item; }
        set
        {
            _item = value;
            if (_item != null)
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
}
