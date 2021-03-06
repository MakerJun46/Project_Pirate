using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidUnit : MonoBehaviour
{
    #region Variables & Initializer
    [Header("Info")]
    Boids myBoids;
    List<BoidUnit> neighbours = new List<BoidUnit>();

    Vector3 targetVec;
    Vector3 egoVector;
    float speed;

    float additionalSpeed = 0;

    MeshRenderer myMeshRenderer;

    [Header("Neighbour")]
    [SerializeField] float obstacleDistance;
    [SerializeField] float FOVAngle=120;
    [SerializeField] float maxNeighbourCount = 50;
    [SerializeField] float neighbourDistance = 10;

    [Header("ETC")]
    [SerializeField] LayerMask boidUnitLayer;
    [SerializeField] LayerMask obstacleLayer;

    Coroutine findNeighbourCoroutine;
    Coroutine calculateEgoVectorCoroutine;
    public void InitializeUnit(Boids _boids, float _speed, int _myIndex)
    {
        myBoids = _boids;
        speed = _speed;
        myMeshRenderer = GetComponentInChildren<MeshRenderer>();

        findNeighbourCoroutine = StartCoroutine("FindNeighbourCoroutine");
        calculateEgoVectorCoroutine = StartCoroutine("CalculateEgoVectorCoroutine");
    }

    #endregion

    void Update()
    {
        if (additionalSpeed > 0)
            additionalSpeed -= Time.deltaTime;

        // Calculate all the vectors we need
        Vector3 cohesionVec = CalculateCohesionVector()*myBoids.cohesionWeight;
        Vector3 alignmentVec = CalculateAlignmentVector() * myBoids.alignmentWeight;
        Vector3 separationVec = CalculateSeparationVector() * myBoids.separationWeight;
        // 추가적인 방향
        Vector3 boundsVec = CalculateBoundsVector() * myBoids.boundsWeight;
        Vector3 obstacleVec = CalculateObstacleVector() * myBoids.obstacleWeight;
        Vector3 egoVec = egoVector * myBoids.egoWeight;

        targetVec = cohesionVec + alignmentVec + separationVec + boundsVec + obstacleVec + egoVec;

        // Steer and Move
        targetVec = Vector3.Lerp(this.transform.forward, targetVec, Time.deltaTime);
        targetVec = targetVec.normalized;
        if (targetVec == Vector3.zero)
            targetVec = egoVector;

        this.transform.rotation = Quaternion.LookRotation(targetVec);
        this.transform.position += targetVec * (speed + additionalSpeed) * Time.deltaTime;
    }


    #region Calculate Vectors
    IEnumerator CalculateEgoVectorCoroutine()
    {
        speed = Random.Range(myBoids.speedRange.x, myBoids.speedRange.y);
        egoVector = Random.insideUnitSphere;
        yield return new WaitForSeconds(Random.Range(1, 3f));
        calculateEgoVectorCoroutine = StartCoroutine("CalculateEgoVectorCoroutine");
    }
    IEnumerator FindNeighbourCoroutine()
    {
        neighbours.Clear();

        Collider[] colls = Physics.OverlapSphere(transform.position, neighbourDistance, boidUnitLayer);
        for (int i = 0; i < colls.Length; i++)
        {
            if (Vector3.Angle(transform.forward, colls[i].transform.position - transform.position) <= FOVAngle)
            {
                neighbours.Add(colls[i].GetComponent<BoidUnit>());
            }
            if (i > maxNeighbourCount)
            {
                break;
            }
        }
        yield return new WaitForSeconds(Random.Range(0.5f, 2f));
        findNeighbourCoroutine = StartCoroutine("FindNeighbourCoroutine");
    }
    private Vector3 CalculateCohesionVector()
    {
        Vector3 cohesionVec = Vector3.zero;
        if (neighbours.Count > 0)
        {
            // 이웃 unit들의 위치 더하기
            for(int i = 0; i < neighbours.Count; i++)
            {
                cohesionVec += neighbours[i].transform.position;
            }
        }
        else
        {
            // 이웃이 없으면 vector3.zero 반환
            return cohesionVec;
        }

        // 중심 위치로의 벡터 찾기
        cohesionVec /= neighbours.Count;
        cohesionVec -= transform.position;
        cohesionVec.Normalize();
        return cohesionVec;
    }

    private Vector3 CalculateAlignmentVector()
    {
        Vector3 alignmentVec = transform.forward;
        if (neighbours.Count > 0)
        {
            // 이웃들이 향하는 방향의 평균 방향으로 이동
            for(int i = 0; i < neighbours.Count; i++)
            {
                alignmentVec += neighbours[i].transform.forward;
            }
        }
        else
        {
            // 이웃이 없으면 그냥 forward로 이동
            return alignmentVec;
        }

        alignmentVec /= neighbours.Count;
        alignmentVec.Normalize();
        return alignmentVec;
    }

    private Vector3 CalculateSeparationVector()
    {
        Vector3 separationVec = Vector3.zero;
        if (neighbours.Count > 0)
        {
            // 이웃들을 피하는 방향으로 이동
            for(int i = 0; i < neighbours.Count; i++)
            {
                separationVec += (transform.position - neighbours[i].transform.position);
            }
        }
        else
        {
            // 이웃이 없으면 vector.zero 반환
            return separationVec;
        }
        separationVec /= neighbours.Count;
        separationVec.Normalize();
        return separationVec;
    }

    private Vector3 CalculateBoundsVector()
    {
        if(transform.position.y <= myBoids.yRange.x)
        {
            return Vector3.up;
        }else if(transform.position.y >= myBoids.yRange.y)
        {
            return Vector3.down;
        }

        Vector3 offsetToCenter = myBoids.transform.position - transform.position;
        return offsetToCenter.magnitude >= myBoids.spawnRange ? offsetToCenter.normalized : Vector3.zero;
    }

    private Vector3 CalculateObstacleVector()
    {
        Vector3 obstacleVec = Vector3.zero;
        RaycastHit hit;
        if(Physics.Raycast(transform.position,transform.forward,out hit, obstacleDistance, obstacleLayer))
        {
            Debug.DrawLine(transform.position, hit.point, Color.black);
            obstacleVec = hit.normal;
            additionalSpeed = 10;
        }
        return obstacleVec;
    }
#endregion

}
