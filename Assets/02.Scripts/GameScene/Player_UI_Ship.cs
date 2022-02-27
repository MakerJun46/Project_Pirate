using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player_UI_Ship : MonoBehaviour
{
    [SerializeField] private Canvas myCanvas;
    [SerializeField] private Image healthImage;
    /// <summary>
    /// 카메라 시점 및 닉네임/체력 바 canvas 고정을 위한 코드
    /// </summary>


    void Update()
    {
        //Vector3 vec = (MainCamera.transform.position - Canvas.transform.position).normalized;
        myCanvas.transform.rotation = Quaternion.LookRotation(myCanvas.transform.position -  Camera.main.transform.position); 
    }

    public void UpdateHealth(float _val)
    {
        healthImage.fillAmount = _val;
    }
}
