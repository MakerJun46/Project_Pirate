using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using Photon.Pun;
using Random = UnityEngine.Random;

public class Cannon : MonoBehaviourPun
{
    [System.Serializable]
    public struct CannonLayer
    {
        public GameMode GameMode;
        public int layerType;
    }

    public Player_Combat_Ship myShip;

    public AttackJoyStick tmpJoyStick;

    protected Vector2 tmpInput;

    protected enum AttackState { 
        Waiting,
        Aiming,
        Launcing
    }

    protected AttackState attackingState = AttackState.Waiting;

    public Transform cursor;
    public float height = 25;
    public float gravity = -9.81f;

    [SerializeField] protected float ShootVelocity = 50f;

    [SerializeField] protected Image attackAreaImage;
    public float currCannonDistance = 0;

    public float maxChargetAmount = 2f;
    public float currChargeAmount;

    public float maxCoolTime = 2f;
    public float currCoolTime;

    [SerializeField] protected ParticleSystem AttackPS;


    [SerializeField] LayerMask groundLayer;
    [SerializeField] protected List<LineRenderer> lrs;
    [SerializeField] private MeshRenderer[] cannonMeshes;
    [SerializeField] private Material GhostMat;

    protected int spotIndex;

    protected GameMode gameMode;
    Vector3 cursorAddPos;

    [SerializeField] protected FieldOfView fov;
    [SerializeField] protected List<CannonLayer> CannonLayers;
    public virtual void Initialize(Player_Combat_Ship _myShip,int _spotIndex,int _gameModeIndex)
    {
        myShip = _myShip;
        spotIndex = _spotIndex;
        gameMode = (GameMode)_gameModeIndex;
        if (myShip.GetComponent<Photon.Pun.PhotonView>().IsMine)
        {
            cursor = Instantiate(Resources.Load("Cursor") as GameObject, this.transform.position, Quaternion.identity).transform;
            cursor.gameObject.SetActive(false);
        }
    }

    public void UnEquipCannon()
    {
        if(cursor)
            Destroy(cursor.gameObject);
        Destroy(this.gameObject);
    }

    public void ChangeMaterialToGhost()
    {
        for(int i=0;i< cannonMeshes.Length; i++)
        {
            cannonMeshes[i].material = GhostMat;
        }
    }

    protected virtual void Update()
    {
        if (myShip.health <= 0)
        {
            ResetAttackingState(1000f);
            return;
        }

        if (myShip.GetComponent<Photon.Pun.PhotonView>().IsMine)
        {
            tmpInput = tmpJoyStick.GetJoyStickInput();
            //float inputMag = tmpInput.magnitude;
            Vector3 calcInput = (Camera.main.transform.forward * tmpInput.y + Camera.main.transform.right * tmpInput.x);
            calcInput.y = 0;
            calcInput.Normalize();
            tmpInput = new Vector2(calcInput.x, calcInput.z);

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

    public virtual void ChangeCannonType(int _typeIndex, bool _isSet)
    {
    }
    protected void ChargeCannon(float cursorSpeed = -1f,float _currCannonDistance = -1f)
    {
        cursor.gameObject.SetActive(true);
        attackingState = AttackState.Launcing;

        if (_currCannonDistance > 0)
            currCannonDistance = _currCannonDistance;

        attackAreaImage.enabled = true;
        attackAreaImage.transform.localScale = Vector3.one * currCannonDistance*0.2f;
        attackAreaImage.transform.position = new Vector3(attackAreaImage.transform.position.x, 0f, attackAreaImage.transform.position.z);

        if (cursorSpeed == -1)
            cursor.transform.position = new Vector3(transform.position.x,0, transform.position.z)+ new Vector3(tmpInput.x, 0, tmpInput.y) * currCannonDistance;
        else
        {
            cursorAddPos += new Vector3(tmpInput.x, 0, tmpInput.y) * cursorSpeed * Time.deltaTime;
            cursorAddPos = Vector3.ClampMagnitude(cursorAddPos, currCannonDistance);
            cursor.transform.position = new Vector3(transform.position.x, 0, transform.position.z) + cursorAddPos;
        }
    }
    protected void ResetAttackingState(float coolTime)
    {
        maxCoolTime = coolTime;
        currCoolTime = coolTime;
        //attackingState = 3;
        attackAreaImage.enabled = false;

        attackingState = 0;
        currCannonDistance = 0;
        currChargeAmount = 0;
        if(cursor)
            cursor.gameObject.SetActive(false);
    }

    protected LaunchData CalculateLaunchData(Vector3 _offset)
    {
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

    public void PlayAttackPS()
    {
        AttackPS.Play();
    }
}