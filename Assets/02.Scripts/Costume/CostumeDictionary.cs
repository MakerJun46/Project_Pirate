using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CostumeDictionary", menuName = "Costume/CostumeDictionary")]
public class CostumeDictionary : ScriptableObject
{
    public List<Costume> HatCostumes;
    public List<Costume> ClothCostumes;
    public List<Costume> SkinCostumes;

    private void OnValidate()
    {
        for (int i = 0; i < HatCostumes.Count; i++)
        {
            HatCostumes[i].itemID = ((int)HatCostumes[i].costumeType) * 10 + i;
        }
        for (int i = 0; i < ClothCostumes.Count; i++)
        {
            ClothCostumes[i].itemID = ((int)ClothCostumes[i].costumeType) * 10 + i;
        }
        for (int i = 0; i < SkinCostumes.Count; i++)
        {
            SkinCostumes[i].itemID = ((int)SkinCostumes[i].costumeType) * 10 + i;
        }
    }
}
