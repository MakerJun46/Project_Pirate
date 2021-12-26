using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Barrel : MonoBehaviour
{
    public Vector3 gravity;
    Rigidbody rb;
    private bool hittedFloor;

    bool canAttack;

    public float power=100;
    [SerializeField] LayerMask floorLayer;
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        Invoke("Test", 1f);
    }
    private void Test()
    {
        canAttack = true;
    }
    private void Update()
    {
        RaycastHit hit;
        if(GetComponent<Rigidbody>().velocity.y<0 && Physics.Raycast(this.transform.position,Vector3.down, out hit, 10, floorLayer)){
            hittedFloor = true;
            GetComponent<Rigidbody>().velocity = Vector3.zero;
        }
        else
        {
            hittedFloor = false;
        }

        if (hittedFloor)
        {
            rb.velocity = -0.5f * gravity;
        }
    }
    private void FixedUpdate()
    {
        rb.AddForce(gravity);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")&& canAttack)
        {
            other.GetComponent<Player_Combat_Ship>().additionalForce = (other.transform.position-this.transform.position).normalized * power;
            Destroy(this.gameObject);
        }
    }
}
