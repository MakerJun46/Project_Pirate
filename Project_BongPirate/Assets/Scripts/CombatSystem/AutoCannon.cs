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

    Transform targetCursor;

    protected override void Start()
    {
        fov = GetComponent<FieldOfView>();

        targetCursor = Instantiate(Resources.Load("Cursor") as GameObject, this.transform.position, Quaternion.identity).transform;
    }

    protected override void Update()
    {
        base.Update();

        InitializeBullet();
        if (attackingState > 0)
        {
            cursor = targetCursor;
            launchingBullet();
        }
        else
        {
            cursor = fov.currTarget;
            if (cursor != null && currCoolTime <= 0)
            {
                currCoolTime = maxCoolTime;
                LaunchTrajectory();
            }
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            myCannonType = (CannonType)((int)(myCannonType+1)%5);
        }
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
                        cannonDistance = 100;
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
                    attackingState = 2;

                    cannonDistance += Time.deltaTime;
                    cannonDistance = Mathf.Clamp(cannonDistance, 0, 5f);
                    attackAreaImage.enabled = true;
                    attackAreaImage.transform.localScale = Vector3.one * cannonDistance;

                    cursor.transform.position = this.transform.position + new Vector3(tmpInput.x, 0, tmpInput.y) * cannonDistance;

                    lrs[0].enabled = true;
                    DrawPath();
                }
                else if (Input.GetMouseButtonUp(0) && attackingState == 2)
                {
                    attackingState = 3;
                    lrs[0].enabled = false;
                    attackAreaImage.enabled = false;

                    LaunchTrajectory();
                }
                break;
            case CannonType.Straight:
                if (attackingState > 0 && tmpInput.magnitude > 0.1f)
                {
                    attackingState = 2;
                    cannonDistance = 5;
                    attackAreaImage.enabled = true;
                    attackAreaImage.transform.localScale = Vector3.one * cannonDistance;
                    cursor.transform.position = this.transform.position + new Vector3(tmpInput.x, 0, tmpInput.y) * cannonDistance;
                    lrs[0].enabled = true;
                    int resolution = 2;
                    lrs[0].positionCount = resolution;
                    lrs[0].SetPosition(0, this.transform.position);
                    lrs[0].SetPosition(1, cursor.transform.position);
                }
                else if (Input.GetMouseButtonUp(0) && attackingState == 2)
                {
                    attackingState = 3;
                    lrs[0].enabled = false;
                    attackAreaImage.enabled = false;
                    LaunchStraight(cursor.transform.position);
                }
                break;
            case CannonType.ThreeWay:
                if (attackingState > 0 && tmpInput.magnitude > 0.1f)
                {
                    attackingState = 2;
                    cannonDistance = 5;
                    attackAreaImage.enabled = true;
                    attackAreaImage.transform.localScale = Vector3.one * cannonDistance;
                    cursor.transform.position = this.transform.position + new Vector3(tmpInput.x, 0, tmpInput.y) * cannonDistance;
                    int resolution = 2;
                    for (int i = 0; i <= 2; i++)
                    {
                        lrs[i].enabled = true;
                        lrs[i].positionCount = resolution;
                        lrs[i].SetPosition(0, this.transform.position);
                        lrs[i].SetPosition(1, this.transform.position + (Quaternion.AngleAxis(30 * (i - 1), Vector3.up) * (cursor.transform.position - this.transform.position)));
                    }
                }
                else if (Input.GetMouseButtonUp(0) && attackingState == 2)
                {
                    attackingState = 3;
                    attackAreaImage.enabled = false;
                    for (int i = 0; i <= 2; i++)
                    {
                        lrs[i].enabled = false;
                        LaunchStraight(this.transform.position + (Quaternion.AngleAxis(30 * (i - 1), Vector3.up) * (cursor.transform.position - this.transform.position)));
                    }
                }
                break;
            case CannonType.Rain:
                if (attackingState > 0 && tmpInput.magnitude > 0.1f)
                {
                    attackingState = 2;
                    attackAreaImage.enabled = true;

                    attackAreaImage.transform.localScale = Vector3.one * cannonDistance;
                    cursor.transform.position = this.transform.position + new Vector3(tmpInput.x, 0, tmpInput.y) * 5f;
                    attackAreaImage.transform.position = cursor.transform.position;
                }
                else if (Input.GetMouseButtonUp(0) && attackingState == 2)
                {
                    attackingState = 3;
                    attackAreaImage.enabled = false;
                    attackAreaImage.transform.localPosition = Vector3.zero;
                    LaunchRain();
                }
                break;
            case CannonType.Soybean:
                if (attackingState > 0 && tmpInput.magnitude > 0.1f)
                {
                    attackingState = 2;

                    cannonDistance += Time.deltaTime;
                    cannonDistance = Mathf.Clamp(cannonDistance, 0, 5f);
                    attackAreaImage.enabled = true;
                    attackAreaImage.transform.localScale = Vector3.one * cannonDistance;

                    cursor.transform.position = this.transform.position + new Vector3(tmpInput.x, 0, tmpInput.y) * cannonDistance;

                    lrs[0].enabled = true;
                    DrawPath();
                }
                else if (Input.GetMouseButtonUp(0) && attackingState == 2)
                {
                    attackingState = 3;
                    lrs[0].enabled = false;
                    attackAreaImage.enabled = false;

                    LaunchSoybean();
                }
                break;
        }
    }

    protected void LaunchStraight(Vector3 targetPos)
    {
        GameObject tmp = PhotonNetwork.Instantiate("CannonBall", this.transform.position, Quaternion.identity);
        ball = tmp.GetComponent<Rigidbody>();
        ball.GetComponent<CannonBall>().gravity = Vector3.zero;
        ball.velocity = (targetPos - this.transform.position).normalized * 300f;
        attackingState = 0;
        cannonDistance = 0;

        currCoolTime = maxCoolTime;
    }
    protected void LaunchTrajectory()
    {
        GameObject tmp = PhotonNetwork.Instantiate("CannonBall", this.transform.position, Quaternion.identity);
        ball = tmp.GetComponent<Rigidbody>();
        ball.GetComponent<CannonBall>().gravity = Vector3.up * gravity;
        ball.velocity = CalculateLaunchData(Vector3.zero).initialVelocity;
        attackingState = 0;
        cannonDistance = 0;

        currCoolTime = maxCoolTime;
    }

    protected void LaunchRain()
    {
        for (int i = 0; i < 20; i++)
        {
            GameObject tmp = PhotonNetwork.Instantiate("CannonBall", cursor.transform.position + new Vector3(Random.value * cannonDistance, Random.Range(500, 1000f), Random.value * cannonDistance), Quaternion.identity);
            tmp.transform.localScale *= 0.5f;
            ball = tmp.GetComponent<Rigidbody>();
            ball.GetComponent<CannonBall>().gravity = Vector3.up * gravity;
        }
        attackingState = 0;
        cannonDistance = 0;

        currCoolTime = maxCoolTime;
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
        attackingState = 0;
        cannonDistance = 0;
        currCoolTime = maxCoolTime;
    }
}
