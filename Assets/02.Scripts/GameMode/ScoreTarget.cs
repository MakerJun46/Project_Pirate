using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ScoreTarget : MonoBehaviourPunCallbacks
{
    [SerializeField] float score = 1;
    Animator anim;

    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    [PunRPC]
    public void Attacked(object[] param)
    {
        if (param.Length > 2)
        {
            if(PhotonNetwork.IsMasterClient)
                RoomData.GetInstance().SetCurrScore(PhotonView.Find((int)param[2]).OwnerActorNr, score);
            FloatingTextController.CreateFloatingText("+ "+score.ToString(), this.transform, Color.yellow);
            StartCoroutine("DestroyCoroutine"); 
        }
    }

    IEnumerator DestroyCoroutine()
    {
        anim.SetTrigger("Hit");
        yield return new WaitForSeconds(1.5f);
        Destroy(this.gameObject);
    }
}
