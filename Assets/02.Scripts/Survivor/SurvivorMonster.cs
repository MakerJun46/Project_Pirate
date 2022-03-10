using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SurvivorMonster : MonoBehaviourPunCallbacks, IPunObservable
{
    public float damage = 5f;

    protected Rigidbody rb;

    bool isElite;
    protected Vector3 toTargetDir;
    protected Transform target;
    [SerializeField] protected float viewRadius = 20;
    [SerializeField] protected float attackRadius = 20;
    [SerializeField] protected LayerMask targetLayer;

    [SerializeField] protected float speed = 3f;

    Vector3 Impact;
    [SerializeField] float ImpactMultiplier = 1f;
    protected bool attacked;

    float health;
    [SerializeField] float maxHealth = 100f;

    private List<AttackInfo> attackedList = new List<AttackInfo>();
    protected Collider[] colls;

    [SerializeField] Vector2 EXPPercent;

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody>();


        health = maxHealth;
    }

    public virtual void ResetEnemy(SurvivorMonster _data)
    {
        damage = _data.damage;
        speed = _data.speed;
        maxHealth = _data.maxHealth;
        health = maxHealth;

        this.transform.localScale = _data.transform.localScale;
    }

    public void InitializeEnemy(float _damage, float _vel, float _health,  bool _isElite)
    {
        damage += _damage;
        speed += _vel;
        maxHealth += _health;
        isElite = _isElite;

        attacked = false;
        if (target)
        {
            target = FindObjectOfType<Player_Controller_Ship>().transform;
            toTargetDir = (target.position - this.transform.position).normalized;
        }
        health = maxHealth;

        if (isElite)
        {
            this.transform.localScale *= 2f;
            this.transform.GetComponent<SphereCollider>().radius *= 0.5f;
            //if (FindObjectOfType<Player_Controller_Ship>())
            //    health *= (FindObjectOfType<Player_Controller_Ship>().level / 5f + 1);
        }
    }

    protected virtual void Update()
    {
        if (GetComponent<PhotonView>().IsMine)
        {
            colls = Physics.OverlapSphere(this.transform.position, viewRadius, targetLayer);

            float minDist = 100000;
            for (int i = 0; i < colls.Length; i++)
            {
                float tmpDist = Vector3.Distance(colls[i].transform.position, this.transform.position);
                if (tmpDist <= minDist)
                {
                    minDist = tmpDist;
                    target = colls[i].transform;
                }
            }

            if (attacked)
            {
                rb.velocity = Impact * ImpactMultiplier;
            }

            Impact = Vector3.Lerp(Impact, Vector3.zero, Time.deltaTime);
        }
        for (int i = 0; i < attackedList.Count; i++)
        {
            attackedList[i].lifetime -= Time.deltaTime;
            if (attackedList[i].lifetime <= 0)
            {
                attackedList.RemoveAt(i);
                break;
            }
        }
    }


    [PunRPC]
    public void Attacked(object[] param)
    {
        bool canAttack = false;
        if (param.Length > 2)
        {
            if (attackedList.Find(s => s.id == (int)param[2]) == null)
            {
                attackedList.Add(new AttackInfo((int)param[2], 1f));
                canAttack = true;
            }
        }
        else
            canAttack = true;

        if (canAttack)
        {
            health -= (float)param[0];
            //additionalForce = (Vector3)param[1];

            print("Attacked : " + health);
            AttackedFunc(1f);

            if (health <= 0)
            {
                print("Destroy");
                Destroy(this.gameObject);
            }
        }
    }

    protected virtual void AttackedFunc(float _stunTime)
    {
        //StartCoroutine("AttackedCoroutine", _stunTime);
    }

    public virtual void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        throw new System.NotImplementedException();
    }
}
