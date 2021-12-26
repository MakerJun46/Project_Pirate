using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hook : MonoBehaviour
{
    public Player_Combat_Ship myShip;
    public Player_Combat_Ship enemyShip;

    float dragForce = 5;
    void Update()
    {
        if(myShip && enemyShip)
        {
            Vector3 distance = (enemyShip.transform.position - myShip.transform.position);
            if (distance.magnitude < 100f)
                Destroy(this.gameObject);
            myShip.additionalForce = distance.normalized * dragForce;
            enemyShip.additionalForce = -distance.normalized * dragForce;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (enemyShip == null)
        {
            if(other.CompareTag("Player") &&
                other.transform != myShip.transform)
            {
                enemyShip = other.GetComponent<Player_Combat_Ship>();
            }
        }
    }
}
