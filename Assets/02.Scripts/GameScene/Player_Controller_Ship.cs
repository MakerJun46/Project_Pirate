using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using System;
using TMPro;

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
    public Vector3 currPos;
    public Quaternion currRot;

    public ParticleSystem WinnerEffectPrefab;
    public ParticleSystem LoseEffectPrefab;

    public TextMeshProUGUI Count_Text;
    private void Awake()
    {
        RB = GetComponent<Rigidbody>();
        PV = GetComponent<PhotonView>();

        Count_Text = transform.Find("Canvas").transform.Find("Count_Text").GetComponent<TextMeshProUGUI>();

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

    [PunRPC]
    public void InitializePlayer()
    {
        myIndex = (PhotonNetwork.IsConnected) ? GetComponent<PhotonView>().Owner.ActorNumber : characterIndex;
        myName = PV.IsMine ? PhotonNetwork.NickName : PV.Owner.NickName;
        characterIndex++;
        deadTime = 0;
        GameManager.GetInstance().AddThisPlayerToPlayerList(this.gameObject);
    }

    [PunRPC]
    public void EquipCostume(int typeIndex,int index)
    {
        GetComponentInChildren<CharacterCustomize>().EquipCostume(typeIndex,index);
    }

    public void ActiveWinLoseEffect(bool _isWinner)
    {
        if (_isWinner)
        {
            WinnerEffectPrefab.Play();
        }
        else
        {
            LoseEffectPrefab.Play();
        }
    }

    private void FixedUpdate()
    {
        Move();
        GetInput();
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
            currVel = RB.velocity;

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


    public void GetInput()
    {
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


    public void Ship_Stop()
    {
        RB.velocity = Vector3.zero;
        goOrStop = false;
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

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("SeaResource"))
        {
            Debug.Log("get Resource on Sea");

            int resourceCode = (int)other.GetComponent<Resource>().type;

            Item_Manager.instance.AddItem(Item_Manager.instance.Resource_item_list[resourceCode]);

            Destroy(other.gameObject);
        }
        else if(other.gameObject.CompareTag("Treasure") && other.GetComponent<Treasure>().isPickable)
        {
            if (GetComponent<PhotonView>().IsMine)
            {
                Treasure_GameManager.instance.Update_TreasureCount(GetComponent<PhotonView>().ViewID);
            }

            Destroy(other.gameObject);
        }else if (other.CompareTag("ScoreTarget") && photonView.IsMine)
        {
            other.GetComponent<PhotonView>().RPC("Attacked", RpcTarget.AllBuffered, new object[] { 5, Vector3.zero, GetComponent<PhotonView>().ViewID });
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("anchoragePoint"))
        {
            //Debug.Log("On anchoragePoint");
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
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(RB.velocity);
        }
        else
        {
            currVel = (Vector3)stream.ReceiveNext();
        }
    }
}
