using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CombatManager : MonoBehaviour
{
    Player_Combat_Ship myShip;

    public List<AttackJoyStick> joySticks = new List<AttackJoyStick>();
    public List<AttackJoyStick> SpecialJoySticks = new List<AttackJoyStick>();

    

    public void EquipDefaultCannonUI(int _spotIndex)
    {
        EquipCannon(_spotIndex, 0);
    }
    public void EquipSpecialCannonUI(int _spotIndex)
    {
        EquipSpecialCannon(_spotIndex, 0);
    }
    public void EquipSailUI(int _sailIndex)
    {
        EquipSail(0, _sailIndex);
    }

    public void SetMyShip(Player_Combat_Ship _ship)
    {
        myShip = _ship;
    }

    private void EquipSail(int _spotIndex, int _sailIndex)
    {
        myShip.GetComponent<Photon.Pun.PhotonView>().RPC("EquipSail", Photon.Pun.RpcTarget.AllBuffered, new object[] { _spotIndex, _sailIndex });
    }
    private void EquipCannon(int _spotIndex, int _cannonIndex)
    {
        //Cannon tmpCannon = myShip.EquipCannon(_spotIndex, _cannonIndex);
        myShip.GetComponent<Photon.Pun.PhotonView>().RPC("EquipCannon", Photon.Pun.RpcTarget.AllBuffered, new object[] { _spotIndex, _cannonIndex });
    }
    private void EquipSpecialCannon(int _spotIndex, int _cannonIndex)
    {
        //Cannon tmpCannon = myShip.EquipCannon(_spotIndex, _cannonIndex);
        myShip.GetComponent<Photon.Pun.PhotonView>().RPC("EquipSpecialCannon", Photon.Pun.RpcTarget.AllBuffered, new object[] { _spotIndex, _cannonIndex });
    }
    public void AddAutoCannonType(int param)
    {
        myShip.ChangeCannonType(param, 1, false);
    }
    public void AddSpecialCannonType(int param)
    {
        myShip.ChangeSpecialCannonType(param, 1, false);
    }
}
