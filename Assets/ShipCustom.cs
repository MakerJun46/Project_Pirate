using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipCustom : MonoBehaviour
{
    [SerializeField] List<MeshRenderer> shipMRs;

    [SerializeField] List<Material> originalMaterials;
    [SerializeField] List<Material> GhostMaterials;
    public void SetToOriginal()
    {
        for (int i = 0; i < shipMRs.Count; i++)
        {
            shipMRs[i].material = originalMaterials[i];
        }
    }
    public void SetToGhost()
    {
        for (int i = 0; i < shipMRs.Count; i++)
        {
            shipMRs[i].material = GhostMaterials[i];
        }
    }
}
