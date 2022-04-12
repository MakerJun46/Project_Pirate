using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Treasure : MonoBehaviour
{
    public bool isPickable;

    void Start()
    {
        isPickable = false;
    }

    public void startMove(Vector3 pos)
    {
        Debug.Log("RandomPos : " + pos);
        StartCoroutine(parabolicDrop(pos));
    }

    public IEnumerator parabolicDrop(Vector3 target_pos)
    {

        while (Vector3.Distance(transform.position, target_pos) > 0.05f)
        {
            transform.position =  Vector3.Slerp(gameObject.transform.position, target_pos, 7f * Time.deltaTime);


            Debug.Log("isMoving..");

            yield return null;

        }

        yield return null;

        isPickable = true;
    }
}
