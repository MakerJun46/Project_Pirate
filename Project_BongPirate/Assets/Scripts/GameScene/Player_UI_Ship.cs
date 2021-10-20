using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_UI_Ship : MonoBehaviour
{
    public GameObject MainCamera;
    public GameObject Canvas;

    /// <summary>
    /// ī�޶� ���� �� �г���/ü�� �� canvas ������ ���� �ڵ�
    /// </summary>
    void Start()
    {
        MainCamera = GameObject.Find("Main Camera");
    }

    // Update is called once per frame
    void Update()
    {
        //Vector3 vec = (MainCamera.transform.position - Canvas.transform.position).normalized;

        Canvas.transform.rotation = Quaternion.LookRotation(Canvas.transform.position -  MainCamera.transform.position); 

        MainCamera.transform.position = this.gameObject.transform.position + new Vector3(0, 372, -290);
    }
}
