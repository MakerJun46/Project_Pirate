using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_UI_Ship : MonoBehaviour
{
    [SerializeField] private Camera myCam;
    [SerializeField] private Canvas myCanvas;
    [SerializeField] private Vector3 camOffset = new Vector3(0, 372, -290);

    /// <summary>
    /// 카메라 시점 및 닉네임/체력 바 canvas 고정을 위한 코드
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
