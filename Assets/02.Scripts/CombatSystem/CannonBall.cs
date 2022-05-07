using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
public class CannonBall : MonoBehaviourPunCallbacks,IPunObservable,IPunInstantiateMagicCallback
{
    public Vector3 gravity;
    float damage=10;
    Rigidbody rb;

    [SerializeField] List<ParticleSystem> AttackParticles;
    [SerializeField] ParticleSystem DropParticle;


    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        // sendData | 0: damage 1: scale
        info.Sender.TagObject = this.gameObject;
        object[] sendedData = GetComponent<PhotonView>().InstantiationData;

        damage = (float)sendedData[0];
        this.transform.localScale = Vector3.one * (float)sendedData[1];
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
    protected virtual void Update()
    {
        transform.GetChild(0).Rotate(new Vector3(rb.velocity.z, 0, -rb.velocity.x) * Time.deltaTime * 60f, Space.World);

        if(this.transform.position.y < -30)
        {
            Destroy(this.gameObject);
        }

    }

    private void FixedUpdate()
    {
        rb.AddForce(gravity);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (photonView.IsMine)
        {
            bool canAttacked = false;
            if (other.CompareTag("Player") && other.GetComponent<PhotonView>().IsMine == false)
                canAttacked = true;
            else if (other.CompareTag("Enemy"))
                canAttacked = true;
            else if (other.CompareTag("ScoreTarget"))
                canAttacked = true;

            if (canAttacked)
            {
                other.GetComponent<PhotonView>().RPC("Attacked", RpcTarget.AllBuffered, new object[] { damage, Vector3.zero, GetComponent<PhotonView>().ViewID });
            }
        }
    }


    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
        }
        else
        {
        }
    }
}
