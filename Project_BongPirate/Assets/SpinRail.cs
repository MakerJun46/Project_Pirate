using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinRail : MonoBehaviour
{
    public SphereCollider viewRadius;
    public float power = 10000f;
    public float innerAngle = 10;
    [SerializeField] protected LayerMask targetMask;

    void Start()
    {
        viewRadius = GetComponent<SphereCollider>();
    }


    void Update()
    {
        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, viewRadius.radius, targetMask);
        for (int i = 0; i < targetsInViewRadius.Length; i++)
        {
            Transform target = targetsInViewRadius[i].transform;
            if (target.GetComponent<Player_Controller_Ship>())
            {
                Vector3 dist = (target.position - this.transform.position);
                Vector3 cross = Vector3.Cross(dist.normalized, Vector3.up);

                cross= Quaternion.AngleAxis(innerAngle, Vector3.up) * cross;
                Debug.DrawLine(target.position, target.position + cross * power* (1 - dist.magnitude / viewRadius.radius) * (1 - dist.magnitude / viewRadius.radius));
                target.GetComponent<Player_Controller_Ship>().additionalForce = cross * Time.deltaTime * power * (1 - dist.magnitude / viewRadius.radius) ;
                //target.GetComponent<Rigidbody>().AddTorque (cross * Time.deltaTime * power * dist.magnitude / viewRadius,ForceMode.Impulse);
            }
        }
    }
}
