using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Trail 정보를 저장하기 위한 Struct
/// </summary>
[System.Serializable]
public class MeshTrailStruct
{
    public GameObject Container;

    public List<MeshFilter> MeshFilters=new List<MeshFilter>();
}

public class MotionTrail : MonoBehaviour
{
    #region Variables & Initializer
    [Header("[PreRequisite]")]
    private Transform TrailContainer;
    [SerializeField] private GameObject MeshTrailPrefab;
    [SerializeField] List<MeshTrailStruct> MeshTrailStructs = new List<MeshTrailStruct>();

    private MeshFilter[] meshFilters;

    private List<GameObject> bodyParts = new List<GameObject>();
    private List<Vector3> posMemory = new List<Vector3>();
    private List<Quaternion> rotMemory = new List<Quaternion>();

    [Header("[Trail Info]")]
    [SerializeField] private int TrailCount;
    [SerializeField] private float TrailGap = 0.2f;
    [SerializeField] [ColorUsage(true, true)] private Color frontColor;
    [SerializeField] [ColorUsage(true, true)] private Color backColor;
    [SerializeField] [ColorUsage(true, true)] private Color frontColor_Inner;
    [SerializeField] [ColorUsage(true, true)] private Color backColor_Inner;

    #endregion
    bool started = false;
    #region MotionTrail
    private void Start()
    {
        StartMotionTrail();
    }

    float currTime = 0;
    public void Update()
    {
        currTime += Time.deltaTime;
        if (TrailGap<= currTime)
        {
            currTime -= TrailGap;
            
            // Snake 게임처럼 이전의 position과 rotation을 기억
            posMemory.Insert(0, transform.position);
            rotMemory.Insert(0, transform.rotation);

            // Trail Count를 넘어서면 제거
            if (posMemory.Count > TrailCount)
                posMemory.RemoveAt(posMemory.Count - 1);
            if (rotMemory.Count > TrailCount)
                rotMemory.RemoveAt(rotMemory.Count - 1);
            // 기억해둔 Pos, Rot 할당
            for (int i = 0; i < bodyParts.Count; i++)
            {
                bodyParts[i].transform.position = posMemory[Mathf.Min(i, posMemory.Count - 1)];
                bodyParts[i].transform.rotation = rotMemory[Mathf.Min(i, rotMemory.Count - 1)];
                //bodyParts[i].transform.Rotate(Vector3.right * 90);
            }
        }
    }
    public void StartMotionTrail()
    {
        if (started)
        {
            return;
        }
        started = true;
        // 새로운 TailContainer 게임오브젝트를 만들어서 Trail 오브젝트들을 관리
        TrailContainer = new GameObject("TrailContainer").transform;

        meshFilters = GetComponentsInChildren<MeshFilter>();
        for (int i = 0; i < TrailCount; i++)
        {
            // 원하는 TrailCount만큼 생성
            MeshTrailStruct pss = new MeshTrailStruct();
            pss.Container = Instantiate(MeshTrailPrefab, TrailContainer);

            for (int j = 0; j < meshFilters.Length; j++)
            {
                GameObject tmpObj= Instantiate(pss.Container.transform.GetChild(0).gameObject, pss.Container.transform);
                tmpObj.SetActive(true);
                pss.MeshFilters.Add(pss.Container.transform.GetChild(j+1).GetComponent<MeshFilter>());
                pss.MeshFilters[pss.MeshFilters.Count - 1].mesh = meshFilters[j].mesh;
                tmpObj.transform.localScale = meshFilters[j].transform.lossyScale;
                tmpObj.transform.localPosition = meshFilters[j].transform.localPosition;

            }

            MeshTrailStructs.Add(pss);

            bodyParts.Add(pss.Container);

            // Material 속성 설정
            float alphaVal = (1f - (float)i / TrailCount) * 0.5f;
            for (int j = 0; j < pss.MeshFilters.Count; j++)
            {
                pss.MeshFilters[j].GetComponent<MeshRenderer>().material.SetFloat("_Alpha", alphaVal);
                Color tmpColor = Color.Lerp(frontColor, backColor, (float)i / TrailCount);
                pss.MeshFilters[j].GetComponent<MeshRenderer>().material.SetColor("_FresnelColor", tmpColor);
                Color tmpColor_Inner = Color.Lerp(frontColor_Inner, backColor_Inner, (float)i / TrailCount);
                pss.MeshFilters[j].GetComponent<MeshRenderer>().material.SetColor("_BaselColor", tmpColor_Inner);
            }
        }

    }
    #endregion

}
