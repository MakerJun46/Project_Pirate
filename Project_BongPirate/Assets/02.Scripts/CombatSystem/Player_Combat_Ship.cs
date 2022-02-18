using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
public class Player_Combat_Ship : MonoBehaviourPun
{
    [SerializeField] private float health;
    [SerializeField] private float maxHealth;

    [SerializeField] private List<GameObject> shipObjects;
    private GameObject myShipObjects;

    // level 1: 1, level 2: 1, level 3: 2, level4 : 3 
    [SerializeField] private List<Transform> SailSpots;

    // level 1: 0, level 2: 1, level 3: 1, level4 : 2 
    [SerializeField] private List<Transform> SpecialCannonSpots;

    // level 1: 1, level 2: 2, level 3: 4, level4 : 4 
    [SerializeField] private List<Transform> AutoCannonSpots;

    [SerializeField] private List<Cannon> mySpecialCannons;
    [SerializeField] private List<Cannon> myAutoCannons;
    [SerializeField] private List<GameObject> mySails;


    private void Start()
    {
        health = maxHealth;
        GetComponent<Photon.Pun.PhotonView>().RPC("InitializeCombat", Photon.Pun.RpcTarget.AllBuffered, 0);
    }
    [PunRPC]
    public void InitializeCombat(int param)
    {
        for (int i = 0; i < shipObjects.Count; i++)
        {
            shipObjects[i].gameObject.SetActive(false);
        }
        myShipObjects = shipObjects[(int)param];
        myShipObjects.SetActive(true);

        maxHealth += (int)param * 50f;
        health += (int)param * 50f;

        AutoCannonSpots.Clear();
        SpecialCannonSpots.Clear();
        SailSpots.Clear();

        for (int i = 0; i < myShipObjects.transform.Find("CannonSpots").childCount; i++)
            AutoCannonSpots.Add(myShipObjects.transform.Find("CannonSpots").GetChild(i));
        for (int i = 0; i < myShipObjects.transform.Find("SpecialCannonSpots").childCount; i++)
            SpecialCannonSpots.Add(myShipObjects.transform.Find("SpecialCannonSpots").GetChild(i));
        for (int i = 0; i < myShipObjects.transform.Find("SailSpots").childCount; i++)
            SailSpots.Add(myShipObjects.transform.Find("SailSpots").GetChild(i));


        for (int i = 0; i < myAutoCannons.Count; i++)
        {
            if (myAutoCannons[i] != null)
            {
                myAutoCannons[i].transform.SetParent(AutoCannonSpots[i]);
                myAutoCannons[i].transform.localPosition = Vector3.zero;
                myAutoCannons[i].transform.localRotation = Quaternion.identity;
            }
        }
        for (int i = 0; i < mySpecialCannons.Count; i++)
        {
            if (mySpecialCannons[i] != null)
            {
                mySpecialCannons[i].transform.SetParent(SpecialCannonSpots[i]);
                mySpecialCannons[i].transform.localPosition = Vector3.zero;
                mySpecialCannons[i].transform.localRotation = Quaternion.identity;
            }
        }
        for (int i = 0; i < mySails.Count; i++)
        {
            if (mySails[i] != null)
            {
                mySails[i].transform.SetParent(SailSpots[i]);
                mySails[i].transform.localPosition = Vector3.zero;
                mySails[i].transform.localRotation = Quaternion.identity;
            }
        }

        for (int i = 0; i < AutoCannonSpots.Count; i++)
        {
            if (myAutoCannons.Count < AutoCannonSpots.Count)
                myAutoCannons.Add(null);
        }
        for (int i = 0; i < SpecialCannonSpots.Count; i++)
        {
            if (mySpecialCannons.Count < SpecialCannonSpots.Count)
                mySpecialCannons.Add(null);
        }
        for (int i = 0; i < SailSpots.Count; i++)
        {
            if (mySails.Count < SailSpots.Count)
                mySails.Add(null);
        }
    }

    public float GetSailSpeed()
    {
        float tmp = 0;
        for (int i = 0; i < mySails.Count; i++)
        {
            if (mySails[i] != null)
                tmp += 5f;
        }
        return tmp;
    }

