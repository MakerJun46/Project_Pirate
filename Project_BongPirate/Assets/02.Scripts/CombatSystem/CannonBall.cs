using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
public class CannonBall : MonoBehaviourPunCallbacks,IPunObservable
{
    public Vector3 gravity;
    [SerializeField] float damage=10;
    Rigidbody rb;
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
    void Update()
    {
        transform.GetChild(0).Rotate(new Vector3(rb.velocity.z, 0, -rb.velocity.x) * Time.deltaTime * 60f, Space.World);
    }
    private void FixedUpdate()
    {
        rb.AddForce(gravity);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && photonView.IsMine && other.GetComponent<PhotonView>().IsMine==false)
        {
            other.GetComponent<PhotonView>().RPC("Attacked", RpcTarget.AllBuffered, damage);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            //stream.SendNext(this.transform.position);
        }
        else
        {
            //this.transform.position = (Vector3)stream.ReceiveNext();
        }
    }
}
