using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sailor : MonoBehaviour
{
    public enum Sailor_Status { Landing, Gathering, Sailing, Attacking, Escaping };

    public Sailor_Status status;
    public Island_Info Landed_island;

    public GameObject MyShip;

    public GameObject Gathering_target;

    public void Start()
    {
        MyShip = GameManager.GetIstance().MyShip.gameObject;
        status = Sailor_Status.Sailing;
    }

    public void Update()
    {
        Active_By_Status();
    }

    public void Active_By_Status()
    {
        if (status == Sailor_Status.Landing)
        {
            Landed_island = GameManager.GetIstance().All_Island[GameManager.GetIstance().MyShip.Laned_island_ID];

            if(Landed_island.Wood_Object.Count != 0)
            {
                Gathering_target = Landed_island.Wood_Object[0];

                status = Sailor_Status.Gathering;

                StartCoroutine(gathering(Gathering_target));
            }
        }
        else if(status == Sailor_Status.Gathering)
        {
            this.transform.position = Vector3.Lerp(this.transform.position, Gathering_target.transform.position, Time.deltaTime);
        }

        else if (status == Sailor_Status.Escaping)
        {
            this.transform.position = Vector3.Lerp(this.transform.position, MyShip.transform.position, Time.deltaTime);
        }
    }

    public IEnumerator gathering(GameObject target)
    {
        yield return new WaitForSeconds(2.0f);
        Debug.Log("자원 채집함, 채집한 자원 : " + target.name);

        Item_Inventory _item = Item_Manager.GetInstance().item_list[0];
        Item_Manager.GetInstance().AddItem(_item);

        Landed_island.Wood_Object.Remove(target);
        Destroy(target);
        
        status = Sailor_Status.Landing;
    }
}
