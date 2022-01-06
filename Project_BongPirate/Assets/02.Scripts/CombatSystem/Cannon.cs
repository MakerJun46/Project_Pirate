using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Cannon : MonoBehaviour
{
    protected Player_Combat_Ship myShip;

    public AttackJoyStick tmpJoyStick;

    protected Vector2 tmpInput;

    protected int attackingState = 0;

    public Transform cursor;
    public float height = 25;
    public float gravity = -18;
    
    [SerializeField] protected Image attackAreaImage;
    public float currCannonDistance = 0;

    [SerializeField] protected float maxCoolTime = 2f;
    protected float currCoolTime = 0f;

    [SerializeField] protected ParticleSystem AttackPS;


    [SerializeField] LayerMask groundLayer;
    [SerializeField] protected List<LineRenderer> lrs;


    public virtual void Initialize(Player_Combat_Ship _myShip)
    {
        myShip = _myShip;
        if (myShip.GetComponent<Photon.Pun.PhotonView>().IsMine)
        {
            cursor = Instantiate(Resources.Load("Cursor") as GameObject, this.transform.position, Quaternion.identity).transform;
        }
        else
        {

        }
    }

    public void UnEquipCannon()
    {
        if(cursor)
            Destroy(cursor.gameObject);
        Destroy(this.gameObject);
    }


    protected virtual void Update()
    {
        if (myShip.GetComponent<Photon.Pun.PhotonView>().IsMine)
        {
            tmpInput = tmpJoyStick.GetJoyStickInput();
            float inputMag = tmpInput.magnitude;
            Vector3 calcInput = (Camera.main.transform.forward * tmpInput.y + Camera.main.transform.right * tmpInput.x);
            print("JoystickInput__Yester : " + tmpInput);
            calcInput.y = 0;
            calcInput.Normalize();
            tmpInput = new Vector2(calcInput.x, calcInput.z)* inputMag;
            print("JoystickInpu : " + tmpInput);

            currCoolTime -= Time.deltaTime;
            tmpJoyStick.UpdateCoolTime(currCoolTime / maxCoolTime);
        }
    }


    protected virtual void InitializeBullet()
    {

    }
    protected virtual void launchingBullet()
    {

    }

    protected LaunchData CalculateLaunchData(Vector3 _offset)
    {
        print("Caluclate");
        float displacementY = cursor.position.y - transform.position.y;
        Vector3 displacementXZ = new Vector3(cursor.position.x - transform.position.x, 0, cursor.position.z - transform.position.z)+ _offset;
        float time = Mathf.Sqrt(-2 * height / gravity) + Mathf.Sqrt(2 * (displacementY - height) / gravity);
        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * height);
        Vector3 velocityXZ = displacementXZ / time;

        return new LaunchData(velocityXZ + velocityY * -Mathf.Sign(gravity), time);
    }

    protected void DrawPath()
    {
        LaunchData launchData = CalculateLaunchData(Vector3.zero);
        Vector3 previousDrawPoint = transform.position;

        int resolution = 30;
        lrs[0].positionCount = resolution;
        for (int i = 1; i < resolution; i++)
        {
            float simulationTime = i / (float)resolution * launchData.timeToTarget;
            Vector3 displacement = launchData.initialVelocity * simulationTime + Vector3.up * gravity * simulationTime * simulationTime / 2f;
            Vector3 drawPoint = transform.position + displacement;
            Debug.DrawLine(previousDrawPoint, drawPoint, Color.green);
            lrs[0].SetPosition(i - 1, previousDrawPoint);
            lrs[0].SetPosition(i, drawPoint);

            previousDrawPoint = drawPoint;
        }
    }

    protected struct LaunchData
    {
        public readonly Vector3 initialVelocity;
        public readonly float timeToTarget;

        public LaunchData(Vector3 initialVelocity, float timeToTarget)
        {
            this.initialVelocity = initialVelocity;
            this.timeToTarget = timeToTarget;
        }

    }

    public virtual void ChangeCannonType(int _typeIndex, bool _isSet)
    {
    }
}