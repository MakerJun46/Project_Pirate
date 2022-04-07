using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ScoreTarget : MonoBehaviourPunCallbacks
{
    [SerializeField] float score = 1;
    [PunRPC]
    public void Attacked(object[] param)
    {
        if (param.Length > 2)
        {
            if(PhotonNetwork.IsMasterClient)
                RoomData.GetInstance().SetCurrScore(PhotonView.Find((int)param[2]).OwnerActorNr, score);
            Destroy(this.gameObject);
        }
    }
}
