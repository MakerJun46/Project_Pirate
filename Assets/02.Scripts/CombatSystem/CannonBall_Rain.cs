using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonBall_Rain : CannonBall
{
    [SerializeField] LayerMask WaterLayer;
    [SerializeField] GameObject WarningCursor;
    protected override void Update()
    {
        base.Update();

        RaycastHit hit;
        if( Physics.Raycast(this.transform.position, Vector3.down,out hit, 100f, WaterLayer)){
            WarningCursor.SetActive(true);
            WarningCursor.transform.position = hit.point;
        }
        else
        {
            WarningCursor.SetActive(false);
        }
    }
}
