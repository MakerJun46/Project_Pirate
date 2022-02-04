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

    System.Random random = new System.Random();

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
            Landed_island = GameManager.GetIstance().All_Island[GameManager.GetIstance().MyShip.Landed_island_ID];

            if(Landed_island.Wood_Object.Count > 0 || Landed_island.Rock_Object.Count > 0)
            {
                if(Landed_island.Wood_Object.Count == 0)
                {
                    Gathering_target = Landed_island.Rock_Object[0];
                }
                else if(Landed_island.Rock_Object.Count == 0)
                {
                    Gathering_target = Landed_island.Wood_Object[0];
                }
                else
                {
                    Gathering_target = random.Next(0, 1) == 0 ? Landed_island.Wood_Object[0] : Landed_island.Rock_Object[0];
                }

                status = Sailor_Status.Gathering;

                StartCoroutine(gathering(Gathering_target));
            }
            else
            {
                status = Sailor_Status.Escaping;    // 채집할 자원이 더 없으면 바로 escape
                GameManager.GetIstance().island_LandingEscape_Button();
            }
        }
        else if(status == Sailor_Status.Gathering)
        {
            this.transform.position = Vector3.Lerp(this.transform.position, Gathering_target.transform.position, Time.deltaTime * 3);
        }

        else if (status == Sailor_Status.Escaping)
        {
            this.transform.position = Vector3.Lerp(this.transform.position, MyShip.transform.position, Time.deltaTime * 5);
        }
    }

    public IEnumerator gathering(GameObject target)
    {
        yield return new WaitForSeconds(2.0f);
        Debug.Log("자원 채집함, 채집한 자원 : " + target.name);

        int itemIndex = (int)target.GetComponent<Resource>().type;

        Debug.Log("itemIndex : " + itemIndex);

        Item_Inventory _item = Item_Manager.GetInstance().item_list[itemIndex];
        Item_Manager.GetInstance().AddItem(_item);

        Landed_island.getResource_Text.text = target.name.Substring(0, 4) + " + 1";
        Landed_island.getResource_Text.GetComponent<Animation>().Play();
        
        status = Sailor_Status.Landing;

        if (itemIndex == 0) // wood
            Landed_island.Wood_Object.Remove(target);
        else if (itemIndex == 1) // rock
            Landed_island.Rock_Object.Remove(target);

        Destroy(target);
    }
}
