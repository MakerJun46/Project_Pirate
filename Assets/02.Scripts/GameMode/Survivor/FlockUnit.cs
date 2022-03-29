using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlockUnit : MonoBehaviour
{
    [SerializeField] private float FOVAngle;
    [SerializeField] private float smoothDamp;
    [SerializeField] private float speed;
    [SerializeField] private Vector3[] directionsToCheckWhenAvoidingObstacles;

    private List<FlockUnit> cohesionNeighbours = new List<FlockUnit>();
    private List<FlockUnit> alignmentNeighbours = new List<FlockUnit>();
    private List<FlockUnit> avoidanceNeighbours = new List<FlockUnit>();
    private Flock assignedFlock;
    private Vector3 currentVelocity;
    private Vector3 currentObstacleAvoidanceVector;

    [SerializeField] LayerMask obstacleLayer;

    public void AssignFlock(Flock _flock)
    {
        assignedFlock = _flock;
    }

    public void InitializeSpeed(float speed)
    {
        this.speed = speed;
    }

    public void MoveUnit()
    {
        FindNeighbours();
        CalculateSpeed();
        Vector3 cohesionVector = CalculateCohesionVector() * assignedFlock._cohesionWeight;
        Vector3 avoidanceVector = CalculateAvoidanceVector() * assignedFlock._avoidanceWeight;
        Vector3 alignmentVector = CalculateAlignemnetVector() * assignedFlock._alignmentWeight;
        Vector3 boundsVector = CalculateBoundsVector() * assignedFlock._boundsWeight;
        Vector3 obstacleVector = CalculateObstacleVector() * assignedFlock._obstacleWeight;

        Vector3 yBoundsVector = Vector3.zero;
        if (this.transform.position.y >= assignedFlock.MinMaxYParam.y)
            yBoundsVector = Vector3.down * 10;
        else if (this.transform.position.y <= assignedFlock.MinMaxYParam.x)
            yBoundsVector = Vector3.up * 10;

        Vector3 moveVector = cohesionVector + avoidanceVector + cohesionVector + alignmentVector + boundsVector + obstacleVector + yBoundsVector;

        moveVector = Vector3.SmoothDamp(transform.forward, moveVector, ref currentVelocity, smoothDamp);

        moveVector = moveVector.normalized * speed;
        if (moveVector == Vector3.zero)
            moveVector = transform.forward;

        transform.forward = moveVector;
        transform.position += moveVector * Time.deltaTime;
    }

    private void FindNeighbours()
    {
        cohesionNeighbours.Clear();
        avoidanceNeighbours.Clear();
        alignmentNeighbours.Clear();
        FlockUnit[] allUnits = assignedFlock.allUnits;
        for(int i = 0; i < allUnits.Length; i++)
        {
            FlockUnit currentUnit = allUnits[i];
            if (currentUnit != this)
            {
                float currentNeighbourDistanceSqr = Vector3.SqrMagnitude(currentUnit.transform.position - transform.position);
                if (currentNeighbourDistanceSqr <= assignedFlock._cohesionDistance * assignedFlock._cohesionDistance && cohesionNeighbours.Count<=10)
                {
                    cohesionNeighbours.Add(currentUnit);
                }
                if (currentNeighbourDistanceSqr <= assignedFlock._avoidanceDistance * assignedFlock._avoidanceDistance && avoidanceNeighbours.Count <= 10)
                {
                    avoidanceNeighbours.Add(currentUnit);
                }
                if (currentNeighbourDistanceSqr <= assignedFlock._alignmentDistance * assignedFlock._alignmentDistance && alignmentNeighbours.Count <= 10)
                {
                    alignmentNeighbours.Add(currentUnit);
                }
            }
        }
    }

    private void CalculateSpeed()
    {
        if (cohesionNeighbours.Count == 0)
            return;
        speed = 0;
        for(int i = 0; i < cohesionNeighbours.Count; i++)
        {
            speed += cohesionNeighbours[i].speed;
        }
        speed /= cohesionNeighbours.Count;
        speed = Mathf.Clamp(speed, assignedFlock.speedRange.x, assignedFlock.speedRange.y);
    }

    private Vector3 CalculateCohesionVector()
    {
        Vector3 cohesionVector = Vector3.zero;
        if (cohesionNeighbours.Count == 0)
            return cohesionVector;

        int neighboursInFOV = 0;
        for (int i = 0; i < cohesionNeighbours.Count; i++)
        {
            if (IsInFOV(cohesionNeighbours[i].transform.position))
            {
                neighboursInFOV++;
                cohesionVector += cohesionNeighbours[i].transform.position;
            }
        }
        if (neighboursInFOV == 0)
            return cohesionVector;
        cohesionVector /= neighboursInFOV;
        cohesionVector -= transform.position;
        cohesionVector = cohesionVector.normalized;
        return cohesionVector;
    }

    private Vector3 CalculateAlignemnetVector()
    {
        Vector3 alignmentVector = transform.forward;
        if (alignmentNeighbours.Count == 0)
            return alignmentVector;

        int neighboursInFOV = 0;
        for (int i = 0; i < alignmentNeighbours.Count; i++)
        {
            if (IsInFOV(alignmentNeighbours[i].transform.position))
            {
                neighboursInFOV++;
                alignmentVector += alignmentNeighbours[i].transform.forward;
            }
        }
        if (neighboursInFOV == 0)
            return alignmentVector;
        alignmentVector /= neighboursInFOV;
        alignmentVector = alignmentVector.normalized;
        return alignmentVector;
    }
    private Vector3 CalculateAvoidanceVector()
    {
        Vector3 avoidanceVector = Vector3.zero;
        if (avoidanceNeighbours.Count == 0)
            return avoidanceVector;

        int neighboursInFOV = 0;
        for (int i = 0; i < avoidanceNeighbours.Count; i++)
        {
            if (IsInFOV(avoidanceNeighbours[i].transform.position))
            {
                neighboursInFOV++;
                avoidanceVector += (transform.position- avoidanceNeighbours[i].transform.position);
            }
        }
        if (neighboursInFOV == 0)
            return avoidanceVector;
        avoidanceVector /= neighboursInFOV;
        avoidanceVector = avoidanceVector.normalized;
        return avoidanceVector;
    }

    private Vector3 CalculateBoundsVector()
    {
        Vector3 offsetToCenter = assignedFlock.transform.position - transform.position;
        bool isNearCenter = offsetToCenter.magnitude >= assignedFlock._boundsDistance * 0.9f;
        return isNearCenter ? offsetToCenter.normalized : Vector3.zero;
    }
    private Vector3 CalculateObstacleVector()
    {
        Vector3 obstacleVector = Vector3.zero;
        RaycastHit hit;
        //Debug.DrawRay(transform.position, transform.forward * assignedFlock._obstacleDistance);
        if(Physics.Raycast(transform.position,transform.forward,out hit, assignedFlock._obstacleDistance, obstacleLayer))
        {
            obstacleVector = FindBestDirectionToAvoidObstacle();
        }
        else
        {
            currentObstacleAvoidanceVector = Vector3.zero;
        }
        return obstacleVector;
    }

    private Vector3 FindBestDirectionToAvoidObstacle()
    {
        if (currentObstacleAvoidanceVector != Vector3.zero)
        {
            RaycastHit hit;
            if(Physics.Raycast(transform.position,transform.forward,out hit, assignedFlock._obstacleDistance, obstacleLayer) == false)
            {
                return currentObstacleAvoidanceVector;
            }
        }
        float maxDistance = int.MinValue;
        var direction = Vector3.zero;
        for(int i = 0; i < directionsToCheckWhenAvoidingObstacles.Length; i++)
        {
            RaycastHit hit;
            var currentDirection = transform.TransformDirection(directionsToCheckWhenAvoidingObstacles[i].normalized) ;
            if (Physics.Raycast(transform.position, currentDirection, out hit,assignedFlock._obstacleDistance, obstacleLayer))
            {
                float currentDistance = (hit.point - transform.position).sqrMagnitude;
                if(currentDistance > maxDistance)
                {
                    maxDistance = currentDistance;
                    direction = currentDirection;
                }
            }
            else
            {
                direction = currentDirection;
                currentObstacleAvoidanceVector = currentDirection.normalized;
                return direction.normalized;
            }
        }
        return direction.normalized;
    }

    private bool IsInFOV(Vector3 position)
    {
        return Vector3.Angle(transform.forward, position - transform.position) <= FOVAngle;
    }
}
