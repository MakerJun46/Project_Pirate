using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Octopus : SurvivorMonster
{
    private Transform ghostContainer;

    Animator anim;

    [SerializeField] List<ParticleSystem> AttackParticle;


    private float attackCooltime = 5f;

    bool attacking = false;

    public override void ResetEnemy(SurvivorMonster _data)
    {
        base.ResetEnemy(_data);
    }

    protected override void Start()
    {
        base.Start();
        anim = GetComponent<Animator>();
        anim.speed = Random.Range(0.8f, 1.2f);
        attackCooltime = Random.Range(1, 5f);

    }


    protected override void Update()
    {
        base.Update();
        if (GetComponent<PhotonView>().IsMine)
        {
            attackCooltime -= Time.deltaTime;
            if (attackCooltime <= 0)
            {
                if (!attacking && target && Vector3.Distance(target.position, transform.position) <= viewRadius)
                {
                    if (Photon.Pun.PhotonNetwork.IsConnected)
                        GetComponent<PhotonView>().RPC("AttackFunc", RpcTarget.AllBuffered);
                    else
                        AttackFunc();
                }
            }
            if (!attacking && attackCooltime <= 4 && target)
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
    /// 애니메이션 이벤트로 실행
    /// </summary>
    public void SetAttackTrue()
    {
        attacking = true;

        for (int i = 0; i < AttackParticle.Count; i++)
        {
            AttackParticle[i].Play();
        }
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
        }
        else
        {
        }
    }
}
