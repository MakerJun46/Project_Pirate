using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class AutoCannon : Cannon
{
    public enum CannonType
    {
        Trajectory,
        Straight,
        ThreeWay,
        Soybean
    }
    public CannonType myCannonType;

    protected Rigidbody ball;
    FieldOfView fov;

    public override void Initialize(Player_Combat_Ship _myShip)
    {
        myShip = _myShip;
        if (myShip.GetComponent<Photon.Pun.PhotonView>().IsMine)
        {
            fov = GetComponent<FieldOfView>();
            cursor = Instantiate(Resources.Load("Cursor") as GameObject, this.transform.position, Quaternion.identity).transform;
        }
    }

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
                if (fov.currTarget)
                {
                    cursor.transform.position = fov.currTarget.position;
                    if (currCoolTime<=0 &&Vector3.Distance(fov.currTarget.position,this.transform.position) <= fov.viewRadius * currChargeAmount / maxChargetAmount)
                    {
                        currChargeAmount = 0;
                        currCoolTime = maxCoolTime;
                        switch (myCannonType)
                        {
                            case CannonType.Trajectory:
                                LaunchTrajectory();
                                break;
                            case CannonType.Straight:
                                LaunchStraight(1,cursor.transform.position);
                                break;
                            case CannonType.ThreeWay:
                                for (int i = 0; i < 3; i++)
                                {
                                    lrs[i].enabled = false;
                                    LaunchStraight(3,this.transform.position + (Quaternion.AngleAxis(30 * (i - 1), Vector3.up) * (cursor.transform.position - this.transform.position)));
                                }
                                break;
                            case CannonType.Soybean:
                                LaunchSoybean();
                                break;
                        }
                    }
                }
            }
        }
    }

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

        if (attackingState == 0)
        {
            switch (myCannonType)
            {
                case CannonType.Trajectory:
                    if (currCoolTime <= 0)
                    {
                        attackingState = 1;
                    }
                    break;
                case CannonType.Straight:
                    if (currCoolTime <= 0)
                    {
                        attackingState = 1;
                    }
                    break;
                case CannonType.ThreeWay:
                    if (currCoolTime <= 0)
                    {
                        attackingState = 1;
                    }
                    break;
                case CannonType.Soybean:
                    if (currCoolTime <= 0)
                    {
                        attackingState = 1;
                    }
                    break;
            }
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
                else if (Input.GetMouseButtonUp(0) && attackingState == 2)
                {
                    lrs[0].enabled = false;

                    LaunchTrajectory();
                }
                break;
            case CannonType.Straight:
                if (attackingState > 0 && tmpInput.magnitude > 0.1f)
                {
                    ChargeCannon(-1,80);
                    lrs[0].enabled = true;
                    int resolution = 2;
                    lrs[0].positionCount = resolution;
                    lrs[0].SetPosition(0, this.transform.position);
                    lrs[0].SetPosition(1, cursor.transform.position);
                }
                else if (Input.GetMouseButtonUp(0) && attackingState == 2)
                {
                    lrs[0].enabled = false;
                    LaunchStraight(0,cursor.transform.position);
                }
                break;
            case CannonType.ThreeWay:
                if (attackingState > 0 && tmpInput.magnitude > 0.1f)
                {
                    ChargeCannon(-1,80);
                    int resolution = 2;
                    for (int i = 0; i < 3; i++)
                    {
                        lrs[i].enabled = true;
                        lrs[i].positionCount = resolution;
                        lrs[i].SetPosition(0, this.transform.position);
                        lrs[i].SetPosition(1, this.transform.position + (Quaternion.AngleAxis(30 * (i - 1), Vector3.up) * (cursor.transform.position - this.transform.position)));
                    }
                }
                else if (Input.GetMouseButtonUp(0) && attackingState == 2)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        lrs[i].enabled = false;
                        LaunchStraight(3,this.transform.position + (Quaternion.AngleAxis(30 * (i - 1), Vector3.up) * (cursor.transform.position - this.transform.position)));
                    }
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
                else if (Input.GetMouseButtonUp(0) && attackingState == 2)
                {
                    lrs[0].enabled = false;

                    LaunchSoybean();
                }
                break;
        }
    }
    
    protected void LaunchTrajectory()
    {
        GameObject tmp = PhotonNetwork.Instantiate("CannonBall", this.transform.position, Quaternion.identity,0, new object[] { 10.0f, 1.0f });
        ball = tmp.GetComponent<Rigidbody>();
        ball.GetComponent<CannonBall>().gravity = Vector3.up * gravity;
        ball.velocity = CalculateLaunchData(Vector3.zero).initialVelocity;
        AttackPS.Play(true);
        ResetAttackingState(5f);
    }


    protected void LaunchSoybean()
    {
        for (int i = 0; i < 10; i++)
        {
            GameObject tmp = PhotonNetwork.Instantiate("CannonBall", this.transform.position, Quaternion.identity, 0, new object[] {2.0f ,0.3f });
            ball = tmp.GetComponent<Rigidbody>();
            ball.GetComponent<CannonBall>().gravity = Vector3.up * gravity;
            ball.velocity = CalculateLaunchData(new Vector3(Random.Range(-1f, 1f), Random.Range(-0.2f,0.2f), Random.Range(-1f, 1f)) * 10f).initialVelocity;
        }
        AttackPS.Play(true);
        ResetAttackingState(7.5f);
    }

    protected void LaunchStraight(int divided ,Vector3 targetPos)
    {
        float damage=8f;
        float scale=0.8f;
        if (divided > 1)
        {
            damage = 2f;
            scale = 0.5f;
        }
        GameObject tmp = PhotonNetwork.Instantiate("CannonBall", this.transform.position, Quaternion.identity, 0, new object[] { damage,scale});
        ball = tmp.GetComponent<Rigidbody>();
        ball.GetComponent<CannonBall>().gravity = Vector3.zero;
        ball.velocity = (targetPos - this.transform.position).normalized * ShootVelocity;
        AttackPS.Play(true);
        ResetAttackingState((divided>1)? 6f:4f);
    }
}
