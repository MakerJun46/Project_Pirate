using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CostumeDictionary", menuName = "Costume/CostumeDictionary")]
public class CostumeDictionary : ScriptableObject
{
    public List<Costume> costumes;

    private void OnValidate()
    {
        for(int i = 0; i < costumes.Count; i++)
        {
            costumes[i].itemID = i;
        }
    }
}
