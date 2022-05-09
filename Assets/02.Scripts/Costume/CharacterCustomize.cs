using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class CharacterCustomize : MonoBehaviourPun
{
    [SerializeField] GameObject HatObj;
    [SerializeField] GameObject ClothObj;
    [SerializeField] GameObject[] SkinObjs;

    [PunRPC]
    public void EquipCostume(int typeIndex, int index)
    {
        Costume tmpCostume=null;
        if (index >= 0)
        {
            switch ((Costume.CostumeType)typeIndex)
            {
                case Costume.CostumeType.Hat:
                    tmpCostume = CustomizeManager.GetInstance().costumeDictionary.HatCostumes.Find(s => s.itemID == index);
                    break;
                case Costume.CostumeType.Cloth:
                    tmpCostume = CustomizeManager.GetInstance().costumeDictionary.ClothCostumes.Find(s => s.itemID == index);
                    break;
                case Costume.CostumeType.Skin:
                    tmpCostume = CustomizeManager.GetInstance().costumeDictionary.SkinCostumes.Find(s => s.itemID == index);
                    break;
            }
        }
        switch ((Costume.CostumeType)typeIndex)
        {
            case Costume.CostumeType.Hat:
                if (index >= 0)
                {
                    HatObj.GetComponent<MeshFilter>().mesh = tmpCostume.itemMesh[0];
                    HatObj.GetComponent<MeshRenderer>().material = tmpCostume.itemMaterial[0];

                    if (tmpCostume.itemMaterial.Length == 2)
                    {
                        HatObj.GetComponent<MeshRenderer>().materials[1] = tmpCostume.itemMaterial[1];
                    }

                }
                else
                {
                    HatObj.GetComponent<MeshFilter>().mesh = null;
                    HatObj.GetComponent<MeshRenderer>().material = null;
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
                    SkinObjs[i].GetComponent<MeshRenderer>().material = index >= 0 ? tmpCostume.itemMaterial[i] : null;
                }
                break;
            default:
                Debug.LogError("There is no such CostumeType");
                break;
        }

    }
}
