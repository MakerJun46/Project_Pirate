using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Treasure : MonoBehaviour
{
    public bool isPickable;
    public float rotateSpeed;
    public int Score;

    PhotonView PV;

    [SerializeField] MeshFilter MF;
    [SerializeField] MeshRenderer MR;
    [SerializeField] Mesh[] GemMeshs;
    [SerializeField] Material[] GemMats;

    void Start()
    {
        PV = this.GetComponent<PhotonView>();
        isPickable = false;

        MF.mesh = GemMeshs[Random.Range(0, GemMeshs.Length)];
        MR.material = GemMats[Random.Range(0, GemMats.Length)];
    }

    private void Update()
    {
        if(isPickable)
        {
            transform.Rotate(new Vector3(0, rotateSpeed * Time.deltaTime, 0));
        }
    }

    public void startMove(Vector3 pos)
    {
        StartCoroutine(parabolicDrop(pos));
    }

    public IEnumerator parabolicDrop(Vector3 target_pos) // 보물 생성 될 때 포물선 움직임
    {

        while (Vector3.Distance(transform.position, target_pos) > 0.05f)
        {
            transform.position =  Vector3.Slerp(gameObject.transform.position, target_pos, 7f * Time.deltaTime);

            yield return null;
        }

        yield return null;

        PV.RPC("Set_Pickable", RpcTarget.AllBuffered);
    }

    [PunRPC]
    public void Set_Pickable()
    {
        isPickable = true;
    }
}
