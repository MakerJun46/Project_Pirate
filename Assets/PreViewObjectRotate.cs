using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreViewObjectRotate : MonoBehaviour
{
    [SerializeField] float rotateSpeed = 5f;

    void Update()
    {
        gameObject.transform.Rotate(new Vector3(0, rotateSpeed * Time.deltaTime, 0));        
    }
}
