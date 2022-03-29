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

    [SerializeField] Transform SnakeTR;
    List<Transform> SnakeBones;
    [SerializeField] ParticleSystem StartPS;
    [SerializeField] ParticleSystem EndPS;
    bool StartPlayed;
    bool EndPlayed;


    protected override void Start()
    {
        base.Start();

        SnakeBones = new List<Transform>();
        SnakeBones.Add(SnakeTR);
        while (SnakeTR.childCount > 0)
        {
            SnakeBones.Add(SnakeTR.GetChild(0));
            SnakeTR = SnakeTR.GetChild(0);
        }

        if (PhotonNetwork.IsConnected==false || GetComponent<PhotonView>().IsMine)
            StartCoroutine("SetTargetPosCoroutine");
        StartPlayed = false;
        EndPlayed = false;
        if ((PhotonNetwork.IsConnected == false || GetComponent<PhotonView>().IsMine) == false)
            cart.enabled = false;
    }

    IEnumerator SetTargetPosCoroutine()
    {
        if (target)
        {
            Vector3 TargetPos = target.transform.position;
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
            path.m_Waypoints[3].position = path.m_Waypoints[2].position + (endPosition-startPosition).normalized*(10)+(Vector3.down * 150);

            path.InvalidateDistanceCache();
            cart.m_Position = 0;
        }
        yield return new WaitForSeconds(10f);
        StartCoroutine("SetTargetPosCoroutine");
    }

    Vector3 currPos;
    Quaternion curRot;

    Collider[] tmpColls;
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

        RaycastHit hitInfo;
        if (Physics.Raycast(SnakeBones[0].transform.position, Vector3.down, out hitInfo, 20, terrainLayer) && StartPlayed==false)
        {
            StartPS.transform.position = hitInfo.point;
            StartPS.Play();
            StartPlayed = true;
        }
        if (Physics.Raycast(SnakeBones[SnakeBones.Count-1].transform.position, Vector3.down, out hitInfo, 20, terrainLayer) && StartPlayed==true)
        {
            StartPlayed = false;
            StartPS.Stop();
        }

        if (Physics.Raycast(SnakeBones[0].transform.position, Vector3.down, out hitInfo, 20, terrainLayer) && EndPlayed==false)
        {
            EndPS.transform.position = hitInfo.point;
            EndPS.Play();
            EndPlayed = true;
        }
        if (Physics.Raycast(SnakeBones[SnakeBones.Count - 1].transform.position, Vector3.down, out hitInfo, 20, terrainLayer) && EndPlayed==true)
        {
            EndPlayed = false;
            EndPS.Stop();
        }

        if (PhotonNetwork.IsConnected == false || PhotonNetwork.IsMasterClient)
        {
            for(int j=0;j< SnakeBones.Count; j++)
            {
                tmpColls = Physics.OverlapSphere(SnakeBones[j].position, attackRadius, targetLayer);
                for (int i = 0; i < tmpColls.Length; i++)
                {
                    tmpColls[i].transform.GetComponent<PhotonView>().RPC("Attacked", RpcTarget.AllBuffered, new object[] { damage, Vector3.zero, GetComponent<PhotonView>().ViewID });
                }
            }
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
