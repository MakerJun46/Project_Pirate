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
            }
            if (index >= 0 && !attacking && attackCooltime <= 4 && target)
            {
                this.transform.LookAt(new Vector3(target.position.x, 0, target.transform.position.z));
            }

            if (attacking)
            {
                RaycastHit[] tmpColl = Physics.SphereCastAll(this.transform.position, 5f, this.transform.forward, viewRadius-2.5f, targetLayer);
                for (int i = 0; i < tmpColl.Length; i++)
                {
                    if (tmpColl[i].transform.GetComponent<PhotonView>())
                        tmpColl[i].transform.GetComponent<PhotonView>().RPC("Attacked", RpcTarget.AllBuffered, new object[] { damage, Vector3.zero, GetComponent<PhotonView>().ViewID });
                }
            }
        }
    }

    [PunRPC]
    public void AttackFunc()
    {
        anim.SetTrigger("Attack");
        attackCooltime = Random.Range(6f, 8f);
    }


    /// <summary>
    /// �ִϸ��̼� �̺�Ʈ�� ����
    /// </summary>
    public void SetAttackTrue()
    {
        attacking = true;
    }

    public void SetAttackFalse()
    {
        attacking = false;
    }

    protected override void AttackedFunc(float _stunTime)
    {
        StartCoroutine("AttackedCoroutine", _stunTime);
    }

    IEnumerator AttackedCoroutine(float stunTime)
    {
        attacking = false;
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