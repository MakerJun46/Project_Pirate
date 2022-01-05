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
    public float MoveSpeedTmp;
    public bool goOrStop;
    public bool is_Turn_Left;
    public bool is_Turn_Right;

    public float trust = 10000;
    public float turningSpeed;

    public float motorFoamMultiplier;
    public float moterFoamBase;
    public float frontFoamMultiplier;

    private Rigidbody RB;
    private PhotonView PV;
    public Text NickNameText;
    private GameObject anchage_UI;


    Vector3 curPos;
    Vector3 lookDirection;

    ParticleSystem.EmissionModule motor, front;

    private void Start()
    {
        RB = GetComponent<Rigidbody>();

        PV = GetComponent<PhotonView>();
        NickNameText.text = PV.IsMine ? PhotonNetwork.NickName : PV.Owner.NickName;
        NickNameText.color = PV.IsMine ? Color.green : Color.red;

        MoveSpeedTmp = MoveSpeed;
        MaxSpeed = 30f;

        anchage_UI = GameObject.Find("UI_Canvas").transform.Find("Island_Landing_UI_Panel").gameObject;

        goOrStop = false;

        //motor = transform.GetChild(3).GetComponent<ParticleSystem>().emission;
        //front = transform.GetChild(4).GetComponent<ParticleSystem>().emission;
        GameManager.GetIstance().AllShip.Add(this);
    }

    /// <summary>
    /// 플레이어 조작 감지
    /// </summary>

    private void FixedUpdate()
    {
        if (PV.IsMine)
        {
            /*
            if (Input.GetAxis("Horizontal") < -0.2f || Input.GetAxis("Horizontal") > 0.2f)
            {
                Debug.Log("horizontal");
                transform.rotation = Quaternion.EulerRotation(0, transform.rotation.ToEulerAngles().y + Input.GetAxis("Horizontal") * turningSpeed * Time.fixedDeltaTime, 0);
            }
            if (Input.GetAxis("Vertical") > 0.2f)
            {
                Debug.Log("Vertical");
                RB.AddRelativeForce(Vector3.forward * trust * Time.deltaTime);
            }
            */
            if (goOrStop)
            {
                //gameObject.GetComponent<Rigidbody>().AddRelativeForce(Vector3.forward * trust * Time.deltaTime);
                RB.velocity = Vector3.Lerp(RB.velocity, this.transform.forward * trust, Time.deltaTime);
                //RB.AddForce(this.transform.forward * trust);
            }
            else
            {
                RB.velocity = Vector3.Lerp(RB.velocity, Vector3.zero, Time.deltaTime);
            }
            if (is_Turn_Left)
                Turn_Left();
            if (is_Turn_Right)
                Turn_Right();
        }

        // 에러떠서 임시로 주석처리
        //motor.rate = motorFoamMultiplier * Input.GetAxis("Vertical") + moterFoamBase;
        //front.rate = frontFoamMultiplier * GetComponent<Rigidbody>().velocity.magnitude;

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


        }
        if(RB.velocity.magnitude > MaxSpeed )
        {
            //gameObject.GetComponent<Rigidbody>().AddRelativeForce(Vector3.back * trust * Time.deltaTime);
        }
    }


    public void Ship_MoveSpeed_Reset()
    {
        MoveSpeed = MoveSpeedTmp;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("anchoragePoint"))
        {
            Debug.Log("enter");
            MoveSpeed = 1; // 콜라이더 통과 버그 방지를 위함
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("anchoragePoint"))
        {
            Debug.Log("On anchoragePoint");
            if (Input.GetKeyDown(KeyCode.Q))
            {
                anchage_UI.SetActive(true);
                MoveSpeed = 0;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("anchoragePoint"))
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
