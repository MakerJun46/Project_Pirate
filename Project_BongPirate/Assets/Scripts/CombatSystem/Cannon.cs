using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Cannon : MonoBehaviour
{
    public AttackJoyStick tmpJoyStick;
    protected Vector2 tmpInput;

    protected int attackingState = 0;

    protected Transform cursor;
    public float h = 25;
    public float gravity = -18;
    
    [SerializeField] protected Image attackAreaImage;
    public float cannonDistance = 0;

    [SerializeField] protected float maxCoolTime = 5f;
    protected float currCoolTime = 0f;
    [SerializeField] protected Image coolTimeImage;


    [SerializeField] LayerMask groundLayer;
    [SerializeField] protected List<LineRenderer> lrs;

    private void OnDestroy()
    {
        Destroy(cursor.gameObject);
    }

    protected virtual void Start()
    {
        cursor = Instantiate(Resources.Load("Cursor") as GameObject, this.transform.position, Quaternion.identity).transform;
    }

    protected virtual void Update()
    {
        tmpInput = tmpJoyStick.GetJoyStickInput();

        currCoolTime -= Time.deltaTime;
        coolTimeImage.fillAmount = currCoolTime / maxCoolTime;
    }


    protected virtual void InitializeBullet()
    {

    }
    protected virtual void launchingBullet()
    {

    }

    protected LaunchData CalculateLaunchData(Vector3 _offset)
    {
        float displacementY = cursor.position.y - transform.position.y;
        Vector3 displacementXZ = new Vector3(cursor.position.x - transform.position.x, 0, cursor.position.z - transform.position.z)+ _offset;
        float time = Mathf.Sqrt(-2 * h / gravity) + Mathf.Sqrt(2 * (displacementY - h) / gravity);
        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * h);
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
}