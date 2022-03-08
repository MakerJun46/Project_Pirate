using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class TreasureChest : MonoBehaviour
{
    public List<Item_Inventory> items;
    PhotonView PV;

    bool isOpen;

    private void Awake()
    {
        items = new List<Item_Inventory>();
        isOpen = false;
        PV = GetComponent<PhotonView>();
    }

    private void OnTriggerStay(Collider other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            if(Input.GetKeyDown(KeyCode.S))
            {
                if (GameManager.GetInstance().GetComponent<BattleRoyalGameManager>().TreasureChest_UI_Panel.activeInHierarchy==false)
                    OpenTreasureChest();
                else
                    CloseTreasureChest();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            CloseTreasureChest();
        }
    }

    public void OpenTreasureChest()
    {
        GameManager.GetInstance().GetComponent<BattleRoyalGameManager>().TreasureChest_UI_Panel.SetActive(true);
        Item_Manager.instance.FreshSlots_TreasureChest(this);
        PV.RPC("Open", RpcTarget.AllBuffered);
    }

    public void CloseTreasureChest()
    {
        if(GameManager.GetInstance().GetComponent<BattleRoyalGameManager>().TreasureChest_UI_Panel.activeInHierarchy)
            GameManager.GetInstance().GetComponent<BattleRoyalGameManager>().TreasureChest_UI_Panel.SetActive(false);
        PV.RPC("Close", RpcTarget.AllBuffered);
    }

    [PunRPC]
    public void Open()
    {
        isOpen = true;
    }

    [PunRPC]
    public void Close()
    {
        isOpen = false;
    }
}
