using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Costume", menuName = "Costume/Costume")]
public class Costume : ScriptableObject
{
    public enum CostumeType { 
        Hat,
        Accessory,
        Cloth,
        Skin
    }

    public string itemName;
    public CostumeType costumeType;
    public int itemID;

    public Mesh[] itemMesh;
    public Material[] itemMaterial;
}
