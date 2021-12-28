using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player_UI_Ship : MonoBehaviour
{
    [SerializeField] private Canvas myCanvas;
    [SerializeField] private Image healthImage;
    /// <summary>
    /// ī�޶� ���� �� �г���/ü�� �� canvas ������ ���� �ڵ�
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
