using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class GhostShip : MonoBehaviourPunCallbacks, IPunObservable
{
    GameManager GM;
    Vector3 targetVel;
    float speed;
    Rigidbody rb;
    PhotonView pv;

    [SerializeField] float exitDistance=30f;

    private void Start()
    {
        GM = GetComponent<GameManager>();
        InitializeGhostShip();
        if (pv.IsMine)
        {
            speed = Random.Range(5, 10f);

            Vector2 spawnPos = Random.insideUnitCircle.normalized * (exitDistance - 1);
            this.transform.position = new Vector3(spawnPos.x, transform.position.y, spawnPos.y);

            Player_Combat_Ship[] ships = FindObjectsOfType<Player_Combat_Ship>();
            if (ships.Length > 0)
            {
                int randomIndex = Random.Range(0, ships.Length);
                Vector3 dist = (ships[randomIndex].transform.position - this.transform.position);
                dist.y = 0;
                GetComponent<Rigidbody>().velocity = dist.normalized * speed;
                this.transform.LookAt(this.transform.position + dist);
            }
        }
    }
    public void InitializeGhostShip()
    {
        rb = GetComponent<Rigidbody>();
        pv = GetComponent<PhotonView>();
    }

    private void Update()
    {
        if(GM==null)
            GM = FindObjectOfType<GameManager>();

        if (GM.GameStarted)
        {
            if (pv.IsMine)
            {
                if (this.transform.position.magnitude >= exitDistance)
                {
                    Vector2 spawnPos = Random.insideUnitCircle.normalized * (exitDistance - 1);
                    this.transform.position = new Vector3(spawnPos.x, transform.position.y, spawnPos.y);

                    Player_Combat_Ship[] ships = FindObjectsOfType<Player_Combat_Ship>();
                    if (ships.Length > 0)
                    {
                        int randomIndex = Random.Range(0, ships.Length);

                        Vector3 dist = (ships[randomIndex].transform.position - this.transform.position);
                        dist.y = 0;
                        GetComponent<Rigidbody>().velocity = dist.normalized * speed;
                        this.transform.LookAt(this.transform.position + dist);
                    }
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (GM&&GM.GameStarted)
        {
            if (other.CompareTag("Player"))
            {
                FindObjectOfType<GhostShipGameManager>().GetComponent<Photon.Pun.PhotonView>().RPC("Infection", RpcTarget.AllBuffered, other.GetComponent<PhotonView>().ViewID);
            }
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
    }

}
