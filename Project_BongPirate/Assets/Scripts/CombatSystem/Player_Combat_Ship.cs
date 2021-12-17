using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using Photon.Realtime;
public class Player_Combat_Ship : MonoBehaviour
{
    public float health;
    public float maxHealth;

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

    public Cannon EquipCannon(int _spotIndex, int _cannonIndex)
    {
        if (myCannons[_spotIndex] == null)
        {
            GameObject tmpCannon = null;
            if (_spotIndex == 0)
                tmpCannon= PhotonNetwork.Instantiate("Cannon_" + _cannonIndex, Vector3.zero, Quaternion.identity);
            else
                tmpCannon = PhotonNetwork.Instantiate("AutoCannon_" + _cannonIndex, Vector3.zero, Quaternion.identity);
            tmpCannon.transform.SetParent(CannonSpots[_spotIndex]);
            tmpCannon.transform.localPosition = Vector3.zero;
            tmpCannon.transform.localScale = Vector3.one;
            tmpCannon.transform.localRotation = Quaternion.identity;
            myCannons[_spotIndex] = tmpCannon.GetComponent<Cannon>();

            return myCannons[_spotIndex];
        }
        else
        {
            PhotonNetwork.Destroy(myCannons[_spotIndex].gameObject);
            return null;
        }
    }
}
