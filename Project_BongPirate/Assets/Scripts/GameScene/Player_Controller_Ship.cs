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

    public Rigidbody RB;
    public SpriteRenderer SR;
    public PhotonView PV;
    public Text NickNameText;
    public Image HealthImage;

    Vector3 curPos;
    Vector3 lookDirection;

    private void Awake()
    {
        NickNameText.text = PV.IsMine ? PhotonNetwork.NickName : PV.Owner.NickName;
        NickNameText.color = PV.IsMine ? Color.green : Color.red;
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
}
