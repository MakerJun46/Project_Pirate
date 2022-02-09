using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemCombineTable : MonoBehaviour, IDropHandler
{
    public bool onTableItem;

    public Transform slotParent;
    public CombineTable_Slot[] slot;

    void Start()
    {
        onTableItem = false;
    }
    private void OnValidate()
    {
        slot = slotParent.GetComponentsInChildren<CombineTable_Slot>();   
    }

    // Update is called once per frame
    void Update()
    {
        if(onTableItem)
        {

        }
    }


    public void detectCombineItem()
    {

    }

    public void OnDrop(PointerEventData eventData)
    {
        if (Item_Manager.instance.DragItem != null)
        {
            
        }
    }
}
