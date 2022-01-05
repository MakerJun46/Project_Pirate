using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CombatManager : MonoBehaviour
{
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
    }

    public void SetMyShip(Player_Combat_Ship _ship)
    {
        myShip = _ship;
    }

    private void EquipCannon(int _spotIndex, int _cannonIndex)
    {
        //Cannon tmpCannon = myShip.EquipCannon(_spotIndex, _cannonIndex);
        myShip.GetComponent<Photon.Pun.PhotonView>().RPC("EquipCannon", Photon.Pun.RpcTarget.AllBuffered, new object[] { _spotIndex, _cannonIndex });
    }
    public void AddCannonType(int param)
    {
        myShip.ChangeCannonType(param, 1,false);
    }
}
