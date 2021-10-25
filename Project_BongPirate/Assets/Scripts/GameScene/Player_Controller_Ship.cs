using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class Player_Controller_Ship : MonoBehaviourPunCallbacks
{
    public float MoveSpeed;
    public float RotateSpeed;
    public float MoveSpeedTmp;

    public Rigidbody RB;
    public SpriteRenderer SR;
    public PhotonView PV;
    public Text NickNameText;
    public Image HealthImage;
    public GameManager GM;
    public GameObject anchage_UI;

    Vector3 curPos;
    Vector3 lookDirection;

    private void Awake()
    {
        NickNameText.text = PV.IsMine ? PhotonNetwork.NickName : PV.Owner.NickName;
        NickNameText.color = PV.IsMine ? Color.green : Color.red;
    }

    private void Start()
    {
        MoveSpeedTmp = MoveSpeed;
        anchage_UI = GameObject.Find("UI_Canvas").transform.Find("Island_Landing_UI_Panel").gameObject;
    }

    /// <summary>
    /// 플레이어 조작 감지
    /// </summary>
    private void Update()
    {

        if (PV.IsMine)
        {
            if(Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.W) ||
                Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.A) ||
                Input.GetKey(KeyCode.UpArrow) ||    Input.GetKey(KeyCode.D) ||
                Input.GetKey(KeyCode.DownArrow) ||  Input.GetKey(KeyCode.S))
            {
                float xAxis = Input.GetAxisRaw("Vertical");
                float zAxis = Input.GetAxisRaw("Horizontal");
                lookDirection = xAxis * Vector3.forward + zAxis * Vector3.right;

                this.transform.rotation = Quaternion.Lerp(this.transform.rotation, Quaternion.LookRotation(lookDirection), Time.deltaTime * RotateSpeed);
                this.transform.Translate(Vector3.forward * MoveSpeed * Time.deltaTime);
            }
            
            if(Input.GetKeyDown(KeyCode.Space))
            {
                GameObject bullet = PhotonNetwork.Instantiate("CannonBall", this.transform.position, Quaternion.identity);
            }

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
}
