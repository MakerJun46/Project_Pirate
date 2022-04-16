using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class AttackInfo
{
    public int id;
    public float lifetime;
    public AttackInfo(int _id, float _lifeTime = 1f)
    {
        id = _id;
        lifetime = _lifeTime;
    }
}

public class Player_Combat_Ship : MonoBehaviourPun
{
    public float health;
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

    [SerializeField] private ParticleSystem AttackedPS;
    [SerializeField] private List<ParticleSystem> AttackedPS_Flare;
    CinemachineImpulseSource impulseSource;


    List<AttackInfo> AttackIDs = new List<AttackInfo>();

    private void Start()
    {
        health = maxHealth;
        GetComponent<Photon.Pun.PhotonView>().RPC("InitializeCombat", Photon.Pun.RpcTarget.AllBuffered, 0);
        impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    private void Update()
    {
        for(int i= AttackIDs.Count-1; i>=0 ; i--)
        {
            AttackIDs[i].lifetime -= Time.deltaTime;
            if (AttackIDs[i].lifetime <= 0)
                AttackIDs.RemoveAt(i);
        }
    }

    [PunRPC]
    public void InitializeCombat(int param)
    {
        if (shipObjects.Count <= (int)param)
            return;

        for (int i = 0; i < shipObjects.Count; i++)
        {
            shipObjects[i].gameObject.SetActive(false);
        }
        myShipObjects = shipObjects[(int)param];
        myShipObjects.SetActive(true);

        // upgrade
        maxHealth += (int)param * 25f;
        health += (int)param * 25f;

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
    public void PlayAttackPS(int _spotIndex, bool _isSpecial = false)
    {
        ShakeCamera(2f);
        if (_isSpecial)
        {
            mySpecialCannons[_spotIndex].PlayAttackPS();
        }
        else
        {
            myAutoCannons[_spotIndex].PlayAttackPS();
        }
    }

    [PunRPC]
    public void Attacked(object[] param)
    {
        PassTheBombGameManager passTheBombGameManager = FindObjectOfType<PassTheBombGameManager>();
        // pass the bomb °ÔÀÓÀÎ °æ¿ì ºÎµúÈ÷¸é ÆøÅº ÀüÀÌ
        if (passTheBombGameManager && passTheBombGameManager.hasBomb && param.Length > 2 && PhotonView.Find((int)param[2]).transform.GetComponent<Player_Combat_Ship>())
        {
            if ((int)param[2] != photonView.ViewID)
                passTheBombGameManager.CrashOtherShip(PhotonView.Find((int)param[2]).transform.gameObject);
        }

        bool canAttack = false;
        if (param.Length > 2)
        {
            if (AttackIDs.Find(s=>s.id== (int)param[2]) == null)
            {
                AttackIDs.Add(new AttackInfo((int)param[2],1f));
                canAttack = true;
            }
        }
        else
            canAttack = true;

        if (canAttack)
        {
            FloatingTextController.CreateFloatingText("- " + (float)param[0], this.transform, Color.red);

            if (param.Length > 2 && PhotonView.Find((int)param[2]).transform.GetComponent<CannonBall>())
            {
                ParticleSystem tmpVFX = Instantiate(AttackedPS_Flare[Random.Range(0, AttackedPS_Flare.Count)], this.transform.position, Quaternion.identity);
                tmpVFX.transform.position = PhotonView.Find((int)param[2]).transform.position;
                tmpVFX.transform.LookAt(this.transform.position + (PhotonView.Find((int)param[2]).transform.position - this.transform.position).normalized);
                tmpVFX.transform.localScale = Vector3.one * (float)param[0];
            }
            AttackedPS.Play();
            GetComponent<Player_Controller_Ship>().additionalForce = (Vector3)param[1];

            ShakeCamera((float)param[0]);

            if (GameManager.GetInstance().GameStarted)
            {
                health -= (float)param[0];
                GetComponent<Player_UI_Ship>().UpdateHealth(health / maxHealth);


                BattleRoyalGameManager battleRoyalGameManager = FindObjectOfType<BattleRoyalGameManager>();
                if (battleRoyalGameManager && param.Length > 2)
                {
                    if (PhotonNetwork.IsMasterClient)
                        RoomData.GetInstance().SetCurrScore(PhotonView.Find((int)param[2]).OwnerActorNr, (float)param[0]);
                }


                if (health <= 0)
                {
                    GetComponent<Player_Controller_Ship>().deadTime = Time.time;
                    GameManager.GetInstance().Observe(0);

                    if(GetComponent<PhotonView>().IsMine)
                        PhotonNetwork.Destroy(this.gameObject);
                }
            }
        }
    }
    public void ShakeCamera(float _force)
    {
        impulseSource.GenerateImpulse(_force);
    }

    public int GetLastSailIndex()
    {
        int index = -1;
        for (int i = 0; i < mySails.Count; i++)
        {
            if (mySails[i] == null)
            {
                index = i;
                break;
            }
        }
        return index;
    }
    public int GetLastAutoCannonIndex()
    {
        int index = -1;
        for (int i = 0; i < myAutoCannons.Count; i++)
        {
            if (myAutoCannons[i] == null)
            {
                index = i;
                break;
            }
        }
        return index;
    }
    public int GetLastmySpecialCannonsIndex()
    {
        int index = -1;
        for (int i = 0; i < mySpecialCannons.Count; i++)
        {
            if (mySpecialCannons[i] == null)
            {
                index = i;
                break;
            }
        }
        return index;
    }

    public void GetSupply(SupplyType _supplyType, int _supplyIndex)
    {
        PhotonView pv = GetComponent<PhotonView>();
        int spotIndex = -1;
        switch (_supplyType)
        {
            case SupplyType.Sail:
                spotIndex = GetLastSailIndex();
                if(spotIndex>=0)
                {
                    pv.RPC("EquipSail", RpcTarget.AllBuffered, spotIndex, _supplyIndex);
                }
                break;
            case SupplyType.Cannon:
                spotIndex = GetLastAutoCannonIndex();
                if (spotIndex >= 0)
                {
                    pv.RPC("EquipCannon", RpcTarget.AllBuffered, spotIndex, _supplyIndex);
                }
                break;
            case SupplyType.SpecialCannon:
                spotIndex = GetLastmySpecialCannonsIndex();
                if (spotIndex >= 0)
                {
                    pv.RPC("EquipSpecialCannon", RpcTarget.AllBuffered, spotIndex, _supplyIndex);
                }
                break;
        }
    }

    [PunRPC]
    public void EquipSail(int _spotIndex, int _sailIndex)
    {
        Player_Controller_Ship myShip = GetComponent<Player_Controller_Ship>();
        if (_spotIndex >= mySails.Count)
            return;

        if (_sailIndex == -1)
        {
            if (mySails[_spotIndex] != null)
            {
                myShip.MaxSpeed -= 5f;
                myShip.MoveSpeed -= 5f;
                Destroy(mySails[_spotIndex].gameObject);
            }
            return;
        }

        if (mySails[_spotIndex] != null)
        {
            myShip.MaxSpeed -= 5f;
            myShip.MoveSpeed -= 5f;
            Destroy(mySails[_spotIndex].gameObject);
        }

        myShip.MaxSpeed += 5f;
        myShip.MoveSpeed += 5f;

        GameObject tmpCannon = null;
        tmpCannon = Instantiate(Resources.Load("Sail_" + _sailIndex) as GameObject, Vector3.zero, Quaternion.identity);

        tmpCannon.transform.SetParent(SailSpots[_spotIndex]);
        tmpCannon.transform.localPosition = Vector3.zero;
        tmpCannon.transform.localScale = Vector3.one;
        tmpCannon.transform.localRotation = Quaternion.identity;
        mySails[_spotIndex] = tmpCannon;
    }

    [PunRPC]
    public void EquipCannon(int _spotIndex, int _cannonIndex)
    {
        if (_spotIndex >= AutoCannonSpots.Count)
            return;

        if (_cannonIndex == -1)
        {
            if (myAutoCannons[_spotIndex] != null)
            {
                myAutoCannons[_spotIndex].UnEquipCannon();
            }
            return;
        }

        if (myAutoCannons[_spotIndex] == null)
        {
            GameObject tmpCannon = null;
            tmpCannon = Instantiate(Resources.Load("AutoCannon") as GameObject, Vector3.zero, Quaternion.identity);

            tmpCannon.transform.SetParent(AutoCannonSpots[_spotIndex]);
            tmpCannon.transform.localPosition = Vector3.zero;
            tmpCannon.transform.localScale = Vector3.one;
            tmpCannon.transform.localRotation = Quaternion.identity;
            myAutoCannons[_spotIndex] = tmpCannon.GetComponent<Cannon>();

            if(PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("GameModeIndex"))
                tmpCannon.GetComponent<AutoCannon>().Initialize(this,_spotIndex, int.Parse((string)PhotonNetwork.CurrentRoom.CustomProperties["GameModeIndex"]));
            else
                tmpCannon.GetComponent<AutoCannon>().Initialize(this, _spotIndex, -1);
        }

        ChangeCannonType(_spotIndex, _cannonIndex, true);

        if (photonView.IsMine)
        {
            CombatManager combatManager = FindObjectOfType<CombatManager>();
            if (myAutoCannons[_spotIndex] != null)
            {
                myAutoCannons[_spotIndex].tmpJoyStick = combatManager.joySticks[_spotIndex];
            }
        }
    }

    [PunRPC]
    public void EquipSpecialCannon(int _spotIndex, int _cannonIndex)
    {
        if (_spotIndex >= SpecialCannonSpots.Count)
            return;

        if (_cannonIndex == -1)
        {
            if (mySpecialCannons[_spotIndex] != null)
                mySpecialCannons[_spotIndex].UnEquipCannon();
            return;
        }

        bool _active = true;
        if (mySpecialCannons[_spotIndex] == null)
        {
            GameObject tmpCannon = null;
            tmpCannon = Instantiate(Resources.Load("SpecialCannon") as GameObject, Vector3.zero, Quaternion.identity);
            tmpCannon.transform.SetParent(SpecialCannonSpots[_spotIndex]);
            tmpCannon.transform.localPosition = Vector3.zero;
            tmpCannon.transform.localScale = Vector3.one;
            tmpCannon.transform.localRotation = Quaternion.identity;
            mySpecialCannons[_spotIndex] = tmpCannon.GetComponent<Cannon>();
            tmpCannon.GetComponent<SpecialCannon>().Initialize(this, _spotIndex, RoomData.GetInstance().gameMode);
        }
        ChangeSpecialCannonType(_spotIndex, _cannonIndex, true);

        if (photonView.IsMine)
        {
            CombatManager combatManager = FindObjectOfType<CombatManager>();
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
                Vector3 impulse = collision.impulse;
                if (Vector3.Dot(collision.GetContact(0).normal, impulse) < 0f)
                    impulse *= -1f;

                collision.transform.GetComponent<PhotonView>().RPC("Attacked", RpcTarget.AllBuffered, new object[] {
                    5.0f
                    ,-1*impulse*3f,photonView.ViewID
                });
                this.transform.GetComponent<PhotonView>().RPC("Attacked", RpcTarget.AllBuffered, new object[] {
                    5.0f
                    ,impulse*3f,collision.transform.GetComponent<PhotonView>().ViewID
                });
            }
        }
    }
}
