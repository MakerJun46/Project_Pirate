using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class FieldOfView : MonoBehaviourPun
{
    public float viewRadius;
    [Range(0, 360)]
    public float viewAngle;

    public LayerMask targetMask;
    public LayerMask obstacleMask;

    public List<Transform> visibleTargets = new List<Transform>();
    public Transform currTarget;

    public float meshResolution;
    public int edgeResolveInterations;
    public float edgeDistanceThreshold;
    public MeshFilter viewMeshFilter;
    Mesh viewMesh;

    public MeshFilter coolTimeMeshFilter;
    Mesh coolTimeMesh;

    [SerializeField] Material[] fov_mats;

    Cannon myCannon;

    GameMode gameMode;

    private void Start()
    {
        viewMesh = new Mesh();
        viewMesh.name = "View Mesh";
        viewMeshFilter.mesh = viewMesh;

        coolTimeMesh = new Mesh();
        coolTimeMesh.name = "CoolTime Mesh";
        coolTimeMeshFilter.mesh = coolTimeMesh;

        myCannon = GetComponent<Cannon>();
    }
    private void OnEnable()
    {
        StopCoroutine("FindTargetsWithDelay");
        StartCoroutine("FindTargetsWithDelay", 0.2f);
    }
    private void OnDisable()
    {
        StopCoroutine("FindTargetsWithDelay");
    }

    private void Update()
    {
        if (myCannon.myShip.photonView.IsMine || PhotonNetwork.IsConnected==false)
        {
            DrawFieldOfView(viewMesh, viewRadius);
            DrawFieldOfView(coolTimeMesh,viewRadius * myCannon.currChargeAmount/ myCannon.maxChargetAmount);

            float minDistance = 1000;
            int index = -1;
            for (int i = 0; i < visibleTargets.Count; i++)
            {
                if (visibleTargets[i] == null)
                    continue;
                float tmpDistance = Vector3.Distance(this.transform.position, visibleTargets[i].transform.position);
                if (tmpDistance <= minDistance)
                {
                    minDistance = tmpDistance;
                    index = i;
                }
            }
            print("index : " + index);
            if (0 <= index && index < visibleTargets.Count)
            {
                currTarget = visibleTargets[index];
            }
            else
            {
                currTarget = null;
            }

            if (currTarget == null)
            {
                myCannon.currChargeAmount -= Time.deltaTime;
                viewMeshFilter.GetComponent<MeshRenderer>().enabled = false;
                coolTimeMeshFilter.GetComponent<MeshRenderer>().enabled = false;
            }
            else
            {
                if (myCannon.currCoolTime<=0)
                {
                    viewMeshFilter.GetComponent<MeshRenderer>().material = fov_mats[0];
                    coolTimeMeshFilter.GetComponent<MeshRenderer>().enabled = true;
                    myCannon.currChargeAmount += Time.deltaTime * 3f;
                }
                else
                {
                    viewMeshFilter.GetComponent<MeshRenderer>().material = fov_mats[1];
                    coolTimeMeshFilter.GetComponent<MeshRenderer>().enabled = false;
                }

                viewMeshFilter.GetComponent<MeshRenderer>().enabled = true;
            }
            myCannon.currChargeAmount = Mathf.Clamp(myCannon.currChargeAmount, 0, myCannon.maxChargetAmount);
        }
    }
    IEnumerator FindTargetsWithDelay(float delay)
    {
        while (true)
        {
            yield return new WaitForSeconds(delay);
            FindVisibleTargets();
        }

    }

    void FindVisibleTargets()
    {
        visibleTargets.Clear();
        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, viewRadius , targetMask);
        for (int i = 0; i < targetsInViewRadius.Length; i++)
        {
            Transform target = targetsInViewRadius[i].transform;
            if (target == GetComponentInParent<Player_Combat_Ship>().transform)
            {
                continue;
            }

            Vector3 dirToTarget = (target.position - transform.position).normalized;
            float dstToTarget = Vector3.Distance(transform.position, target.position);
            if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle / 2)
            {
                if (!Physics.Raycast(transform.position, dirToTarget, dstToTarget, obstacleMask))
                {
                    visibleTargets.Add(target);
                }
            }
        }
    }

    void DrawFieldOfView(Mesh _viewMesh, float _viewRadius)
    {
        int stepCount = Mathf.RoundToInt(viewAngle * meshResolution);
        float stepAngleSize = viewAngle / stepCount;
        List<Vector3> viewPoints = new List<Vector3>();
        ViewCastInfo oldViewCast = new ViewCastInfo();
        for (int i = 0; i <= stepCount; i++)
        {
            float angle = transform.eulerAngles.y - viewAngle / 2 + stepAngleSize * i;
            ViewCastInfo newViewCast = ViewCast(angle, _viewRadius);
            if (i > 0)
            {
                bool edgeDstThresholdExceeded = Mathf.Abs(oldViewCast.dst - newViewCast.dst) > edgeDistanceThreshold;
                if (oldViewCast.hit != newViewCast.hit || (oldViewCast.hit && newViewCast.hit && edgeDstThresholdExceeded))
                {
                    EdgeInfo edge = FindEdge(oldViewCast, newViewCast, _viewRadius);
                    if (edge.pointA != Vector3.zero)
                    {
                        viewPoints.Add(edge.pointA);
                    }
                    if (edge.pointB != Vector3.zero)
                    {
                        viewPoints.Add(edge.pointB);
                    }
                }
            }
            viewPoints.Add(newViewCast.point);
            oldViewCast = newViewCast;
        }
        int vertexCount = viewPoints.Count + 1;
        Vector3[] verticies = new Vector3[vertexCount];
        int[] triangles = new int[(vertexCount - 2) * 3];

        verticies[0] = Vector3.zero;
        for (int i = 0; i < vertexCount - 1; i++)
        {
            verticies[i + 1] = transform.InverseTransformPoint(viewPoints[i]);

            if (i < vertexCount - 2)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }
        }
        _viewMesh.Clear();
        _viewMesh.vertices = verticies;
        _viewMesh.triangles = triangles;
        _viewMesh.RecalculateNormals();
    }
    


    EdgeInfo FindEdge(ViewCastInfo minViewCast, ViewCastInfo maxViewCast, float _viewRadius)
    {
        float minAngle = minViewCast.angle;
        float maxAngle = maxViewCast.angle;
        Vector3 minPoint = Vector3.zero;
        Vector3 maxPoint = Vector3.zero;

        for (int i = 0; i < edgeResolveInterations; i++)
        {
            float angle = (minAngle + maxAngle) / 2;
            ViewCastInfo newViewCast = ViewCast(angle, _viewRadius);

            bool edgeDstThresholdExceeded = Mathf.Abs(minViewCast.dst - newViewCast.dst) > edgeDistanceThreshold;

            if (newViewCast.hit == minViewCast.hit && !edgeDstThresholdExceeded)
            {
                minAngle = angle;
                minPoint = newViewCast.point;
            }
            else
            {
                maxAngle = angle;
                maxPoint = newViewCast.point;
            }
        }
        return new EdgeInfo(minPoint, maxPoint);
    }

    ViewCastInfo ViewCast(float globalAngle, float viewRadius)
    {
        Vector3 dir = DirFromAngle(globalAngle, true);
        RaycastHit hit;
        if (Physics.Raycast(transform.position, dir, out hit, viewRadius, obstacleMask))
        {
            return new ViewCastInfo(true, hit.point, hit.distance, globalAngle);
        }
        else
        {
            return new ViewCastInfo(false, transform.position + dir * viewRadius, viewRadius, globalAngle);
        }
    }


    public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    public struct ViewCastInfo
    {
        public bool hit;
        public Vector3 point;
        public float dst;
        public float angle;

        public ViewCastInfo(bool _hit, Vector3 _point, float _dst, float _angle)
        {
            hit = _hit;
            point = _point;
            dst = _dst;
            angle = _angle;
        }
    }

    public struct EdgeInfo
    {
        public Vector3 pointA;
        public Vector3 pointB;

        public EdgeInfo(Vector3 _pointA, Vector3 _pointB)
        {
            pointA = _pointA;
            pointB = _pointB;
        }
    }
}