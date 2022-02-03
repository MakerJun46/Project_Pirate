using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Shark : MonoBehaviourPunCallbacks, IPunInstantiateMagicCallback
{
    public Player_Controller_Ship myShip;
    public Player_Controller_Ship enemyShip;

    bool focused;
    [SerializeField] float dragForce = 5;
    [SerializeField] float waitingTime = 10f;
    [SerializeField] float sharkRoundSpeed = 3f;

    [SerializeField] GameObject WaitingSharkObj;
    GameObject shark;

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        info.Sender.TagObject = this.gameObject;
        object[] sendedData = GetComponent<PhotonView>().InstantiationData;
        GetComponent<Rigidbody>().velocity = ((Vector3)sendedData[1] - this.transform.position).normalized * (float)sendedData[2];

        foreach (Player_Controller_Ship ship in FindObjectsOfType<Player_Controller_Ship>())
        {
            if (ship.GetComponent<PhotonView>().Owner.ActorNumber == (int)sendedData[0])
            {
                myShip = ship;
                break;
            }
        }
        //AttackPS.Play(true);
    }

    void Update()
    {
        waitingTime -= Time.deltaTime;
        if (enemyShip && photonView.IsMine)
        {
            if (waitingTime <= 0)
            {
                if (focused == false)
                {
                    WaitingSharkObj.SetActive(false);
                    shark = Instantiate(Resources.Load("Shark") as GameObject, this.transform.position, Quaternion.identity);
                    focused = true;
                }
                shark.transform.position += Vector3.up * Time.deltaTime*4f;
            }
            else
            {
                transform.position = enemyShip.transform.position;
                transform.rotation = Quaternion.identity;
                GetComponent<Rigidbody>().velocity = Vector3.zero;

                WaitingSharkObj.SetActive(true);
                this.transform.Rotate(Vector3.up, Time.time*-360f);
                print("123");
            }
        }


        if (waitingTime < -3f)
        {
            Destroy(shark);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (enemyShip == null && photonView.IsMine)
        {
            if (other.CompareTag("Player") &&
                other.transform != myShip.transform)
            {
                enemyShip = other.GetComponent<Player_Controller_Ship>();
                transform.GetChild(0).gameObject.SetActive(false);
            }
        }
    }
}
