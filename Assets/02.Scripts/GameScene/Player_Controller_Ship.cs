using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using System;

public class Player_Controller_Ship : MonoBehaviourPunCallbacks, IPunObservable
{
    public int myIndex;
    public string myName;
    public float deadTime;
    public static int characterIndex;

    public int upgradeIndex;

    public float MoveSpeed;
    public float MaxSpeed;
    public float turningSpeed;

    public Vector3 inputVel;
    public Vector3 additionalForce;

    public bool goOrStop;
    public bool is_Turn_Left;
    public bool is_Turn_Right;
    public bool is_Landing;

    public float motorFoamMultiplier;
    public float moterFoamBase;
    public float frontFoamMultiplier;

    private Rigidbody RB;
    private PhotonView PV;
    public Text NickNameText;
    public int Landed_island_ID;

    ParticleSystem.EmissionModule motor, front;

    public Vector3 currVel;

    private void Awake()
    {
        RB = GetComponent<Rigidbody>();
        PV = GetComponent<PhotonView>();

        NickNameText.text = PV.IsMine ? PhotonNetwork.NickName : PV.Owner.NickName;
        NickNameText.color = PV.IsMine ? Color.green : Color.red;

        goOrStop = false;
        is_Turn_Left = false;
        is_Turn_Right = false;
        is_Landing = false;

    }
    private void Start()
    {
        GameManager.GetInstance().AllShip.Add(this);
        Reset_Ship_Status();
    }


    private void FixedUpdate()
    {
        Move();
        GetInput();
    }

    public void GetInput()
    {
    }

    public void Reset_Ship_Status()
    {

        //motor = transform.GetChild(3).GetComponent<ParticleSystem>().emission;
        //front = transform.GetChild(4).GetComponent<ParticleSystem>().emission;
    }
    public void UpgradeShip()
    {
        upgradeIndex++;
        GetComponent<Photon.Pun.PhotonView>().RPC("InitializeCombat", Photon.Pun.RpcTarget.AllBuffered, upgradeIndex);
        Reset_Ship_Status();
    }
    [PunRPC]
    public void InitializePlayer()
    {
        if (PhotonNetwork.IsConnected)
        {
            myIndex = GetComponent<PhotonView>().Owner.ActorNumber - 1;
        }
        else
        {
            myIndex = characterIndex;
        }
        myName = PV.IsMine ? PhotonNetwork.NickName : PV.Owner.NickName;
        characterIndex++;
        deadTime = 0;
        GameManager.GetInstance().RefreshBestPlayer(this.gameObject);
    }

    public void Move()
    {
        if (PV.IsMine && !is_Landing)
        {
            additionalForce = Vector3.Lerp(additionalForce, Vector3.zero, Time.deltaTime);
            if (goOrStop)
            {
                inputVel = Vector3.Lerp(inputVel, this.transform.forward * MoveSpeed, Time.deltaTime);
            }
            else
            {
                inputVel = Vector3.Lerp(inputVel, Vector3.zero, Time.deltaTime);
            }
            RB.velocity = inputVel + additionalForce;


            if (is_Turn_Left)
                Turn_Left();
            if (is_Turn_Right)
                Turn_Right();
        }
        else
        {
            Ship_Stop();
        }

        if (PV.IsMine == false)
        {
            RB.velocity = currVel;
        }

        // 에러떠서 임시로 주석처리 => 배가 지나간 잔상 파티클 부분
        //motor.rate = motorFoamMultiplier * Input.GetAxis("Vertical") + moterFoamBase;
        //front.rate = frontFoamMultiplier * GetComponent<Rigidbody>().velocity.magnitude;
    }

    public void Ship_Stop()
    {
        RB.velocity = Vector3.zero;
        goOrStop = false;

    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("SeaResource"))
        {
            Debug.Log("get Resource on Sea");

            int resourceCode = (int)other.GetComponent<Resource>().type;

            Item_Manager.instance.AddItem(Item_Manager.instance.Resource_item_list[resourceCode]);

            Destroy(other.gameObject);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("anchoragePoint"))
        {
            Debug.Log("On anchoragePoint");
            GameManager.GetInstance().GetComponent<BattleRoyalGameManager>().MyShip_On_Landing_Point = true;
            Landed_island_ID = other.GetComponentInParent<Island_Info>().Island_ID;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("anchoragePoint"))
        {
            GameManager.GetInstance().GetComponent<BattleRoyalGameManager>().MyShip_On_Landing_Point = false;
        }
    }

    public void Turn_Left()
    {
        transform.rotation = Quaternion.EulerRotation(0, transform.rotation.ToEulerAngles().y + -1 * turningSpeed * Time.fixedDeltaTime, 0);
    }

    public void Turn_Right()
    {
        transform.rotation = Quaternion.EulerRotation(0, transform.rotation.ToEulerAngles().y + 1 * turningSpeed * Time.fixedDeltaTime, 0);
    }

    public void GoOrStop_Button()
    {
        goOrStop = !goOrStop;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(GetComponent<Rigidbody>().velocity);
        }
        else
        {
            currVel = (Vector3)stream.ReceiveNext();
        }
    }
}
