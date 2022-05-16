using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
public class Hook : MonoBehaviourPunCallbacks, IPunInstantiateMagicCallback
{
    public Player_Controller_Ship myShip;
    public Player_Controller_Ship enemyShip;

    [SerializeField] float dragForce = 1;
    [SerializeField] float lifeTime = 5f;

    [SerializeField] GameObject ChainEffect;
    [SerializeField] LineRenderer ChainLine;

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        // sendData | 0: actor number 1:targetPos 2: shootVelocity
        info.Sender.TagObject = this.gameObject;
        object[] sendedData = GetComponent<PhotonView>().InstantiationData;
        GetComponent<Rigidbody>().velocity = ((Vector3)sendedData[1] - this.transform.position).normalized * (float)sendedData[2];

        foreach(Player_Controller_Ship ship in FindObjectsOfType<Player_Controller_Ship>())
        {
            if(ship.GetComponent<PhotonView>().Owner.ActorNumber == (int)sendedData[0])
            {
                myShip = ship;
                break;
            }
        }
    }

    void Update()
    {
        lifeTime -= Time.deltaTime;
        if (lifeTime <= 0f || myShip == null)
            Destroy(this.gameObject);
        if (enemyShip)
            this.transform.position = enemyShip.transform.position;
        if (myShip && enemyShip)
        {
            ChainEffect.transform.position = enemyShip.transform.position+Vector3.up*2;
            ChainLine.SetPosition(0, myShip.transform.position-this.transform.position + Vector3.up * 2f);
            ChainLine.SetPosition(1, enemyShip.transform.position - this.transform.position+Vector3.up*2f);

            Vector3 distance = (enemyShip.transform.position - myShip.transform.position);
            myShip.additionalForce += distance.normalized * dragForce;
            enemyShip.additionalForce += -distance.normalized * dragForce;

            if (distance.magnitude <= 10f)
            {
                if(this.gameObject)
                    PhotonNetwork.Destroy(this.gameObject);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (enemyShip == null)
        {
            if(other.CompareTag("Player") &&
                other.transform != myShip.transform)
            {
                enemyShip = other.GetComponent<Player_Controller_Ship>();
                ChainEffect.GetComponentInChildren<ParticleSystem>().Play();
            }
        }
    }

}
