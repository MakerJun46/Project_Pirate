using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Obstacle : MonoBehaviourPun
{
    public void Initialize()
    {
        int param = Random.Range(1, transform.childCount);
        GetComponent<PhotonView>().RPC("SetObstacleMesh",RpcTarget.AllBuffered, param);
    }

    [PunRPC]
    private void SetObstacleMesh(int _params)
    {
        transform.GetChild(_params).gameObject.SetActive(true);
    }
}
