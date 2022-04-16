using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

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

    float currLevelUpTime;
    [SerializeField] float levelUpCoolTime=10;
    private float remainLevelUpCount = 0;
    [SerializeField] GameObject LevelUpPanel;
    [SerializeField] Transform LevelUpBtnContainer;

    private void Update()
    {
        if (GameManager.GetInstance().GameStarted)
        {
            if (levelUpCoolTime >= 0)
            {
                currLevelUpTime += Time.deltaTime;
                if (currLevelUpTime >= levelUpCoolTime)
                {
                    currLevelUpTime -= levelUpCoolTime;
                    SetLevelUpCount(1);
                }
            }
        }
    }

    public void SetLevelUpCount(int _add)
    {
        if((_add>=0 && remainLevelUpCount == 0) || (_add < 0 && remainLevelUpCount>1))
        {
            LevelUp();
        }
        remainLevelUpCount += _add;
    }

    public void LevelUp()
    {
        LevelUpPanel.SetActive(true);

        if (PhotonNetwork.IsMasterClient == false)
        {
            List<Vector2> randomRoullet = new List<Vector2>();
            Player_Combat_Ship currShip = myShip;
            int spotIndex = currShip.GetLastSailIndex();
            if (spotIndex >= 0)
            {
                randomRoullet.Add(new Vector2(0, 0));
                randomRoullet.Add(new Vector2(0, 1));
            }
            spotIndex = currShip.GetLastAutoCannonIndex();
            if (spotIndex >= 0)
            {
                randomRoullet.Add(new Vector2(1, 0));
                randomRoullet.Add(new Vector2(1, 1));
                randomRoullet.Add(new Vector2(1, 2));
                randomRoullet.Add(new Vector2(1, 3));
            }
            if (myShip.GetComponent<Player_Controller_Ship>().upgradeIndex <= 1)
            {
                randomRoullet.Add(new Vector2(3, 0));
            }
            if (myShip.GetComponent<Player_Controller_Ship>().upgradeIndex >= 1)
            {
                spotIndex = currShip.GetLastmySpecialCannonsIndex();
                if (spotIndex >= 0)
                {
                    randomRoullet.Add(new Vector2(2, 0));
                    randomRoullet.Add(new Vector2(2, 1));
                    randomRoullet.Add(new Vector2(2, 2));
                    randomRoullet.Add(new Vector2(2, 3));
                }
            }


            for (int i = 0; i < LevelUpBtnContainer.childCount; i++)
            {
                GameObject levelUpBtn = LevelUpBtnContainer.GetChild(i).gameObject;


                if (randomRoullet.Count > 0)
                {
                    int selectIndex = Random.Range(0, randomRoullet.Count);
                    Vector2 selectedRoullet = randomRoullet[selectIndex];
                    randomRoullet.RemoveAt(selectIndex);

                    levelUpBtn.SetActive(true);
                    levelUpBtn.GetComponentInChildren<Text>().text = "";
                    levelUpBtn.GetComponent<Button>().onClick.RemoveAllListeners();


                    int levelUpIndex = (int)selectedRoullet.y;

                    switch ((int)selectedRoullet.x)
                    {
                        case 0:
                            levelUpBtn.GetComponentInChildren<Text>().text = "Get Sail " + levelUpIndex;
                            levelUpBtn.GetComponent<Button>().onClick.AddListener(() => EquipSail(currShip.GetLastSailIndex(), levelUpIndex));
                            break;
                        case 1:
                            levelUpBtn.GetComponentInChildren<Text>().text = "Get Cannon " + levelUpIndex;
                            levelUpBtn.GetComponent<Button>().onClick.AddListener(() => EquipCannon(currShip.GetLastAutoCannonIndex(), levelUpIndex));
                            break;
                        case 2:
                            levelUpBtn.GetComponentInChildren<Text>().text = "Get SpecialCannon " + levelUpIndex;
                            levelUpBtn.GetComponent<Button>().onClick.AddListener(() => EquipSpecialCannon(currShip.GetLastmySpecialCannonsIndex(), levelUpIndex));
                            break;
                        case 3:
                            levelUpBtn.GetComponentInChildren<Text>().text = "Upgrade";
                            levelUpBtn.GetComponent<Button>().onClick.AddListener(() => GameManager.GetInstance().TryUpgradeShip());
                            break;
                    }
                    levelUpBtn.GetComponent<Button>().onClick.AddListener(() => LevelUpPanel.SetActive(false));
                    levelUpBtn.GetComponent<Button>().onClick.AddListener(() => SetLevelUpCount(-1));
                }
                else
                {
                    levelUpBtn.SetActive(false);
                }
            }
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
