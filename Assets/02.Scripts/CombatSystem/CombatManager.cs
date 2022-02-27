using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CombatManager : MonoBehaviour
{
    public static CombatManager instance;

    private void Awake()
    {
        instance = this;
    }


    Player_Combat_Ship myShip;

    public List<AttackJoyStick> joySticks = new List<AttackJoyStick>();
    public List<AttackJoyStick> SpecialJoySticks = new List<AttackJoyStick>();


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            EquipCannon(0, 0);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            EquipCannon(1, 0);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            EquipCannon(2, 0);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            EquipCannon(3, 0);
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            EquipSail(0, 0);
        }
        else if (Input.GetKeyDown(KeyCode.O))
        {
            EquipSail(0, 1);
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            EquipSpecialCannon(0, 0);
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            EquipSpecialCannon(1, 0);
        }
    }

    public void SetMyShip(Player_Combat_Ship _ship)
    {
        myShip = _ship;
    }

    public void EquipSail(int _spotIndex, int _sailIndex)
    {
        if(myShip)
            myShip.GetComponent<Photon.Pun.PhotonView>().RPC("EquipSail", Photon.Pun.RpcTarget.AllBuffered, new object[] { _spotIndex, _sailIndex });
    }
    public void EquipCannon(int _spotIndex, int _cannonIndex)
    {
        //Cannon tmpCannon = myShip.EquipCannon(_spotIndex, _cannonIndex);
        if (myShip)
            myShip.GetComponent<Photon.Pun.PhotonView>().RPC("EquipCannon", Photon.Pun.RpcTarget.AllBuffered, new object[] { _spotIndex, _cannonIndex });
    }
    public void EquipSpecialCannon(int _spotIndex, int _cannonIndex)
    {
        //Cannon tmpCannon = myShip.EquipCannon(_spotIndex, _cannonIndex);
        if (myShip)
            myShip.GetComponent<Photon.Pun.PhotonView>().RPC("EquipSpecialCannon", Photon.Pun.RpcTarget.AllBuffered, new object[] { _spotIndex, _cannonIndex });
    }
    public void AddAutoCannonType(int param)
    {
        if (myShip)
            myShip.ChangeCannonType(param, 1, false);
    }
    public void AddSpecialCannonType(int param)
    {
        if (myShip)
            myShip.ChangeSpecialCannonType(param, 1, false);
    }
}
