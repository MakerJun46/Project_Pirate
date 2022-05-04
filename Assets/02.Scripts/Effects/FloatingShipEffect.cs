using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
public class FloatingShipEffect : MonoBehaviour
{
    public float amplitude;          //Set in Inspector 
    public float speed;                  //Set in Inspector 
    private float tempVal;
    private Vector3 tempPos;
    public float yOffset;

    void Start()
    {
        tempPos = transform.position;
        tempVal = transform.position.y;
    }

    void Update()
    {
        tempPos.y = tempVal + amplitude * Mathf.Sin(speed * Time.time) - yOffset;
        transform.position = tempPos;
    }
}
