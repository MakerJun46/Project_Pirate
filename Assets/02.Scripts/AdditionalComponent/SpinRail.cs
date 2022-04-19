using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinRail : MonoBehaviour
{
    [SerializeField] SphereCollider viewRadius;
    float realRadius;

    [SerializeField] float power = 10000f;
    [SerializeField] float innerAngle = 10;

    [SerializeField] float lifetime;
    [SerializeField] protected LayerMask targetMask;

    void Start()
    {
        viewRadius = GetComponent<SphereCollider>();
        realRadius = 0f;
        StartCoroutine(LifetimeCoroutine());
    }


    void Update()
    {
        transform.GetChild(0).localScale = new Vector3(realRadius*2f,1f, realRadius * 2f);
        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, realRadius, targetMask);
        for (int i = 0; i < targetsInViewRadius.Length; i++)
        {
            Transform target = targetsInViewRadius[i].transform;
            if (target.GetComponent<Player_Controller_Ship>())
            {
                Vector3 dist = (target.position - this.transform.position);
                Vector3 cross = Vector3.Cross(dist.normalized, Vector3.up);

                cross= Quaternion.AngleAxis(innerAngle, Vector3.up) * cross;
                Debug.DrawLine(target.position, target.position + cross * power* (1 - dist.magnitude / realRadius) * (1 - dist.magnitude / realRadius));
                target.GetComponent<Player_Controller_Ship>().additionalForce += cross * Time.deltaTime * power * (1 - dist.magnitude / realRadius) ;
            }
        }
    }

    IEnumerator LifetimeCoroutine()
    {
        while (viewRadius.radius- realRadius >= 0.1f)
        {
            realRadius = Mathf.Lerp(realRadius, viewRadius.radius, Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }
        yield return new WaitForSeconds(lifetime);

        while (realRadius > 0.1f)
        {
            realRadius = Mathf.Lerp(realRadius, 0, Time.deltaTime*3f);
            yield return new WaitForEndOfFrame();
        }
        Destroy(this.gameObject);

    }
}
