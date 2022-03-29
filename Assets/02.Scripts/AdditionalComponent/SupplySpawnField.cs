using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SupplySpawnField : MonoBehaviour
{
    [SerializeField] float SupplySpawnRadius = 100f;

    [SerializeField] float spawnCount = 2;
    [SerializeField] float spawnCoolTime = 10;

    void Start()
    {
        StartCoroutine("SpawnSupplyItemCoroutine");
    }

    IEnumerator SpawnSupplyItemCoroutine()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            for (int i = 0; i < spawnCount; i++)
            {
                PhotonNetwork.Instantiate("SupplyItem", CalculateSpawnPos(), Quaternion.Euler(0, 90, 0), 0, new object[] { Random.Range(0, 3) });
            }
        }
        yield return new WaitForSeconds(spawnCoolTime);
        StartCoroutine("SpawnSupplyItemCoroutine");
    }
    public Vector3 CalculateSpawnPos()
    {
        for (int i = 0; i < 50; i++)
        {
            Vector3 radomPos = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)) * SupplySpawnRadius;
            RaycastHit hit;
            if (Physics.SphereCast(radomPos + Vector3.up * 100, 10f, Vector3.down, out hit, 200f))
            {
                if (hit.transform.CompareTag("Sea"))
                {
                    return radomPos;
                }
            }
        }
        return new Vector3(10, 0, 10);
    }

    void OnDrawGizmos()
    {
        // Death Field Sphere
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(Vector3.zero, SupplySpawnRadius);
    }
}
