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
    public CostumeDictionary costumeDictionary;
    [SerializeField] Transform costumeContainer;

    //[SerializeField] GameObject EquipBtn;
    //[SerializeField] GameObject UnEquipBtn;

    public int myHatIndex;
    public int myClothIndex;
    public int mySkinndex;

    Costume.CostumeType selectedCostumeType;
    int selectedIndex;

    GameObject selectedHatItem;
    GameObject selectedClothItem;
    GameObject selectedSkinItem;

    [SerializeField] CharacterCustomize characterCustomize;

    private void Start()
    {
        DontDestroyOnLoad(this.gameObject);


        for (int i = 0; i < costumeDictionary.HatCostumes.Count; i++)
        {
            print("yse");
            GameObject costuemBtn = Instantiate(costumeContainer.GetChild(0).gameObject, costumeContainer);
            costuemBtn.transform.name = costumeDictionary.HatCostumes[i].itemID + "_" + costumeDictionary.HatCostumes[i].costumeType.ToString() + "_" + costumeDictionary.HatCostumes[i].itemName;
            int itemID = costumeDictionary.HatCostumes[i].itemID;
            Costume.CostumeType type = costumeDictionary.HatCostumes[i].costumeType;
            costuemBtn.GetComponent<Button>().onClick.AddListener(() => SelectItem(costuemBtn.GetComponent<Button>(), type, itemID));
            costuemBtn.GetComponentInChildren<Text>().text = costumeDictionary.HatCostumes[i].itemName;
            costuemBtn.transform.GetChild(0).GetComponent<Image>().sprite = costumeDictionary.HatCostumes[i].itemPreview;
            costuemBtn.gameObject.SetActive(true);
        }
        for (int i = 0; i < costumeDictionary.ClothCostumes.Count; i++)
        {
            GameObject costuemBtn = Instantiate(costumeContainer.GetChild(0).gameObject, costumeContainer);
            costuemBtn.transform.name = costumeDictionary.ClothCostumes[i].itemID + "_" + costumeDictionary.ClothCostumes[i].costumeType.ToString() + "_" + costumeDictionary.ClothCostumes[i].itemName;
            int itemID = costumeDictionary.ClothCostumes[i].itemID;
            Costume.CostumeType type = costumeDictionary.ClothCostumes[i].costumeType;
            costuemBtn.GetComponent<Button>().onClick.AddListener(() => SelectItem(costuemBtn.GetComponent<Button>(), type, itemID));
            costuemBtn.GetComponentInChildren<Text>().text = costumeDictionary.ClothCostumes[i].itemName;
            costuemBtn.transform.GetChild(0).GetComponent<Image>().sprite = costumeDictionary.ClothCostumes[i].itemPreview;
            costuemBtn.gameObject.SetActive(true);
        }

        selectedCostumeType = Costume.CostumeType.Skin;
        selectedIndex = ((int)selectedCostumeType) * 10 + Random.Range(0, 4);
        for (int i = 0; i < costumeDictionary.SkinCostumes.Count; i++)
        {
            GameObject costuemBtn = Instantiate(costumeContainer.GetChild(0).gameObject, costumeContainer);
            costuemBtn.transform.name = costumeDictionary.SkinCostumes[i].itemID + "_" + costumeDictionary.SkinCostumes[i].costumeType.ToString() + "_" + costumeDictionary.SkinCostumes[i].itemName;
            int itemID = costumeDictionary.SkinCostumes[i].itemID;
            Costume.CostumeType type = costumeDictionary.SkinCostumes[i].costumeType;
            costuemBtn.GetComponent<Button>().onClick.AddListener(() => SelectItem(costuemBtn.GetComponent<Button>(), type, itemID));
            costuemBtn.GetComponentInChildren<Text>().text = costumeDictionary.SkinCostumes[i].itemName;
            costuemBtn.transform.GetChild(0).GetComponent<Image>().sprite = costumeDictionary.SkinCostumes[i].itemPreview;
            costuemBtn.gameObject.SetActive(true);

            if(selectedIndex== itemID)
            {
                SelectItem(costuemBtn.GetComponent<Button>(), Costume.CostumeType.Skin, itemID);
            }
        }
    }
    private void Update()
    {
        bool inLobby = (PhotonNetwork.IsConnected && SceneManager.GetActiveScene().name == "Lobby");
        if (inLobby == false)
        {
            costumePanel.SetActive(false);
        }
        //OptionSettingManager.GetInstance().ActiveCustomPanelOpenBtn(inLobby);
    }
    public void SelectCategory(int _index)
    {
        for(int i=1;i< costumeContainer.childCount; i++)
        {
            if (_index >= 0)
            {
                string tmpCostumeType = ((Costume.CostumeType)_index).ToString();
                costumeContainer.GetChild(i).gameObject.SetActive(costumeContainer.GetChild(i).name.Contains(tmpCostumeType));
            }
            else
            {
                costumeContainer.GetChild(i).gameObject.SetActive(true);
            }
        }
    }
    private void SelectItem(Button _btn,Costume.CostumeType type, int _index)
    {
        selectedCostumeType = type;
        selectedIndex = _index;

        //print("selected :" + selectedCostumeType+" / "+ selectedIndex);

        switch (type)
        {
            case Costume.CostumeType.Hat:
                if (selectedHatItem)
                {
                    // 이전 것 되돌리기
                    selectedHatItem.GetComponent<Image>().color = Color.white;
                    selectedHatItem.transform.GetChild(2).gameObject.SetActive(false);
                    if (selectedHatItem == _btn.gameObject)
                    {
                        UnEquip();
                        selectedHatItem = null;
                        return;
                    }
                }
                selectedHatItem = _btn.gameObject;
                selectedHatItem.GetComponent<Image>().color = Color.grey;
                selectedHatItem.transform.GetChild(2).gameObject.SetActive(true);
                break;
            case Costume.CostumeType.Cloth:
                if (selectedClothItem)
                {
                    // 이전 것 되돌리기
                    selectedClothItem.GetComponent<Image>().color = Color.white;
                    selectedClothItem.transform.GetChild(2).gameObject.SetActive(false);
                    if (selectedClothItem == _btn.gameObject)
                    {
                        UnEquip();
                        selectedClothItem = null;
                        return;
                    }
                }
                selectedClothItem = _btn.gameObject;
                selectedClothItem.GetComponent<Image>().color = Color.grey;
                selectedClothItem.transform.GetChild(2).gameObject.SetActive(true);
                break;
            case Costume.CostumeType.Skin:
                if (selectedSkinItem)
                {
                    // 이전 것 되돌리기
                    selectedSkinItem.GetComponent<Image>().color = Color.white;
                    selectedSkinItem.transform.GetChild(2).gameObject.SetActive(false);
                }
                selectedSkinItem = _btn.gameObject;
                selectedSkinItem.GetComponent<Image>().color = Color.grey;
                selectedSkinItem.transform.GetChild(2).gameObject.SetActive(true);
                break;
        }

        SetCostumeIndex();

        /*
        if (selectedIndex==myHatIndex || selectedIndex==myClothIndex)
        {
            EquipBtn.SetActive(false);
            UnEquipBtn.SetActive(true);
        }else{
            EquipBtn.SetActive(true);
            UnEquipBtn.SetActive(false);
        }
        */
    }
    public void UnEquip()
    {
        switch (selectedCostumeType)
        {
            case Costume.CostumeType.Hat:
                myHatIndex = -1;
                break;
            case Costume.CostumeType.Cloth:
                myClothIndex = -1;
                break;
            case Costume.CostumeType.Skin:
                // Skin은 장착 해제 불가
                return;
            default:
                break;
        }
        characterCustomize.EquipCostume((int)selectedCostumeType, -1);
        /*
        EquipBtn.SetActive(true);
        UnEquipBtn.SetActive(false);
        */
    }
    public void SetCostumeIndex()
    {
        switch (selectedCostumeType)
        {
            case Costume.CostumeType.Hat:
                myHatIndex = selectedIndex;
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
        if (selectedCostumeType != Costume.CostumeType.Skin)
        {
            // Skin이라면 장착 해제가 있어서는 안 됨
            /*
            EquipBtn.SetActive(false);
            UnEquipBtn.SetActive(true);
            */
        }
    }


    public void EquipCostume(int viewID)
    {
        GameObject tmpObj = PhotonView.Find(viewID).gameObject;
        tmpObj.GetComponent<PhotonView>().RPC("EquipCostume", RpcTarget.AllBuffered, 0,myHatIndex);
        tmpObj.GetComponent<PhotonView>().RPC("EquipCostume", RpcTarget.AllBuffered, 1, myClothIndex);
        tmpObj.GetComponent<PhotonView>().RPC("EquipCostume", RpcTarget.AllBuffered, 2, mySkinndex);
    }

    public void ResetBtn()
    {
        selectedCostumeType = Costume.CostumeType.Hat;
        if (selectedHatItem)
        {
            // 이전 것 되돌리기
            selectedHatItem.GetComponent<Image>().color = Color.white;
            selectedHatItem.transform.GetChild(2).gameObject.SetActive(false);
            UnEquip();
            selectedHatItem = null;
        }
        selectedCostumeType = Costume.CostumeType.Cloth;
        if (selectedClothItem)
        {
            // 이전 것 되돌리기
            selectedClothItem.GetComponent<Image>().color = Color.white;
            selectedClothItem.transform.GetChild(2).gameObject.SetActive(false);
            UnEquip();
            selectedClothItem = null;
        }
        if (selectedSkinItem)
        {
            // 이전 것 되돌리기
            selectedSkinItem.GetComponent<Image>().color = Color.white;
            selectedSkinItem.transform.GetChild(2).gameObject.SetActive(false);
        }

        myHatIndex = -1;
        myClothIndex = -1;
        mySkinndex = 20;

        characterCustomize.EquipCostume((int)Costume.CostumeType.Hat, -1);
        characterCustomize.EquipCostume((int)Costume.CostumeType.Cloth, -1);
        characterCustomize.EquipCostume((int)Costume.CostumeType.Skin, 20);

        /*
        EquipBtn.SetActive(true);
        UnEquipBtn.SetActive(false);
        */
    }
}
