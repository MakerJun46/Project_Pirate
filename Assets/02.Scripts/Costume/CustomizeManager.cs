using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CustomizeManager : MonoBehaviour
{
    private static CustomizeManager instance;
    public static CustomizeManager GetInstance()
    {
        if (instance == null)
        {
            instance = FindObjectOfType<CustomizeManager>();
        }
        return instance;
    }

    [SerializeField] GameObject costumePanel;
    [SerializeField] GameObject costumePanelOpenBtn;
    public CostumeDictionary costumeDictionary;
    [SerializeField] Transform costumeContainer;

    [SerializeField] GameObject EquipBtn;
    [SerializeField] GameObject UnEquipBtn;

    public int myHatIndex;
    public int myAccessoryIndex;
    public int myClothIndex;
    public int mySkinndex;

    Costume.CostumeType selectedCostumeType;
    int selectedIndex;

    GameObject selectedCostumeItem;

    [SerializeField] CharacterCustomize characterCustomize;

    private void Start()
    {
        DontDestroyOnLoad(this.gameObject);

        myHatIndex = Random.Range(0, 3);
        myAccessoryIndex = Random.Range(3, 6);
        myClothIndex = Random.Range(6, 9);
        mySkinndex = Random.Range(9, 12);

        characterCustomize.EquipCostume(0, myHatIndex);
        characterCustomize.EquipCostume(1, myAccessoryIndex);
        characterCustomize.EquipCostume(2, myClothIndex);
        characterCustomize.EquipCostume(3, mySkinndex);

        for (int i = 0; i < costumeDictionary.costumes.Count; i++)
        {
            GameObject costuemBtn = Instantiate(costumeContainer.GetChild(0).gameObject, costumeContainer);
            costuemBtn.transform.name = costumeDictionary.costumes[i].itemID + "_" + costumeDictionary.costumes[i].costumeType.ToString() + "_" + costumeDictionary.costumes[i].itemName;
            int itemID = costumeDictionary.costumes[i].itemID;
            Costume.CostumeType type = costumeDictionary.costumes[i].costumeType;
            costuemBtn.GetComponent<Button>().onClick.AddListener(() => SelectItem(costuemBtn.GetComponent<Button>(),type, itemID));
            costuemBtn.GetComponentInChildren<Text>().text = costumeDictionary.costumes[i].itemName;
            costuemBtn.gameObject.SetActive(true);
        }
    }
    private void Update()
    {
        bool inLobby = (PhotonNetwork.IsConnected && SceneManager.GetActiveScene().name == "Lobby");
        if (inLobby == false)
        {
            costumePanelOpenBtn.SetActive(false);
            costumePanel.SetActive(false);
        }
        else
        {
            costumePanelOpenBtn.SetActive(true);
        }
    }

    private void SelectItem(Button _btn,Costume.CostumeType type, int _index)
    {
        selectedCostumeType = type;
        selectedIndex = _index;

        if(selectedCostumeItem)
            selectedCostumeItem.GetComponent<Image>().color = Color.white;
        selectedCostumeItem = _btn.gameObject;
        selectedCostumeItem.GetComponent<Image>().color = Color.grey;
        

        if (selectedIndex==myHatIndex || selectedIndex==myClothIndex || selectedIndex == myAccessoryIndex || selectedIndex == mySkinndex)
        {
            EquipBtn.SetActive(false);
            UnEquipBtn.SetActive(true);
        }
        else
        {
            EquipBtn.SetActive(true);
            UnEquipBtn.SetActive(false);
        }
    }
    public void UnEquip()
    {
        switch (selectedCostumeType)
        {
            case Costume.CostumeType.Hat:
                myHatIndex = -1;
                break;
            case Costume.CostumeType.Accessory:
                myAccessoryIndex = -1;
                break;
            case Costume.CostumeType.Cloth:
                myClothIndex = -1;
                break;
            case Costume.CostumeType.Skin:
                mySkinndex = -1;
                break;
            default:
                break;
        }
        characterCustomize.EquipCostume((int)selectedCostumeType, -1);
        EquipBtn.SetActive(true);
        UnEquipBtn.SetActive(false);
    }
    public void SetCostumeIndex()
    {
        switch (selectedCostumeType)
        {
            case Costume.CostumeType.Hat:
                myHatIndex = selectedIndex;
                break;
            case Costume.CostumeType.Accessory:
                myAccessoryIndex = selectedIndex;
                break;
            case Costume.CostumeType.Cloth:
                myClothIndex = selectedIndex;
                break;
            case Costume.CostumeType.Skin:
                mySkinndex = selectedIndex;
                break;
            default:
                break;
        }
        characterCustomize.EquipCostume((int)selectedCostumeType, selectedIndex);
        EquipBtn.SetActive(false);
        UnEquipBtn.SetActive(true);
    }


    public void EquipCostume(int viewID)
    {
        GameObject tmpObj = PhotonView.Find(viewID).gameObject;
        tmpObj.GetComponent<PhotonView>().RPC("EquipCostume", RpcTarget.AllBuffered, 0,myHatIndex);
        tmpObj.GetComponent<PhotonView>().RPC("EquipCostume", RpcTarget.AllBuffered, 1,myAccessoryIndex);
        tmpObj.GetComponent<PhotonView>().RPC("EquipCostume", RpcTarget.AllBuffered, 2, myClothIndex);
        tmpObj.GetComponent<PhotonView>().RPC("EquipCostume", RpcTarget.AllBuffered, 3, mySkinndex);
    }
}
