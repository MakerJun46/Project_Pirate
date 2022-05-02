using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class Player_Rank : MonoBehaviourPunCallbacks, IPunInstantiateMagicCallback
{
    [SerializeField] Text playerNameText;
    [SerializeField] Text playerScoreText;

    [SerializeField] private List<GameObject> shipObjects;
    public GameObject myShipObjects;

    int rank = -1;

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        // sendData | 0: rank
        info.Sender.TagObject = this.gameObject;
        object[] sendedData = GetComponent<PhotonView>().InstantiationData;

        rank = (int)sendedData[0];
        InitializePlyerRank();

        playerNameText.text = GetComponent<PhotonView>().Owner.NickName;
        playerScoreText.text=  RoomData.GetInstance().FinalScores[GetComponent<PhotonView>().OwnerActorNr].ToString();
    }

    public void InitializePlyerRank()
    {
        int upgradeIndex = 0;
        if (rank <= 0)
            upgradeIndex = 2;
        else if (rank == 1)
            upgradeIndex = 1;
        else
            upgradeIndex = 0;

        for (int i = 0; i < shipObjects.Count; i++)
            shipObjects[i].gameObject.SetActive(false);
        myShipObjects = shipObjects[upgradeIndex];
        myShipObjects.SetActive(true);

        //if(GetComponent<PhotonView>().IsMine)
        //    FindObjectOfType<CustomizeManager>().EquipCostume(GetComponent<PhotonView>().ViewID);
    }
}
