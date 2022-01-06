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
        Rain,
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
        else
        {

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
                    if (currCoolTime <= 0)
                    {
                        currCoolTime = maxCoolTime;
                        LaunchTrajectory();
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
            myCannonType = (CannonType)(((int)myCannonType + _typeIndex) % 5);
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
                case CannonType.Rain:
                    if (currCoolTime <= 0)
                    {
                        attackingState = 1;
                        currCannonDistance = 100;
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

                    currCannonDistance += Time.deltaTime;
                    currCannonDistance = Mathf.Clamp(currCannonDistance, 0, 5f);

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
                    ChargeCannon(5);
                    lrs[0].enabled = true;
                    int resolution = 2;
                    lrs[0].positionCount = resolution;
                    lrs[0].SetPosition(0, this.transform.position);
                    lrs[0].SetPosition(1, cursor.transform.position);
                }
                else if (Input.GetMouseButtonUp(0) && attackingState == 2)
                {
                    lrs[0].enabled = false;
                    LaunchStraight(cursor.transform.position);
                }
                break;
            case CannonType.ThreeWay:
                if (attackingState > 0 && tmpInput.magnitude > 0.1f)
                {
                    ChargeCannon(5);
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
                        LaunchStraight(this.transform.position + (Quaternion.AngleAxis(30 * (i - 1), Vector3.up) * (cursor.transform.position - this.transform.position)));
                    }
                }
                break;
            case CannonType.Rain:
                if (attackingState > 0 && tmpInput.magnitude > 0.1f)
                {
                    ChargeCannon(5);
                }
                else if (Input.GetMouseButtonUp(0) && attackingState == 2)
                {
                    LaunchRain();
                }
                break;
            case CannonType.Soybean:
                if (attackingState > 0 && tmpInput.magnitude > 0.1f)
                {
                    ChargeCannon();

                    currCannonDistance += Time.deltaTime;
                    currCannonDistance = Mathf.Clamp(currCannonDistance, 0, 5f);

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
    
    private void ChargeCannon(float _currCannonDistance=-1f)
    {
        attackingState = 2;

        if (_currCannonDistance > 0)
            currCannonDistance = _currCannonDistance;

        attackAreaImage.enabled = true;
        attackAreaImage.transform.localScale = Vector3.one * currCannonDistance;

        cursor.transform.position = this.transform.position + new Vector3(tmpInput.x, 0, tmpInput.y) * currCannonDistance;
    }

    protected void LaunchStraight(Vector3 targetPos)
    {
        GameObject tmp = PhotonNetwork.Instantiate("CannonBall", this.transform.position, Quaternion.identity);
        ball = tmp.GetComponent<Rigidbody>();
        ball.GetComponent<CannonBall>().gravity = Vector3.zero;
        ball.velocity = (targetPos - this.transform.position).normalized * 300f;
        AttackPS.Play(true);
        ResetAttackingState();
    }
    protected void LaunchTrajectory()
    {
        GameObject tmp = PhotonNetwork.Instantiate("CannonBall", this.transform.position, Quaternion.identity);
        ball = tmp.GetComponent<Rigidbody>();
        ball.GetComponent<CannonBall>().gravity = Vector3.up * gravity;
        ball.velocity = CalculateLaunchData(Vector3.zero).initialVelocity;
        AttackPS.Play(true);
        ResetAttackingState();
    }

    protected void LaunchRain()
    {
        for (int i = 0; i < 20; i++)
        {
            GameObject tmp = PhotonNetwork.Instantiate("CannonBall", cursor.transform.position + new Vector3(Random.Range(-1,1f) * currCannonDistance, Random.Range(50, 100f), Random.Range(-1, 1f) * currCannonDistance), Quaternion.identity);
            tmp.transform.localScale *= 0.75f;
            ball = tmp.GetComponent<Rigidbody>();
            ball.GetComponent<CannonBall>().gravity = Vector3.up * gravity;
        }
        AttackPS.Play(true);
        ResetAttackingState();
    }

    protected void LaunchSoybean()
    {
        for (int i = 0; i < 10; i++)
        {
            GameObject tmp = PhotonNetwork.Instantiate("CannonBall", this.transform.position, Quaternion.identity);
            tmp.transform.localScale *= 0.2f;
            ball = tmp.GetComponent<Rigidbody>();
            ball.GetComponent<CannonBall>().gravity = Vector3.up * gravity;
            ball.velocity = CalculateLaunchData(new Vector3(Random.value, Random.value, Random.value) * 20f).initialVelocity;
        }
        AttackPS.Play(true);
        ResetAttackingState();
    }

    private void ResetAttackingState()
    {
        attackingState = 3;
        attackAreaImage.enabled = false;

        attackingState = 0;
        currCannonDistance = 0;
        currCoolTime = maxCoolTime;
    }
}
