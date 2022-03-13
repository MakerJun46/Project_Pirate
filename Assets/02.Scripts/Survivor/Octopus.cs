using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Octopus : SurvivorMonster
{
    Animator anim;
    [SerializeField] Transform IKHandler;
    [SerializeField] Transform IKHint;

    private float attackCooltime = 5f;


    Vector3 hintPos;
    Vector3 handlePos;

    Vector3 TargetPos;

    int index;
    bool attacking = false;

    Vector3 offset;

    public override void ResetEnemy(SurvivorMonster _data)
    {
        base.ResetEnemy(_data);
        GetComponentInChildren<SkinnedMeshRenderer>().materials[1].color = Color.white;
    }

    protected override void Start()
    {
        base.Start();
        anim = GetComponent<Animator>();
        anim.speed = Random.Range(0.8f, 1.2f);
        attackCooltime = Random.Range(1, 5f);
        StartCoroutine("ChangeOffsetCoroutine");
    }
    IEnumerator ChangeOffsetCoroutine()
    {
        yield return new WaitForSeconds(Random.Range(0.5f,2f));
        offset = new Vector3(Random.Range(-1, 1f), Random.Range(0, 1f), Random.Range(-1, 1f)) * 10f;
        StartCoroutine("ChangeOffsetCoroutine");
    }

    protected override void Update()
    {
        base.Update();
        if (GetComponent<PhotonView>().IsMine)
        {
            attackCooltime -= Time.deltaTime;
            if (attackCooltime <= 0)
            {
                if (index >= 0 && !attacking && target && Vector3.Distance(target.position, transform.position) <= viewRadius)
                {
                    TargetPos = target.transform.position;
                    if (Photon.Pun.PhotonNetwork.IsConnected)
                        GetComponent<PhotonView>().RPC("AttackFunc", RpcTarget.AllBuffered);
                    else
                        AttackFunc();
                }

                if (attacking)
                {
                    Collider[] tmpColl = Physics.OverlapSphere(IKHandler.position, attackRadius, targetLayer);
                    for (int i = 0; i < tmpColl.Length; i++)
                    {
                        if (tmpColl[i].transform.GetComponent<PhotonView>())
                            tmpColl[i].transform.GetComponent<PhotonView>().RPC("Attacked", RpcTarget.AllBuffered, new object[] { damage, Vector3.zero, GetComponent<PhotonView>().ViewID });
                    }
                    //IKHandler.transform.position = Vector3.Slerp(IKHandler.transform.position, TargetPos, Time.deltaTime * 5f);
                    //IKHandler.transform.rotation = Quaternion.Euler(Mathf.Lerp(IKHandler.transform.rotation.eulerAngles.x,90, Time.deltaTime * 5f),0,0);
                }
            }
            if (index >= 0 && !attacking && target)
            {
                this.transform.LookAt(new Vector3(target.position.x, 0, target.transform.position.z));
                IKHint.transform.localPosition = -(target.transform.position - this.transform.position);
                //IKHandler.transform.rotation = Quaternion.Euler(Mathf.Lerp(IKHandler.transform.rotation.eulerAngles.x, -90, Time.deltaTime * 5f), 0, 0);
            }
            IKHandler.transform.localPosition = Vector3.Slerp(IKHandler.transform.localPosition, new Vector3(0, 15, -12) + offset, Time.deltaTime * 1f);
        }
        else
        {
            IKHint.transform.localPosition = hintPos;
            IKHandler.transform.localPosition = handlePos;
        }
    }

    [PunRPC]
    public void AttackFunc()
    {
        StartCoroutine("AttackCoroutine");
        anim.SetTrigger("Attack");
    }

    IEnumerator AttackCoroutine()
    {
        attacking = true;
        yield return new WaitForSeconds(4f);
        attackCooltime = Random.Range(7f,9f);
        attacking = false;
    }

    protected override void AttackedFunc(float _stunTime)
    {
        StartCoroutine("AttackedCoroutine", _stunTime);
    }


    IEnumerator AttackedCoroutine(float stunTime)
    {
        attacked = true;
        anim.SetTrigger("Attacked");
        GetComponentInChildren<SkinnedMeshRenderer>().materials[0].color = Color.red;
        yield return new WaitForSeconds(stunTime);
        GetComponentInChildren<SkinnedMeshRenderer>().materials[1].color = Color.white;
        attacked = false;
    }
    public override void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(IKHint.transform.localPosition);
            stream.SendNext(IKHandler.transform.localPosition);
        }
        else
        {
            hintPos = (Vector3)stream.ReceiveNext();
            handlePos = (Vector3)stream.ReceiveNext();
        }
    }
}
