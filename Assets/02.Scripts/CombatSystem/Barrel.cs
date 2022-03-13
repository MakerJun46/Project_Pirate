using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class Barrel : MonoBehaviourPunCallbacks, IPunInstantiateMagicCallback
{
    public Vector3 gravity;
    Rigidbody rb;
    private bool hittedFloor;

    bool canAttack;

    public float power=100;
    [SerializeField] LayerMask floorLayer;

    [SerializeField] GameObject ExplodePrefab;

    bool destroyed = false;
    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        rb = GetComponent<Rigidbody>();
        Invoke("Test", 1f);

        info.Sender.TagObject = this.gameObject;
        object[] sendedData = GetComponent<PhotonView>().InstantiationData;
        GetComponent<Rigidbody>().velocity = (Vector3)sendedData[1];
        gravity = Vector3.up * (float)sendedData[2];

        //AttackPS.Play(true);
    }

    private void Test()
    {
        canAttack = true;
    }
    private void Update()
    {
        RaycastHit hit;
        if(GetComponent<Rigidbody>().velocity.y<0 && Physics.Raycast(this.transform.position,Vector3.down, out hit, 1, floorLayer)){
            hittedFloor = true;
            GetComponent<Rigidbody>().velocity = Vector3.zero;
        }
    }
    private void FixedUpdate()
    {
        if (!hittedFloor)
            rb.AddForce(gravity);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && canAttack && GetComponent<PhotonView>().IsMine && destroyed==false)
        {
            if (other.transform.GetComponent<Player_Combat_Ship>())
            {
                other.transform.GetComponent<PhotonView>().RPC("Attacked", RpcTarget.AllBuffered
                    , new object[] {30.0f,(other.transform.position-this.transform.position).normalized*power}
                );
            }
            GetComponent<PhotonView>().RPC("DestroyObjRPC", RpcTarget.AllBuffered, 1.5f);
        }
    }

    [PunRPC]
    public void DestroyObjRPC(float _time)
    {
        destroyed = true;
        StartCoroutine("DestroyObjCoroutine",_time);
    }
    IEnumerator DestroyObjCoroutine(float _time)
    {
        Instantiate(ExplodePrefab, this.transform.position, Quaternion.identity);
        yield return new WaitForSeconds(_time);
        Destroy(this.gameObject);
    }
}
