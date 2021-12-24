using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shark : MonoBehaviour
{
    public Player_Combat_Ship myShip;
    public Player_Combat_Ship enemyShip;

    bool focused;
    [SerializeField] float dragForce = 5;
    [SerializeField] float waitingTime = 10f;
    [SerializeField] float sharkRoundSpeed = 3f;

    [SerializeField] GameObject WaitingSharkObj;
    GameObject shark;

    void Start()
    {

    }

    void Update()
    {
        waitingTime -= Time.deltaTime;
        if (enemyShip)
        {
            if (waitingTime <= 0)
            {
                if (focused == false)
                {
                    WaitingSharkObj.SetActive(false);
                    shark = Instantiate(Resources.Load("Shark") as GameObject, this.transform.position, Quaternion.identity);
                    focused = true;
                }
                shark.transform.position += Vector3.up * Time.deltaTime*4f;
            }
            else
            {
                transform.position = enemyShip.transform.position;
                transform.rotation = Quaternion.identity;
                GetComponent<Rigidbody>().velocity = Vector3.zero;

                WaitingSharkObj.SetActive(true);
                this.transform.Rotate(Vector3.up, Time.time*-360f);
                print("123");
            }
        }


        if (waitingTime < -3f)
        {
            Destroy(shark);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (enemyShip == null)
        {
            if (other.CompareTag("Player") &&
                other.transform != myShip.transform)
            {
                enemyShip = other.GetComponent<Player_Combat_Ship>();
                transform.GetChild(0).gameObject.SetActive(false);
            }
        }
    }
}
