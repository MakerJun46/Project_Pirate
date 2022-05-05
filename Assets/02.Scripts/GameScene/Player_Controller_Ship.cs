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
    [SerializeField] float boosterCoolTime=5f;

    public Vector3 inputVel;
    public Vector3 additionalForce;

    public bool goOrStop;
    public bool is_Turn_Left;
    public bool is_Turn_Right;
    public bool is_Landing;
    public bool isBoosting;
    private bool isBoostingSynced;
    float steeringRot;

    public float motorFoamMultiplier;
    public float moterFoamBase;
    public float frontFoamMultiplier;

    private Rigidbody RB;
    private PhotonView PV;
    public Text NickNameText;
    public GameObject MyShip_Canvas;

    ParticleSystem.EmissionModule motor, front;

    public Vector3 currVel;
    public Vector3 currPos;
    public Quaternion currRot;

    public ParticleSystem WinnerEffectPrefab;
    public ParticleSystem LoseEffectPrefab;
    public GameObject BoosterEffect;

    public TextMeshProUGUI Count_Text;
    private void Awake()
    {
        RB = GetComponent<Rigidbody>();
        PV = GetComponent<PhotonView>();

        Count_Text = transform.Find("Canvas").transform.Find("Count_Text").GetComponent<TextMeshProUGUI>();

        NickNameText.text = PV.IsMine ? PhotonNetwork.NickName : PV.Owner.NickName;
        NickNameText.color = PV.IsMine ? Color.green : Color.red;

        MyShip_Canvas = transform.Find("Canvas").gameObject;

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
    private void Update()
    {
        GameObject myShipObjects = GetComponent<Player_Combat_Ship>().myShipObjects;
        if (photonView.IsMine)
        {
            if (is_Turn_Left)
                steeringRot = Mathf.Lerp(steeringRot, 24, Time.deltaTime);
            else if (is_Turn_Right)
                steeringRot = Mathf.Lerp(steeringRot, -24, Time.deltaTime);
            else
                steeringRot = Mathf.Lerp(steeringRot, 0, Time.deltaTime);
        }
        myShipObjects.transform.localEulerAngles = new Vector3
            (myShipObjects.transform.localEulerAngles.x,
            myShipObjects.transform.localEulerAngles.y,
            steeringRot
            );

        if (isBoosting)
        {
            if (isBoostingSynced == false)
            {
                isBoostingSynced = true;
                BoosterEffect.SetActive(true);
                GetComponent<Player_Combat_Ship>().myShipObjects.GetComponent<MotionTrail>().StartMotionTrail();
            }
        }
        else
        {
            if (isBoostingSynced == true)
            {
                isBoostingSynced = false;
                BoosterEffect.SetActive(false);
                GetComponent<Player_Combat_Ship>().myShipObjects.GetComponent<MotionTrail>().EndMotionTrail();
            }
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

    public void startBooster()
    {
        if (!isBoosting)
        {
            if(goOrStop==false)
                GoOrStop_Button();
            StartCoroutine(Ship_Booster(3.0f, 15f, 1f));
        }
    }

    public IEnumerator Ship_Booster(float sec, float addMoveSpeed, float addTrunSpeed)
    {
        isBoosting = true;

        MoveSpeed += addMoveSpeed;
        turningSpeed += addTrunSpeed;

        yield return new WaitForSecondsRealtime(sec);

        MoveSpeed -= addMoveSpeed;
        turningSpeed -= addTrunSpeed;

        GetComponent<Player_Combat_Ship>().myShipObjects.GetComponent<MotionTrail>().EndMotionTrail();

        yield return new WaitForSecondsRealtime(boosterCoolTime); // cooltime
        isBoosting = false;
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

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Terrain"))
        {
            additionalForce += collision.GetContact(0).normal * 30f;
        }else if (collision.gameObject.CompareTag("Treasure") && collision.gameObject.GetComponent<Treasure>().isPickable)
        {
            if (GetComponent<PhotonView>().IsMine)
            {
                Debug.LogError("GetTreasure");
                Debug.LogError(Treasure_GameManager.instance.Player_TreasureCount_Value);
                Treasure_GameManager.instance.Player_TreasureCount_Value++;
                Treasure_GameManager.instance.Update_TreasureCount(photonView.ViewID);
            }
            Destroy(collision.gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Treasure") && other.GetComponent<Treasure>().isPickable)
        {
            if (GetComponent<PhotonView>().IsMine)
            {
                Debug.LogError("GetTreasure");
                Debug.LogError(Treasure_GameManager.instance.Player_TreasureCount_Value);
                Treasure_GameManager.instance.Player_TreasureCount_Value++;
                Treasure_GameManager.instance.Update_TreasureCount(photonView.ViewID);
            }

            Destroy(other.gameObject);
        }else if (other.CompareTag("ScoreTarget") && photonView.IsMine)
        {
            other.GetComponent<PhotonView>().RPC("Attacked", RpcTarget.AllBuffered, new object[] { 5, Vector3.zero, GetComponent<PhotonView>().ViewID });
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(RB.velocity);
            stream.SendNext(steeringRot);
            stream.SendNext(isBoosting);
        }
        else
        {
            currVel = (Vector3)stream.ReceiveNext();
            steeringRot = (float)stream.ReceiveNext();
            isBoosting = (bool)stream.ReceiveNext();
        }
    }
}
