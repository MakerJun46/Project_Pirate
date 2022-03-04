using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SurvivorMonster : MonoBehaviour
{
    public float damage = 5f;

    Rigidbody rb;

    bool followTarget;
    bool isElite;
    Vector3 originTargetPos;
    Transform target;
    [SerializeField] float speed = 3f;

    Vector3 Impact;
    [SerializeField] float ImpactMultiplier = 1f;
    bool attacked;

    float health;
    [SerializeField] float maxHealth = 100f;

    private List<AttackInfo> attackedList = new List<AttackInfo>();

    [SerializeField] LayerMask targetLayer;
    [SerializeField] Vector2 EXPPercent;

    public string prefabName;
    void Start()
    {
        rb = GetComponent<Rigidbody>();


        health = maxHealth;
    }

    public void ResetEnemy(SurvivorMonster _data)
    {
        prefabName = _data.name;
        damage = _data.damage;
        speed = _data.speed;
        maxHealth = _data.maxHealth;
        health = maxHealth;

        this.transform.localScale = _data.transform.localScale;
        GetComponentInChildren<MeshRenderer>().material.color = Color.white;
        this.transform.GetComponent<SphereCollider>().radius = _data.transform.GetComponent<SphereCollider>().radius;
    }

    public void InitializeEnemy(float _damage, float _vel, float _health, bool _followTarget, bool _isElite)
    {
        damage += _damage;
        speed += _vel;
        maxHealth += _health;
        followTarget = _followTarget;
        isElite = _isElite;

        attacked = false;
        if (target)
        {
            target = FindObjectOfType<Player_Controller_Ship>().transform;
            originTargetPos = (target.position - this.transform.position).normalized;
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

    float targetFollowTime = 3;
    Collider[] colls;
    void Update()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (target == null)
            {
                Player_Controller_Ship[] players = FindObjectsOfType<Player_Controller_Ship>();
                float minDist = 100000;
                foreach(Player_Controller_Ship player in players)
                {
                    float tmpDist = Vector3.Distance(player.transform.position, this.transform.position);
                    if (tmpDist <= minDist)
                    {
                        minDist = tmpDist;
                        target = player.transform;
                    }
                }
            }
            else
            {
                targetFollowTime -= Time.deltaTime;
                if (targetFollowTime <= 0)
                {
                    targetFollowTime = 3f;
                    target = null;
                }
            }

            colls = Physics.OverlapSphere(this.transform.position, GetComponent<SphereCollider>().radius*2f+1f, targetLayer);
          
            for(int i = 0; i < colls.Length; i++)
            {
                colls[i].transform.GetComponent<PhotonView>().RPC("Attacked", RpcTarget.AllBuffered, new object[] { damage, Vector3.zero, GetComponent<PhotonView>().ViewID });
            }

            if (attacked)
            {
                rb.velocity = Impact * ImpactMultiplier;
            }
            else
            {
                if (followTarget && target)
                {
                    originTargetPos = (target.position - this.transform.position).normalized;
                }
                rb.velocity = originTargetPos * speed;
                this.transform.LookAt(target);
            }
            Impact = Vector3.Lerp(Impact, Vector3.zero, Time.deltaTime);

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
        //GetComponentInChildren<Animator>().SetFloat("Vel", rb2D.velocity.magnitude);
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
            //AttackedPS.Play();

            health -= (float)param[0];
            //additionalForce = (Vector3)param[1];

            StartCoroutine("AttackedCoroutine",1f);

            if (health <= 0)
            {
                /*
                GameObject go = PhotonNetwork.Instantiate("TreasureChest", transform.position, Quaternion.identity);

                for (int i = 0; i < Item_Manager.instance.Player_items.Count; i++)
                {
                    go.GetComponent<TreasureChest>().items.Add(Item_Manager.instance.Player_items[i]);
                }
                */
                //GameManager.GetIstance().EndGame(false);

                Destroy(this.gameObject);
            }
        }
    }

    IEnumerator AttackedCoroutine(float stunTime)
    {
        attacked = true;
        GetComponentInChildren<MeshRenderer>().material.color = Color.red;
        yield return new WaitForSeconds(stunTime);
        GetComponentInChildren<MeshRenderer>().material.color = Color.white;
        attacked = false;
    }
}
