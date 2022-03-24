using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class CharacterCustomize : MonoBehaviour
{
    [SerializeField] GameObject HatObj;
    [SerializeField] GameObject AccessoryObj;
    [SerializeField] GameObject ClothObj;
    [SerializeField] GameObject[] SkinObjs;

    public void EquipCostume(int typeIndex, int index)
    {
        Costume tmpCostume=null;
        if (index >= 0)
            tmpCostume = CustomizeManager.GetInstance().costumeDictionary.costumes[index];
            
        switch ((Costume.CostumeType)typeIndex)
        {
            case Costume.CostumeType.Hat:
                if (index >= 0)
                {
                    HatObj.GetComponent<MeshFilter>().mesh = tmpCostume.itemMesh[0];
                    HatObj.GetComponent<MeshRenderer>().material = tmpCostume.itemMaterial[0];
                }
                else
                {
                    HatObj.GetComponent<MeshFilter>().mesh = null;
                    HatObj.GetComponent<MeshRenderer>().material = null;
                }
                break;
            case Costume.CostumeType.Accessory:
                if (index >= 0)
                {
                    AccessoryObj.GetComponent<MeshFilter>().mesh = tmpCostume.itemMesh[0];
                    AccessoryObj.GetComponent<MeshRenderer>().material = tmpCostume.itemMaterial[0];
                }else{
                    AccessoryObj.GetComponent<MeshFilter>().mesh = null;
                    AccessoryObj.GetComponent<MeshRenderer>().material = null;
                }
                break;
            case Costume.CostumeType.Cloth:
                if (index >= 0)
                {
                    ClothObj.GetComponent<MeshFilter>().mesh = tmpCostume.itemMesh[0];
                    ClothObj.GetComponent<MeshRenderer>().material = tmpCostume.itemMaterial[0];
                }
                else
                {
                    ClothObj.GetComponent<MeshFilter>().mesh = null;
                    ClothObj.GetComponent<MeshRenderer>().material = null;
                }
                break;
            case Costume.CostumeType.Skin:
                // Body Tail Ear Face
                for(int i = 0; i < 4; i++)
                {
                    if (index >= 0)
                    {
                        //SkinObjs[i].GetComponent<MeshFilter>().mesh = tmpCostume.itemMesh[i];
                        SkinObjs[i].GetComponent<MeshRenderer>().material = tmpCostume.itemMaterial[i];
                    }
                    else
                    {
                        SkinObjs[i].GetComponent<MeshRenderer>().material = null;
                    }
                }
                break;
            default:
                Debug.LogError("There is no such CostumeType");
                break;
        }

    }
}
