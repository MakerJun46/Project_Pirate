using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class CharacterCustomize : MonoBehaviourPun
{
    [SerializeField] GameObject[] HatObj;
    [SerializeField] GameObject[] ClothObj;
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
                for(int i = 0; i < HatObj.Length; i++)
                {
                    HatObj[i].SetActive(false);
                }
                if (index >= 0)
                {
                    HatObj[tmpCostume.itemMeshIndex].SetActive(true);
                    HatObj[tmpCostume.itemMeshIndex].GetComponent<MeshRenderer>().material = tmpCostume.itemMaterial[0];
                }
                break;
            case Costume.CostumeType.Cloth:
                for (int i = 0; i < ClothObj.Length; i++)
                {
                    ClothObj[i].SetActive(false);
                }
                if (index >= 0)
                {
                    ClothObj[tmpCostume.itemMeshIndex].SetActive(true);
                    ClothObj[tmpCostume.itemMeshIndex].GetComponent<SkinnedMeshRenderer>().material = tmpCostume.itemMaterial[0];
                }
                break;
            case Costume.CostumeType.Skin:
                // Body Tail Ear Face
                for(int i = 0; i < 4; i++)
                {
                    SkinObjs[i].GetComponent<SkinnedMeshRenderer>().material = index >= 0 ? tmpCostume.itemMaterial[i] : null;
                }
                break;
            default:
                Debug.LogError("There is no such CostumeType");
                break;
        }

    }
}
