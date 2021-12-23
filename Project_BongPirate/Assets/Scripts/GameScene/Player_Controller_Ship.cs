using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using System;

public class Player_Controller_Ship : MonoBehaviourPunCallbacks
{
    public float MoveSpeed;
    public float MaxSpeed;
    public float RotateSpeed;
    public float MoveSpeedTmp;
    public bool goOrStop;
    public bool is_Turn_Left;
    public bool is_Turn_Right;

    public float trust = 10000;
    public float turningSpeed;

    public float motorFoamMultiplier;
    public float moterFoamBase;
    public float frontFoamMultiplier;

    public Rigidbody RB;
    public SpriteRenderer SR;
    public PhotonView PV;
    public Text NickNameText;
    public Image HealthImage;
    public GameManager GM;
    public GameObject anchage_UI;


    Vector3 curPos;
    Vector3 lookDirection;

    ParticleSystem.EmissionModule motor, front;

    private void Awake()
    {
        NickNameText.text = PV.IsMine ? PhotonNetwork.NickName : PV.Owner.NickName;
        NickNameText.color = PV.IsMine ? Color.green : Color.red;
    }

    private void Start()
    {
        GM = GameObject.Find("GameManager").GetComponent<GameManager>();
        
        GM.MyShip = this.gameObject;
        MoveSpeedTmp = MoveSpeed;
        turningSpeed = 0.2f;
        MaxSpeed = 30f;

        anchage_UI = GameObject.Find("UI_Canvas").transform.Find("Island_Landing_UI_Panel").gameObject;

        motor = transform.GetChild(3).GetComponent<ParticleSystem>().emission;
        front = transform.GetChild(4).GetComponent<ParticleSystem>().emission;

        goOrStop = false;
    }

    /// <summary>
    /// 플레이어 조작 감지
    /// </summary>

    private void FixedUpdate()
    {
        if (Input.GetAxis("Horizontal") < -0.2f || Input.GetAxis("Horizontal") > 0.2f)
        {
            Debug.Log("horizontal");
            transform.rotation = Quaternion.EulerRotation(0, transform.rotation.ToEulerAngles().y + Input.GetAxis("Horizontal") * turningSpeed * Time.fixedDeltaTime, 0);
        }
        if (Input.GetAxis("Vertical") > 0.2f)
        {
            Debug.Log("Vertical");
            gameObject.GetComponent<Rigidbody>().AddRelativeForce(Vector3.forward * trust * Time.deltaTime);
        }

        motor.rate = motorFoamMultiplier * Input.GetAxis("Vertical") + moterFoamBase;
        front.rate = frontFoamMultiplier * GetComponent<Rigidbody>().velocity.magnitude;

    }

    private void Update()
    {

        if (PV.IsMine)
        {
            /*
            if(Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.W) ||
                Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.A) ||
                Input.GetKey(KeyCode.UpArrow) ||    Input.GetKey(KeyCode.D) ||
                Input.GetKey(KeyCode.DownArrow) ||  Input.GetKey(KeyCode.S))
            {
                float xAxis = Input.GetAxisRaw("Horizontal");
                float zAxis = Input.GetAxisRaw("Vertical");
                lookDirection = xAxis * Vector3.right + zAxis * Vector3.forward;

                this.transform.rotation = Quaternion.Lerp(this.transform.rotation, Quaternion.LookRotation(lookDirection), Time.deltaTime * RotateSpeed);
                //this.transform.Translate(Vector3.forward * MoveSpeed * Time.deltaTime);

                Vector3 movedirection = new Vector3(xAxis, 0, zAxis);
                gameObject.GetComponent<Rigidbody>().velocity = movedirection * MoveSpeed;

            }
            */

            if (Input.GetKeyDown(KeyCode.Space))
            {
                GameObject bullet = PhotonNetwork.Instantiate("CannonBall", this.transform.position, Quaternion.identity);
            }

            if(goOrStop)
            {
                gameObject.GetComponent<Rigidbody>().AddRelativeForce(Vector3.forward * trust * Time.deltaTime);
            }
            if (is_Turn_Left)
            {
                Turn_Left();
            }
            if (is_Turn_Right)
            {
                Turn_Right();
            }
        }
        if(this.gameObject.GetComponent<Rigidbody>().velocity.magnitude > MaxSpeed )
        {
            gameObject.GetComponent<Rigidbody>().AddRelativeForce(Vector3.back * trust * Time.deltaTime);
        }
    }


    public void Ship_MoveSpeed_Reset()
    {
        MoveSpeed = MoveSpeedTmp;
    }


    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag != "Sea")
        {
            Debug.Log("enter");
            MoveSpeed = 1; // 콜라이더 통과 버그 방지를 위함
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "anchoragePoint")
        {
            Debug.Log("On anchoragePoint");
            if (Input.GetKeyDown(KeyCode.Q))
            {
                anchage_UI.SetActive(true);
                MoveSpeed = 0;
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if(collision.gameObject.tag != "Sea")
        {
            Debug.Log("exit");
            MoveSpeed = MoveSpeedTmp;
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
}
