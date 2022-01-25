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
        //AttackPS.Play(true);
    }

    void Update()
    {
        if(myShip && enemyShip)
        {
            Vector3 distance = (enemyShip.transform.position - myShip.transform.position);
            myShip.additionalForce = distance.normalized * dragForce;
            enemyShip.additionalForce = -distance.normalized * dragForce;
        }
        lifeTime -= Time.deltaTime;
        if (lifeTime < 0f)
            Destroy(this.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (enemyShip == null)
        {
            if(other.CompareTag("Player") &&
                other.transform != myShip.transform)
            {
                enemyShip = other.GetComponent<Player_Controller_Ship>();
            }
        }
    }

}
