using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class FieldOfView : MonoBehaviourPun
{
    public float viewRadius;
    [Range(0, 360)]
    public float viewAngle;

    [SerializeField] private LayerMask Enemy_Ship_Mask;
    [SerializeField] private LayerMask Ship_Mask;
    [SerializeField] private LayerMask Enemy_Mask;
    public int targetMaskType;
    public LayerMask obstacleMask;

    public List<Transform> visibleTargets = new List<Transform>();
    public Transform currTarget;

    public float meshResolution;
    public int edgeResolveInterations;
    public float edgeDistanceThreshold;
    public MeshFilter viewMeshFilter;
    Mesh viewMesh;

    public bool useCoolTime=true;
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
        viewMeshFilter.transform.position = new Vector3(viewMeshFilter.transform.position.x, 1f, viewMeshFilter.transform.position.z);
        coolTimeMeshFilter.transform.position = new Vector3(coolTimeMeshFilter.transform.position.x, 1f, coolTimeMeshFilter.transform.position.z);
        if (myCannon.myShip.photonView.IsMine || PhotonNetwork.IsConnected==false)
        {
            DrawFieldOfView(viewMesh, viewRadius);
            DrawFieldOfView(coolTimeMesh,viewRadius * myCannon.currChargeAmount/ myCannon.maxChargetAmount);

            float minDistance = 1000;
            int closestIndex = -1;
            for (int i = 0; i < visibleTargets.Count; i++)
            {
                if (visibleTargets[i] == null)
                    continue;
                float tmpDistance = Vector3.Distance(this.transform.position, visibleTargets[i].transform.position);
                if (tmpDistance <= minDistance)
                {
                    minDistance = tmpDistance;
                    closestIndex = i;
                }
            }

            if (0 <= closestIndex && closestIndex < visibleTargets.Count)
            {
                currTarget = visibleTargets[closestIndex];
            }
            else
            {
                currTarget = null;
            }

            MeshRenderer viewMeshRenderer = viewMeshFilter.GetComponent<MeshRenderer>();
            MeshRenderer coolTimeMeshRenderer = coolTimeMeshFilter.GetComponent<MeshRenderer>();
            if (useCoolTime)
            {
                if (currTarget == null)
                {
                    myCannon.currChargeAmount -= Time.deltaTime;
                    viewMeshRenderer.enabled = false;
                    coolTimeMeshRenderer.enabled = false;
                }
                else
                {
                    if (myCannon.currCoolTime <= 0)
                    {
                        viewMeshRenderer.material = fov_mats[0];
                        coolTimeMeshRenderer.enabled = true;
                        myCannon.currChargeAmount += Time.deltaTime * 3f;
                    }
                    else
                    {
                        viewMeshRenderer.material = fov_mats[1];
                        coolTimeMeshRenderer.enabled = false;
                    }

                    viewMeshRenderer.enabled = true;
                }
            }
            else
            {
                viewMeshRenderer.enabled = true;
                coolTimeMeshRenderer.enabled = false;
                myCannon.currChargeAmount =3;
                if (currTarget == null)
                {
                    viewMeshRenderer.material = fov_mats[0];
                }
                else
                {
                    viewMeshRenderer.material = fov_mats[2];
                }
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
        Collider[] targetsInViewColliders;
        switch (targetMaskType)
        {
            case 0:
                targetsInViewColliders = Physics.OverlapSphere(transform.position, viewRadius, Enemy_Ship_Mask);
                break;
            case 1:
                targetsInViewColliders = Physics.OverlapSphere(transform.position, viewRadius, Ship_Mask);
                break;
            case 2:
                targetsInViewColliders = Physics.OverlapSphere(transform.position, viewRadius, Enemy_Mask);
                break;
            default:
                targetsInViewColliders = Physics.OverlapSphere(transform.position, viewRadius, Enemy_Ship_Mask);
                break;
        }

        for (int i = 0; i < targetsInViewColliders.Length; i++)
        {
            Transform target = targetsInViewColliders[i].transform;
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

        Vector2[] uvs = new Vector2[vertexCount];

        verticies[0] = Vector3.zero;
        uvs[0] = Vector2.zero;

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

        for (int i = 1; i < vertexCount ; i++)
        {
            float angle = ((float)i / (vertexCount - 1)) * 90f * Mathf.Deg2Rad;
            uvs[i] = new Vector2(Mathf.Cos(angle) , Mathf.Sin(angle));
        }

        /*
        for(int i=1;i< vertexCount / 2; i++)
        {
            uvs[i] = new Vector2( (i) / (vertexCount / 2f), 1f);
        }
        for(int i = vertexCount / 2; i < vertexCount; i++)
        {
            uvs[i] = new Vector2(1f,1- (((i+1)- (vertexCount / 2)) / (vertexCount / 2f)));
        }
        */

        _viewMesh.Clear();
        _viewMesh.vertices = verticies;
        _viewMesh.triangles = triangles;
        _viewMesh.uv = uvs;
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