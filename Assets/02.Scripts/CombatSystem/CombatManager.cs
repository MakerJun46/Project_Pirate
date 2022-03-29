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
