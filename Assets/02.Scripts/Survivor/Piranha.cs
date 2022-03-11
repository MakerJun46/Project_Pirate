using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Piranha : SurvivorMonster
{
    float targetFollowTime = 3;
    Vector3 currVel;
    Vector3 currPos;
    Quaternion currRot;
    public override void ResetEnemy(SurvivorMonster _data)
    {
        base.ResetEnemy(_data);
        GetComponentInChildren<MeshRenderer>().material.color = Color.white;
        this.transform.GetComponent<SphereCollider>().radius = _data.transform.GetComponent<SphereCollider>().radius;
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();
        print("Update");
        if (GetComponent<PhotonView>().IsMine)
        {
            print("etComponent<PhotonView>().IsMine");
            for (int i = 0; i < colls.Length; i++)
            {
                if (Vector3.Distance(colls[i].transform.position, transform.position) <= attackRadius)
                    colls[i].transform.GetComponent<PhotonView>().RPC("Attacked", RpcTarget.AllBuffered, new object[] { damage, Vector3.zero, GetComponent<PhotonView>().ViewID });
            }

            //if (target != null)
            //{
            //    targetFollowTime -= Time.deltaTime;
            //    if (targetFollowTime <= 0)
            //    {
            //        targetFollowTime = 3f;
            //        target = null;
            //    }
            //}


            if (!attacked)
            {
                print("!attacked");
                if (target)
                {
                    print("target");
                    toTargetDir = (target.position - this.transform.position).normalized;
                }
                else
                {
                    toTargetDir = this.transform.forward;
                }
            }
            rb.velocity = toTargetDir * speed;
            this.transform.LookAt(target);
        }
    }
    protected override void AttackedFunc(float _stunTime)
    {
        StartCoroutine("AttackedCoroutine", _stunTime);
    }
    IEnumerator AttackedCoroutine(float stunTime)
    {
        attacked = true;
        GetComponentInChildren<MeshRenderer>().material.color = Color.red;
        yield return new WaitForSeconds(stunTime);
        GetComponentInChildren<MeshRenderer>().material.color = Color.white;
        attacked = false;
    }

    public override void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
        }
        else
        {
        }
    }
}