    [PunRPC]
    public void Attacked(object[] param)
    {
        health -= (float)param[0];
        GetComponent<Player_Controller_Ship>().additionalForce = (Vector3)param[1];
        GetComponent<Player_UI_Ship>().UpdateHealth(health / maxHealth);

        if (health <= 0)
        {
            GameObject go = PhotonNetwork.Instantiate("TreasureChest", transform.position, Quaternion.identity);
            
            for(int i = 0; i <Item_Manager.instance.Player_items.Count; i++)
            {
                go.GetComponent<TreasureChest>().items.Add(Item_Manager.instance.Player_items[i]);
            }
            if (photonView.IsMine)
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(0, UnityEngine.SceneManagement.LoadSceneMode.Single);
            }

            Destroy(this.gameObject);
        }
    }

    [PunRPC]
    public void EquipSail(int _spotIndex, int _sailIndex)
    {
        if (mySails[_spotIndex] == null)
        {
            GetComponent<Player_Controller_Ship>().MaxSpeed += 5f;
            GetComponent<Player_Controller_Ship>().MoveSpeed += 5f;

            GameObject tmpCannon = null;
            tmpCannon = Instantiate(Resources.Load("Sail_" + _sailIndex) as GameObject, Vector3.zero, Quaternion.identity);

            tmpCannon.transform.SetParent(SailSpots[_spotIndex]);
            tmpCannon.transform.localPosition = Vector3.zero;
            tmpCannon.transform.localScale = Vector3.one;
            tmpCannon.transform.localRotation = Quaternion.identity;
            mySails[_spotIndex] = tmpCannon;
        }
        else
        {
            GetComponent<Player_Controller_Ship>().MaxSpeed -= 5f;
            GetComponent<Player_Controller_Ship>().MoveSpeed -= 5f;
            Destroy(mySails[_spotIndex].gameObject);
        }
    }

    [PunRPC]
    public void EquipCannon(int _spotIndex, int _cannonIndex)
    {
        bool _active = true;
        if (myAutoCannons[_spotIndex] == null)
        {
            GameObject tmpCannon = null;
            tmpCannon = Instantiate(Resources.Load("AutoCannon_" + _cannonIndex) as GameObject, Vector3.zero, Quaternion.identity);

            tmpCannon.transform.SetParent(AutoCannonSpots[_spotIndex]);
            tmpCannon.transform.localPosition = Vector3.zero;
            tmpCannon.transform.localScale = Vector3.one;
            tmpCannon.transform.localRotation = Quaternion.identity;
            myAutoCannons[_spotIndex] = tmpCannon.GetComponent<Cannon>();
            tmpCannon.GetComponent<Cannon>().Initialize(this);
        }
        else
        {
            _active = false;
            myAutoCannons[_spotIndex].UnEquipCannon();
        }

        if (photonView.IsMine)
        {
            CombatManager combatManager = FindObjectOfType<CombatManager>();
            print("joystick : " + _active);
            combatManager.joySticks[_spotIndex].gameObject.SetActive(_active);
            if (myAutoCannons[_spotIndex] != null)
            {
                myAutoCannons[_spotIndex].tmpJoyStick = combatManager.joySticks[_spotIndex];
            }
        }
    }
    [PunRPC]
    public void EquipSpecialCannon(int _spotIndex, int _cannonIndex)
    {
        bool _active = true;
        if (mySpecialCannons[_spotIndex] == null)
        {
            GameObject tmpCannon = null;
            tmpCannon = Instantiate(Resources.Load("SpecialCannon_" + _cannonIndex) as GameObject, Vector3.zero, Quaternion.identity);
            tmpCannon.transform.SetParent(SpecialCannonSpots[_spotIndex]);
            tmpCannon.transform.localPosition = Vector3.zero;
            tmpCannon.transform.localScale = Vector3.one;
            tmpCannon.transform.localRotation = Quaternion.identity;
            mySpecialCannons[_spotIndex] = tmpCannon.GetComponent<Cannon>();
            tmpCannon.GetComponent<Cannon>().Initialize(this);
        }
        else
        {
            _active = false;
            mySpecialCannons[_spotIndex].UnEquipCannon();
        }

        if (photonView.IsMine)
        {
            CombatManager combatManager = FindObjectOfType<CombatManager>();
            print("special : " + _active);
            combatManager.SpecialJoySticks[_spotIndex].gameObject.SetActive(_active);
            if (mySpecialCannons[_spotIndex] != null)
            {
                mySpecialCannons[_spotIndex].tmpJoyStick = combatManager.SpecialJoySticks[_spotIndex];
            }
        }
    }

    public void ChangeCannonType(int _spotIndex, int _typeIndex, bool _isSet = true)
    {
        myAutoCannons[_spotIndex].ChangeCannonType(_typeIndex, _isSet);
    }
    public void ChangeSpecialCannonType(int _spotIndex, int _typeIndex, bool _isSet = true)
    {
        mySpecialCannons[_spotIndex].ChangeCannonType(_typeIndex, _isSet);
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (photonView.IsMine && collision.transform.CompareTag("Player"))
        {
            if (collision.transform.GetComponent<Player_Combat_Ship>())
            {
                collision.transform.GetComponent<PhotonView>().RPC("Attacked", RpcTarget.AllBuffered, new object[] {
                    10.0f,
                    (collision.transform.position-this.transform.position).normalized*50f
                });
                this.transform.GetComponent<PhotonView>().RPC("Attacked", RpcTarget.AllBuffered, new object[] {
                    5.0f,
                    (this.transform.position-collision.transform.position).normalized*50f
                });
            }
        }
    }
}
