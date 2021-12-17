using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;

public class SpecialCannon : Cannon
{
    public enum SpecialCannonType
    {
        Hook,
        Shark,
        OakBarrel
    }
    public SpecialCannonType mySpecialCannonType;

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();
        InitializeBullet();

        if (Input.GetKeyDown(KeyCode.O))
        {
            mySpecialCannonType = (SpecialCannonType)((int)(mySpecialCannonType+1) % 3);
        }

        if (attackingState > 0)
        {
            launchingBullet();
        }
    }

    protected override void InitializeBullet()
    {
        if (tmpInput.magnitude <= 0)
            return;

        if (attackingState == 0)
        {
            switch (mySpecialCannonType)
            {
                case SpecialCannonType.Hook:
                    if (currCoolTime <= 0)
                    {
                        attackingState = 1;
                    }
                    break;
                case SpecialCannonType.Shark:
                    if (currCoolTime <= 0)
                    {
                        attackingState = 1;
                    }
                    break;
                case SpecialCannonType.OakBarrel:
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
        switch (mySpecialCannonType)
        {
            case SpecialCannonType.Hook:
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
                    LaunchHook(cursor.transform.position);
                }
                break;
            case SpecialCannonType.Shark:
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
                    LaunchShark(cursor.transform.position);
                }
                break;
            case SpecialCannonType.OakBarrel:
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

                    LaunchBarrel();
                }
                break;
        }
    }
    protected void LaunchHook(Vector3 targetPos)
    {
        GameObject tmp = PhotonNetwork.Instantiate("Cannon_Hook", this.transform.position, Quaternion.identity);
        tmp.GetComponent<Rigidbody>().velocity = (targetPos - this.transform.position).normalized * 300f;
        tmp.GetComponent<Hook>().myShip = GetComponentInParent<Player_Combat_Ship>();

        attackingState = 0;
        cannonDistance = 0;

        currCoolTime = maxCoolTime;
    }
    protected void LaunchShark(Vector3 targetPos)
    {
        GameObject tmp = PhotonNetwork.Instantiate("Cannon_Shark", this.transform.position, Quaternion.identity);
        tmp.GetComponent<Rigidbody>().velocity = (targetPos - this.transform.position).normalized * 300f;
        tmp.GetComponent<Shark>().myShip = GetComponentInParent<Player_Combat_Ship>();

        attackingState = 0;
        cannonDistance = 0;

        currCoolTime = maxCoolTime;
    }

    protected void LaunchBarrel()
    {
        GameObject tmp = PhotonNetwork.Instantiate("Cannon_Barrel", this.transform.position, Quaternion.identity);
        tmp.GetComponent<Barrel>().gravity = Vector3.up * gravity;
        tmp.GetComponent<Rigidbody>().velocity = CalculateLaunchData(Vector3.zero).initialVelocity;
        attackingState = 0;
        cannonDistance = 0;

        currCoolTime = maxCoolTime;
    }
}