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
    private Player_Combat_Ship combatComponent;
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
    float shipRot;

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

    public ParticleSystem FrontFoam;

    public TextMeshProUGUI Count_Text;
    private void Awake()
    {
        RB = GetComponent<Rigidbody>();
        PV = GetComponent<PhotonView>();
        combatComponent = GetComponent<Player_Combat_Ship>();

        Count_Text = transform.Find("Canvas").transform.Find("Count_Text").GetComponent<TextMeshProUGUI>();

        NickNameText.text = PV.IsMine ? PhotonNetwork.NickName : PV.Owner.NickName;
        NickNameText.color = PV.IsMine ? Color.green : Color.red;

        MyShip_Canvas = transform.Find("Canvas").gameObject;

        goOrStop = false;
        is_Turn_Left = false;
        is_Turn_Right = false;
        is_Landing = false;

        FrontFoam.Stop();
    }
    private void Start()
    {
        if (GetComponent<Photon.Pun.PhotonView>().IsMine)
        {
            Player_Controller_Ship[] currentShips = FindObjectsOfType<Player_Controller_Ship>();
            for (int i = 0; i < currentShips.Length; i++)
            {
                if (currentShips[i].gameObject == this.gameObject)
                {
                    continue;
                }

                if (currentShips[i].GetComponent<PhotonView>().IsMine)
                {
                    PhotonNetwork.Destroy(currentShips[i].GetComponent<PhotonView>());
                    if (currentShips[i] != null)
                    {
                        Destroy(currentShips[i].gameObject);
                    }
                }
            }
        }

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
        CharacterCustomize[] customizes = GetComponentsInChildren<CharacterCustomize>();
        
        for(int i = 0; i < customizes.Length; i++)
        {
            customizes[i].EquipCostume(typeIndex, index);
        }
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
        if (combatComponent.health > 0)
        {
            GameObject myShipObjects = combatComponent.myShipObjects;
            int myupgradeIndex = combatComponent.upgradeIndex;

            if (photonView.IsMine)
            {
                if (is_Turn_Left)
                    steeringRot = Mathf.Lerp(steeringRot, myupgradeIndex == 0 ? 5 : 18, Time.deltaTime);
                else if (is_Turn_Right)
                    steeringRot = Mathf.Lerp(steeringRot, -(myupgradeIndex == 0 ? 5 : 18), Time.deltaTime);
                else
                    steeringRot = Mathf.Lerp(steeringRot, 0, Time.deltaTime);
                shipRot = this.transform.rotation.eulerAngles.y;
            }
            else
            {
                this.transform.rotation = Quaternion.Euler(Vector3.up * shipRot);
            }
            if(myShipObjects)
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
                    combatComponent.myShipObjects.GetComponent<MotionTrail>().StartMotionTrail();
                }
            }
            else
            {
                if (isBoostingSynced == true)
                {
                    isBoostingSynced = false;
                    BoosterEffect.SetActive(false);
                    combatComponent.myShipObjects.GetComponent<MotionTrail>().EndMotionTrail();
                }
            }
        }
    }
    private void FixedUpdate()
    {
        if (combatComponent.health > 0)
        {
            Move();
            GetInput();
        }
        else
        {
            RB.constraints = RigidbodyConstraints.None;
            GetComponent<CapsuleCollider>().isTrigger = true;
            //this.transform.Translate(Vector3.down*5f*Time.deltaTime);
            this.transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(Vector3.right * -60f),Time.deltaTime);
        }
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

            var FF = FrontFoam.main;
            FF.startSpeed = RB.velocity.magnitude;

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

    Coroutine ShipBoosterCoroutine;
    public void startBooster()
    {
        if (goOrStop == false)
            GoOrStop_Button();
        if(ShipBoosterCoroutine!=null)
            StopCoroutine(ShipBoosterCoroutine);
        ShipBoosterCoroutine= StartCoroutine(Ship_Booster(3.0f, 15f, 1f));
    }

    public IEnumerator Ship_Booster(float sec, float addMoveSpeed, float addTrunSpeed)
    {
        isBoosting = true;

        MoveSpeed += addMoveSpeed;
        turningSpeed += addTrunSpeed;

        yield return new WaitForSecondsRealtime(sec);

        MoveSpeed -= addMoveSpeed;
        turningSpeed -= addTrunSpeed;

        combatComponent.myShipObjects.GetComponent<MotionTrail>().EndMotionTrail();

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
        GameManager.GetInstance().ActiveGoOrStopBtn();

        if(FrontFoam.isPlaying)
        {
            FrontFoam.Stop();
        }
        else
        {
            FrontFoam.Play();
        }
    }


    public void Ship_Stop()
    {
        RB.velocity = Vector3.zero;
        goOrStop = false;

        FrontFoam.Stop();
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

                int score = other.GetComponent<Treasure>().Score;

                Treasure_GameManager.instance.Player_TreasureCount_Value += score;
                Treasure_GameManager.instance.Update_TreasureCount(photonView.ViewID, score);
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
            stream.SendNext(shipRot);
            stream.SendNext(isBoosting);
        }
        else
        {
            currVel = (Vector3)stream.ReceiveNext();
            steeringRot = (float)stream.ReceiveNext();
            shipRot = (float)stream.ReceiveNext();
            isBoosting = (bool)stream.ReceiveNext();
        }
    }
}
