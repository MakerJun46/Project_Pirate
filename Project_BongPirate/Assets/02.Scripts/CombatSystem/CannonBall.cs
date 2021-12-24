using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonBall : MonoBehaviour
{
    public Vector3 gravity;
    Rigidbody rb;
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
    void Update()
    {
        transform.GetChild(0).Rotate(new Vector3(rb.velocity.z, 0, -rb.velocity.x) * Time.deltaTime * 60f, Space.World);
    }
    private void FixedUpdate()
    {
        rb.AddForce(gravity);
    }
}
