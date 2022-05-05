using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class DeathField : MonoBehaviour
{
    [Header("[DeathField]")]
    [SerializeField] private float deathFieldRadius = 100;
    [SerializeField] private Vector2 deathFieldMinMaxRadius;
    [SerializeField] private float deathFieldDamage = 5;
    [SerializeField] private float deathFieldAttackCooltime = 1;
    [SerializeField] private float deathFieldReduceSpeed = 3;
    [SerializeField] private LayerMask deathFieldLayer;
    private void Start()
    {
        if (PhotonNetwork.IsMasterClient || PhotonNetwork.IsConnected == false)
        {
            StartCoroutine("DeathFieldCoroutine");
        }
    }

    IEnumerator DeathFieldCoroutine()
    {
        yield return new WaitForSeconds(deathFieldAttackCooltime);

        // ���� �� �������� �ʴ� �÷��̾ ã�Ƽ� Attack
        Collider[] innerFieldShips = Physics.OverlapSphere(Vector3.zero, deathFieldRadius, deathFieldLayer);
        List<Player_Controller_Ship> tmpColls = new List<Player_Controller_Ship>();
        foreach (Collider c in innerFieldShips)
        {
            tmpColls.Add(c.GetComponent<Player_Controller_Ship>());
        }
        // ������ �ƴ϶� �ٱ��� �ִ� �ֵ��� ������ �޾ƾ��ϱ⿡ GameManager�� ��� Ship�� ���ؼ� ���� �ȿ� �ִ� ship ����Ʈ�� ���ִ��� Ȯ��
        for (int i = 0; i < GameManager.GetInstance().AllShip.Count; i++)
        {
            if (GameManager.GetInstance().AllShip[i] != null && tmpColls.Contains(GameManager.GetInstance().AllShip[i]) == false)
            {
                GameManager.GetInstance().AllShip[i].GetComponent<PhotonView>().RPC("Attacked", RpcTarget.AllBuffered, new object[] { deathFieldDamage, Vector3.zero });
            }
        }
        StartCoroutine("DeathFieldCoroutine");
    }

    private void Update()
    {
        deathFieldRadius -= Time.deltaTime * deathFieldReduceSpeed;
        deathFieldRadius = Mathf.Clamp(deathFieldRadius, deathFieldMinMaxRadius.x, deathFieldMinMaxRadius.y);
        this.transform.localScale = Vector3.one * deathFieldRadius / 200;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(Vector3.zero, deathFieldRadius);
    }
}
