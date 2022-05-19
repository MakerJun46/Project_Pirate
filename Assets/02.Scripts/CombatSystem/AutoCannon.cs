using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class AutoCannon : Cannon
{
    #region Variables & Initializer
    

    public enum CannonType
    {
        Trajectory,
        Soybean,
        Straight,
        ThreeWay
    }
    public CannonType myCannonType;

    protected Rigidbody ball;
    public override void Initialize(Player_Combat_Ship _myShip, int _spotIndex, int _gameModeIndex)
    {
        myShip = _myShip;
        spotIndex = _spotIndex;
        gameMode = (GameMode)_gameModeIndex;
        if (myShip.GetComponent<Photon.Pun.PhotonView>().IsMine)
        {
            fov = GetComponent<FieldOfView>();
            cursor = Instantiate(Resources.Load("Cursor") as GameObject, this.transform.position, Quaternion.identity).transform;
            cursor.gameObject.SetActive(false);
        }
        CannonLayer cannonLayer = CannonLayers.Find(s => s.GameMode == gameMode);
        fov.targetMaskType = cannonLayer.layerType;
    }
    #endregion

    protected override void Update()
    {
        base.Update();

        if (myShip.GetComponent<Photon.Pun.PhotonView>().IsMine)
        {
            InitializeBullet();
            if (attackingState > 0)
            {
                //cursor = targetCursor;
                launchingBullet();
            }
            else
            {
                cursor.gameObject.SetActive(fov.currTarget != null);
                if (fov.currTarget)
                {
                    cursor.transform.position = new Vector3(fov.currTarget.position.x,0, fov.currTarget.position.z);
                    if (currCoolTime<=0 &&Vector3.Distance(fov.currTarget.position,this.transform.position) <= fov.viewRadius * currChargeAmount / maxChargetAmount)
                    {
                        currChargeAmount = 0;
                        currCoolTime = maxCoolTime;
                        switch (myCannonType)
                        {
                            case CannonType.Trajectory:
                                LaunchTrajectory();
                                break;
                            case CannonType.Soybean:
                                LaunchSoybean();
                                break;
                            case CannonType.Straight:
                                LaunchStraight(1, cursor.transform.position);
                                break;
                            case CannonType.ThreeWay:
                                for (int i = 0; i < 3; i++)
                                {
                                    lrs[i].enabled = false;
                                    LaunchStraight(3, this.transform.position + (Quaternion.AngleAxis(30 * (i - 1), Vector3.up) * (cursor.transform.position - this.transform.position)));
                                }
                                break;
                        }
                    }
                }
            }
        }
    }

    #region Cannon
    public override void ChangeCannonType(int _typeIndex,bool _isSet)
    {
        if(_isSet)
            myCannonType = (CannonType)(_typeIndex);
        else
            myCannonType = (CannonType)(((int)myCannonType + _typeIndex) % 4);
    }


    protected override void InitializeBullet()
    {
        if (tmpInput.magnitude <= 0)
            return;

        // 입력이 존재하는데, coolTime이 다 지나갔으면, 
        if (attackingState == 0 && currCoolTime <= 0)
        {
            attackingState = AttackState.Aiming;
        }
    }

    protected override void launchingBullet()
    {
        switch (myCannonType)
        {
            case CannonType.Trajectory:
                if (attackingState > 0 && tmpInput.magnitude > 0.1f)
                {
                    ChargeCannon();

                    currCannonDistance += Time.deltaTime*20f;
                    currCannonDistance = Mathf.Clamp(currCannonDistance, 0,100f);

                    lrs[0].enabled = true;
                    DrawPath();
                }
                else if (Input.GetMouseButtonUp(0) && attackingState == AttackState.Launcing)
                {
                    lrs[0].enabled = false;

                    LaunchTrajectory();
                }
                break;
            case CannonType.Soybean:
                if (attackingState > 0 && tmpInput.magnitude > 0.1f)
                {
                    ChargeCannon();

                    currCannonDistance += Time.deltaTime*20f;
                    currCannonDistance = Mathf.Clamp(currCannonDistance, 0, 100f);

                    lrs[0].enabled = true;
                    DrawPath();
                }
                else if (Input.GetMouseButtonUp(0) && attackingState == AttackState.Launcing)
                {
                    lrs[0].enabled = false;

                    LaunchSoybean();
                }
                break;
            case CannonType.Straight:
                if (attackingState > 0 && tmpInput.magnitude > 0.1f)
                {
                    ChargeCannon(-1, 80);
                    lrs[0].enabled = true;
                    int resolution = 2;
                    lrs[0].positionCount = resolution;
                    lrs[0].SetPosition(0, this.transform.position);
                    lrs[0].SetPosition(1, cursor.transform.position);
                }
                else if (Input.GetMouseButtonUp(0) && attackingState == AttackState.Launcing)
                {
                    lrs[0].enabled = false;
                    LaunchStraight(0, cursor.transform.position);
                }
                break;
            case CannonType.ThreeWay:
                if (attackingState > 0 && tmpInput.magnitude > 0.1f)
                {
                    ChargeCannon(-1, 80);
                    int resolution = 2;
                    for (int i = 0; i < 3; i++)
                    {
                        lrs[i].enabled = true;
                        lrs[i].positionCount = resolution;
                        lrs[i].SetPosition(0, this.transform.position);
                        lrs[i].SetPosition(1, this.transform.position + (Quaternion.AngleAxis(30 * (i - 1), Vector3.up) * (cursor.transform.position - this.transform.position)));
                    }
                }
                else if (Input.GetMouseButtonUp(0) && attackingState == AttackState.Launcing)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        lrs[i].enabled = false;
                        LaunchStraight(3, this.transform.position + (Quaternion.AngleAxis(30 * (i - 1), Vector3.up) * (cursor.transform.position - this.transform.position)));
                    }
                }
                break;
        }
    }

    
    protected void LaunchTrajectory()
    {
        GameObject tmp = PhotonNetwork.Instantiate("CannonBall", this.transform.position, Quaternion.identity,0, new object[] { 30.0f, 1.0f });
        ball = tmp.GetComponent<Rigidbody>();
        ball.GetComponent<CannonBall>().gravity = Vector3.up * gravity;

        Vector3 targetPos = Vector3.zero;
        if (fov.currTarget)
        {
            if(fov.currTarget.GetComponent<Rigidbody>())
                targetPos = fov.currTarget.GetComponent<Rigidbody>().velocity;
            else
                targetPos = Vector3.zero;
        }
        ball.velocity = CalculateLaunchData(targetPos).initialVelocity;
        //OptionSettingManager.GetInstance().Play("FireCannon", true);
        myShip.photonView.RPC("PlayAttackPS",RpcTarget.AllBuffered,spotIndex, false);
        ResetAttackingState(5f);
    }


    protected void LaunchSoybean()
    {
        for (int i = 0; i < 10; i++)
        {
            GameObject tmp = PhotonNetwork.Instantiate("CannonBall", this.transform.position, Quaternion.identity, 0, new object[] {8.0f ,0.3f });
            ball = tmp.GetComponent<Rigidbody>();
            ball.GetComponent<CannonBall>().gravity = Vector3.up * gravity;

            Vector3 targetPos = new Vector3(Random.Range(-1f, 1f), Random.Range(0f, 0.4f), Random.Range(-1f, 1f)) * 8f;
            if (fov.currTarget)
            {
                targetPos += fov.currTarget.GetComponent<Rigidbody>().velocity;
            }
            ball.velocity = CalculateLaunchData(targetPos).initialVelocity;
        }
        //OptionSettingManager.GetInstance().Play("FireCannon", true);
        myShip.photonView.RPC("PlayAttackPS", RpcTarget.AllBuffered, spotIndex, false);
        ResetAttackingState(7.5f);
    }

    protected void LaunchStraight(int divided ,Vector3 _targetPos)
    {
        float damage=20f;
        float scale=0.8f;
        if (divided > 1)
        {
            damage = 10f;
            scale = 0.5f;
        }
        Vector3 spawnPos = this.transform.position;
        spawnPos.y = 1.5f;
        GameObject tmp = PhotonNetwork.Instantiate("CannonBall", this.transform.position, Quaternion.identity, 0, new object[] { damage,scale});
        ball = tmp.GetComponent<Rigidbody>();
        ball.GetComponent<CannonBall>().gravity = Vector3.zero;

        Vector3 targetPos = Vector3.zero;
        if (fov.currTarget)
        {
            targetPos = fov.currTarget.GetComponent<Rigidbody>().velocity;
        }
        Vector3 dist = (_targetPos + targetPos - this.transform.position);
        dist.y = 0;
        ball.velocity = dist.normalized * ShootVelocity;
        //OptionSettingManager.GetInstance().Play("FireCannon", true);
        myShip.photonView.RPC("PlayAttackPS", RpcTarget.AllBuffered, spotIndex, false);
        ResetAttackingState((divided>1)? 6f:4f);
    }
#endregion
}
