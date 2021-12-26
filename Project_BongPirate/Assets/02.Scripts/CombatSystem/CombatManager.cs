using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    Player_Combat_Ship myShip;

    [SerializeField] List<AttackJoyStick> joySticks = new List<AttackJoyStick>();
    [SerializeField] List<AttackJoyStick> SpecialJoySticks = new List<AttackJoyStick>();

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
        Cannon tmpCannon = myShip.EquipCannon(_spotIndex, _cannonIndex);
        if (_spotIndex <= 0)
        {
            SpecialJoySticks[_spotIndex].gameObject.SetActive(tmpCannon != null);
            if (tmpCannon != null)
            {
                tmpCannon.tmpJoyStick = SpecialJoySticks[_spotIndex];
            }
        }
        else
        {
            joySticks[_spotIndex].gameObject.SetActive(tmpCannon != null);
            if (tmpCannon != null)
            {
                tmpCannon.tmpJoyStick = joySticks[_spotIndex];
            }
        }
    }
}
