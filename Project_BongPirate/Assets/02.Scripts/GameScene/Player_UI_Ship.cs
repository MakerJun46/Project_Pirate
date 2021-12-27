using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_UI_Ship : MonoBehaviour
{
    [SerializeField] private Camera myCam;
    [SerializeField] private Canvas myCanvas;
    [SerializeField] private Vector3 camOffset = new Vector3(0, 372, -290);

    /// <summary>
    /// ī�޶� ���� �� �г���/ü�� �� canvas ������ ���� �ڵ�
    /// </summary>
    void Start()
    {
        myCam = Camera.main;
        myCanvas = GetComponentInChildren<Canvas>();
    }

    void Update()
    {
        myCanvas.transform.rotation = Quaternion.LookRotation(myCanvas.transform.position -  myCam.transform.position); 

        myCam.transform.position = this.gameObject.transform.position + camOffset;
    }
}
