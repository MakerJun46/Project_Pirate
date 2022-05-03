using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
public class FloatingObjectsEffect : MonoBehaviour
{
    public float underWaterDrag = 3f;
    public float underWaterangularDrag = 1f;
    public float airDrag = 0f;
    public float airAngularDrag = 0.0f;
    public float floatingPower = 100f;

    public float waterHeight = 0f;

    Rigidbody RB;
    bool underwater;

    void Start()
    {
        RB = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        float diff = transform.position.y - waterHeight;

        if(diff < -1)
        {
            RB.AddForceAtPosition(Vector3.up * floatingPower * Mathf.Abs(diff), transform.position, ForceMode.Force);
            
            if(!underwater)
            {
                underwater = true;
                SwitchState(underwater);
            }
        }
        else if(underwater)
        {
            underwater = false;
            SwitchState(underwater);
        }
    }

    public void SwitchState(bool isUnderWater)
    {
        if(isUnderWater)
        {
            RB.drag = underWaterDrag;
            RB.angularDrag = underWaterangularDrag;
        }
        else
        {
            RB.drag = airDrag;
            RB.angularDrag = airAngularDrag;
        }
    }
}
