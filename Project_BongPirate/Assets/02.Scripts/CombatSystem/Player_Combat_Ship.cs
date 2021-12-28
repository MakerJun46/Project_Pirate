using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using Photon.Realtime;
public class Player_Combat_Ship : MonoBehaviourPun
{
    [SerializeField] private float health;
    [SerializeField] private float maxHealth;

    [SerializeField] private List<Transform> CannonSpots;

    [SerializeField] private List<Cannon> myCannons;

    public Vector3 additionalForce;

    private void Start()
    {
        health = maxHealth;
        for (int i=0;i< CannonSpots.Count; i++)
        {
            myCannons.Add(null);
        }
    }

    private void Update()
    {
        if (additionalForce.magnitude > 0.1f)
        {
            GetComponent<Rigidbody>().AddForce(additionalForce);
            additionalForce = Vector3.Lerp(additionalForce, Vector3.zero, Time.deltaTime);
        }
    }
    [PunRPC]
    public void Attacked(float damage)
    {
        health -= damage;
        GetComponent<Player_UI_Ship>().UpdateHealth(health / maxHealth);

        if (health <= 0)
            Destroy(this.gameObject);
    }

    [PunRPC]
    public void EquipCannon(int _spotIndex, int _cannonIndex)
    {
        if (myCannons[_spotIndex] == null)
        {
            GameObject tmpCannon = null;
            if (_spotIndex == 0)
                tmpCannon= Instantiate(Resources.Load("Cannon_" + _cannonIndex) as GameObject, Vector3.zero, Quaternion.identity);
            else
                tmpCannon = Instantiate(Resources.Load("AutoCannon_" + _cannonIndex) as GameObject, Vector3.zero, Quaternion.identity);
            tmpCannon.transform.SetParent(CannonSpots[_spotIndex]);
            tmpCannon.transform.localPosition = Vector3.zero;
            tmpCannon.transform.localScale = Vector3.one;
            tmpCannon.transform.localRotation = Quaternion.identity;
            myCannons[_spotIndex] = tmpCannon.GetComponent<Cannon>();
            tmpCannon.GetComponent<Cannon>().Initialize(this);
        }
        else
        {
            Destroy(myCannons[_spotIndex].gameObject);
        }

        if (photonView.IsMine)
        {
            CombatManager combatManager = FindObjectOfType<CombatManager>();
            if (_spotIndex <= 0)
            {
                combatManager.SpecialJoySticks[_spotIndex].gameObject.SetActive(myCannons[_spotIndex] != null);
                if (myCannons[_spotIndex] != null)
                {
                    myCannons[_spotIndex].tmpJoyStick = combatManager.SpecialJoySticks[_spotIndex];
                }
            }
            else
            {
                combatManager.joySticks[_spotIndex].gameObject.SetActive(myCannons[_spotIndex] != null);
                if (myCannons[_spotIndex] != null)
                {
                    myCannons[_spotIndex].tmpJoyStick = combatManager.joySticks[_spotIndex];
                }
            }
        }
    }
}
