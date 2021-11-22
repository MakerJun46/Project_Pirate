using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hook : MonoBehaviour
{
    public Player_Combat_Ship myShip;
    public Player_Combat_Ship enemyShip;

    float force = 5;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(myShip && enemyShip)
        {
            Vector3 distance = (enemyShip.transform.position - myShip.transform.position);
            if (distance.magnitude < 100f)
                Destroy(this.gameObject);
            myShip.additionalForce = distance.normalized * force;
            enemyShip.additionalForce = -distance.normalized * force;
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
