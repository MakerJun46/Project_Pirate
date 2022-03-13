using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Photon.Pun;


public class Snake : SurvivorMonster
{
    [SerializeField] LayerMask terrainLayer;
    [SerializeField] CinemachineSmoothPath path;
    [SerializeField] CinemachineDollyCart cart;

    protected override void Start()
    {
        base.Start();

        if(PhotonNetwork.IsConnected==false || GetComponent<PhotonView>().IsMine)
            StartCoroutine("SetTargetPosCoroutine");
    }

    IEnumerator SetTargetPosCoroutine()
    {
        if (target)
        {
            Vector3 TargetPos = target.transform.position;
            TargetPos.y = Mathf.Max(10, TargetPos.y);
            Vector3 randomRange = Random.insideUnitSphere * 100f;
            randomRange.y = 0;
            Vector3 startPosition = TargetPos + randomRange;
            Vector3 endPosition = TargetPos;

            RaycastHit hitInfo;
            if (Physics.Raycast(startPosition, Vector3.down, out hitInfo, 1000, terrainLayer))
            {
                startPosition = hitInfo.point;
            }
            if (Physics.Raycast(endPosition, Vector3.down, out hitInfo, 1000, terrainLayer))
            {
                endPosition = hitInfo.point;
            }
            path.m_Waypoints[0].position = startPosition + (Vector3.down * 15);
            path.m_Waypoints[2].position = endPosition+ target.GetComponent<Player_Controller_Ship>().currVel*3f;
            Vector3 middlePos = (path.m_Waypoints[0].position + path.m_Waypoints[2].position) / 2f;
            middlePos.y = 0;
            path.m_Waypoints[1].position = middlePos + (Vector3.up * 10);
            path.m_Waypoints[3].position = path.m_Waypoints[2].position + (endPosition-startPosition).normalized*(10)+(Vector3.down * 50);

            path.InvalidateDistanceCache();
            cart.m_Position = 0;
        }
        yield return new WaitForSeconds(10f);
        StartCoroutine("SetTargetPosCoroutine");
    }

    Vector3 currPos;
    Quaternion curRot;
    protected override void Update()
    {
        base.Update();
        if (PhotonNetwork.IsConnected == false || GetComponent<PhotonView>().IsMine)
        {
            cart.m_Position += Time.deltaTime * cart.m_Speed;
        }
        else
        {
            cart.transform.position = currPos;
            cart.transform.rotation = curRot;
        }
    }
    public override void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(cart.transform.position);
            stream.SendNext(cart.transform.rotation);
        }
        else
        {
            currPos = (Vector3)stream.ReceiveNext();
            curRot = (Quaternion)stream.ReceiveNext();
        }
    }
}
