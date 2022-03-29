using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public enum SupplyType
{
    Sail,
    Cannon,
    SpecialCannon
}
public class SupplyItem : MonoBehaviourPunCallbacks, IPunInstantiateMagicCallback
{
    [SerializeField] SupplyType supplyType;
    [SerializeField] int supplyIndex;

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.CompareTag("Player") && Photon.Pun.PhotonNetwork.IsMasterClient)
        {
            other.GetComponent<Player_Combat_Ship>().GetSupply(supplyType, supplyIndex);

            PhotonNetwork.Destroy(this.gameObject);
        }
    }
    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        // sendData | 0: type
        info.Sender.TagObject = this.gameObject;
        object[] sendedData = GetComponent<PhotonView>().InstantiationData;

        supplyType = (SupplyType)sendedData[0];
    }
}
