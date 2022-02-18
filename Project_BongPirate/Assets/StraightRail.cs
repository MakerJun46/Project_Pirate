using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StraightRail : MonoBehaviour
{
    public BoxCollider halfExtents;
    public float power = 10000f;

    [SerializeField] protected LayerMask targetMask;

    void Start()
    {
        halfExtents = GetComponent<BoxCollider>();
    }


    void Update()
    {
        Collider[] targetsInViewRadius = Physics.OverlapBox(transform.position, halfExtents.size/2f,this.transform.rotation, targetMask);
        for (int i = 0; i < targetsInViewRadius.Length; i++)
        {
            Transform target = targetsInViewRadius[i].transform;
            if (target.GetComponent<Player_Controller_Ship>())
            {
                Vector3 dist = (target.position - this.transform.position);

                target.GetComponent<Player_Controller_Ship>().additionalForce += this.transform.forward * power *Time.deltaTime;
                //target.GetComponent<Rigidbody>().AddTorque (cross * Time.deltaTime * power * dist.magnitude / viewRadius,ForceMode.Impulse);
            }
        }
    }
}
