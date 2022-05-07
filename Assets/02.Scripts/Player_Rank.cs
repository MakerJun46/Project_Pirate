using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class Player_Rank : MonoBehaviourPunCallbacks, IPunInstantiateMagicCallback
{
    [SerializeField] Text playerNameText;
    [SerializeField] Text playerScoreText;

    int rank = -1;
    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        // sendData | 0: rank
        info.Sender.TagObject = this.gameObject;
        object[] sendedData = GetComponent<PhotonView>().InstantiationData;

        rank = (int)sendedData[0];

        playerNameText.text = GetComponent<PhotonView>().Owner.NickName;
        playerScoreText.text=  RoomData.GetInstance().FinalScores[GetComponent<PhotonView>().OwnerActorNr].ToString();
    }
    private void Start()
    {
        InitializePlyerRank();
    }

    public void InitializePlyerRank()
    {
        if (photonView.IsMine)
            FindObjectOfType<CustomizeManager>().EquipCostume(photonView.ViewID);
    }
    [PunRPC]
    public void EquipCostume(int typeIndex, int index)
    {
        GetComponentInChildren<CharacterCustomize>().EquipCostume(typeIndex, index);
    }

}
