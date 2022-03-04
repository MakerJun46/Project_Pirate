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
        Sniper,
        OakBarrel,
        Rain
    }
    public SpecialCannonType mySpecialCannonType;


    public override void Initialize(Player_Combat_Ship _myShip,int _spotIndex,int _gameModeIndex)
    {
        base.Initialize(_myShip, _spotIndex, _gameModeIndex);
    }

    protected override void Update()
    {
        base.Update();

        if (myShip.GetComponent<Photon.Pun.PhotonView>().IsMine)
        {
            InitializeBullet();

            if (attackingState > 0)
            {
                launchingBullet();
            }
        }
    }

    public override void ChangeCannonType(int _typeIndex, bool _isSet)
    {
        if (_isSet)
            mySpecialCannonType = (SpecialCannonType)(_typeIndex);
        else
            mySpecialCannonType = (SpecialCannonType)(((int)mySpecialCannonType + _typeIndex) % 4);
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
                    if (currCoolTime <=0)
                    {
                        attackingState = 1;
                    }
                    break;
                case SpecialCannonType.Sniper:
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
                case SpecialCannonType.Rain:
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
                    ChargeCannon(-1,30);

                    lrs[0].enabled = true;
                    int resolution = 2;
                    lrs[0].positionCount = resolution;
                    lrs[0].SetPosition(0, this.transform.position);
                    lrs[0].SetPosition(1, cursor.transform.position);
                }
                else if (Input.GetMouseButtonUp(0) && attackingState == 2)
                {
                    lrs[0].enabled = false;
                    LaunchHook(cursor.transform.position);
                }
                break;
            case SpecialCannonType.Rain:
                if (attackingState > 0 && tmpInput.magnitude > 0.1f)
                {
                    cursor.transform.localScale = Vector3.one *6f;
                    ChargeCannon(20,80);
                }
                else if (Input.GetMouseButtonUp(0) && attackingState == 2)
                {
                    LaunchRain();
                    cursor.transform.localScale = Vector3.one;
                }
                break;
            case SpecialCannonType.Sniper:
                if (attackingState > 0 && tmpInput.magnitude > 0.1f)
                {
                    ChargeCannon(20,80);
                }
                else if (Input.GetMouseButtonUp(0) && attackingState == 2)
                {
                    LaunchSniper();
                }
                break;
            case SpecialCannonType.OakBarrel:
                if (attackingState > 0 && tmpInput.magnitude > 0.1f)
                { 
                    ChargeCannon();

                    currCannonDistance += Time.deltaTime*20f;
                    currCannonDistance = Mathf.Clamp(currCannonDistance, 0, 12f);

                    lrs[0].enabled = true;
                    DrawPath();
                }
                else if (Input.GetMouseButtonUp(0) && attackingState == 2)
                {
                    lrs[0].enabled = false;

                    LaunchBarrel();
                }
                break;
        }
    }
    protected void LaunchHook(Vector3 targetPos)
    {
        GameObject tmp = PhotonNetwork.Instantiate("Cannon_Hook", this.transform.position, Quaternion.identity,0,new object[]{ PhotonNetwork.LocalPlayer.ActorNumber,targetPos,ShootVelocity});

        ResetAttackingState(10f);
        myShip.photonView.RPC("PlayAttackPS", RpcTarget.AllBuffered, spotIndex, true);
    }
    protected void LaunchSniper()
    {
        GameObject tmp = PhotonNetwork.Instantiate("CannonBall", cursor.transform.position + Vector3.up* 50f, Quaternion.identity,
            0, new object[] { 20.0f, 1.2f });
        tmp.GetComponent<CannonBall>().gravity = Vector3.up * gravity*4f;

        myShip.photonView.RPC("PlayAttackPS", RpcTarget.AllBuffered, spotIndex, true);
        ResetAttackingState(15f);
    }

    protected void LaunchBarrel()
    {
        GameObject tmp = PhotonNetwork.Instantiate("Cannon_Barrel", this.transform.position, Quaternion.identity, 0,
            new object[] { 
                PhotonNetwork.LocalPlayer.ActorNumber,
                CalculateLaunchData(Vector3.zero).initialVelocity,
                gravity*2f
            });

        myShip.photonView.RPC("PlayAttackPS", RpcTarget.AllBuffered, spotIndex, true);
        ResetAttackingState(10f);
    }


    protected void LaunchRain()
    {
        myShip.photonView.RPC("PlayAttackPS", RpcTarget.AllBuffered, spotIndex, true);
        StartCoroutine("RainCoroutine");
    }
    IEnumerator RainCoroutine()
    {
        yield return new WaitForSeconds(0.5f);

        for (int i = 0; i < 30; i++)
        {
            GameObject tmp = PhotonNetwork.Instantiate(
                "CannonBall",
                cursor.transform.position + new Vector3(Random.Range(-1, 1f) * 30f, Random.Range(50f, 60f), Random.Range(-1, 1f) * 30f),
                Quaternion.identity,
                0, new object[] { 1.0f, 0.5f });
            tmp.GetComponent<CannonBall>().gravity = Vector3.up * gravity;
            yield return new WaitForSeconds(0.05f);
        }

        ResetAttackingState(15f);
    }
}
