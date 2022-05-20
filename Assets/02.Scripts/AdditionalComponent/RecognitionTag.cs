using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class RecognitionTag : MonoBehaviour
{
    public Player_Controller_Ship myEnemy;
    private Image spriteImg;
    [SerializeField] float border=30;
    [SerializeField] float threshold = 10;
    private void Start()
    {
        spriteImg = GetComponent<Image>();
    }
    void Update()
    {
        if (myEnemy)
        {
            //Vector3 targetPos = Camera.main.WorldToScreenPoint(myEnemy.transform.position, Camera.MonoOrStereoscopicEye.Mono);
            Vector3 targetPos = Camera.main.WorldToScreenPoint(myEnemy.transform.position + Vector3.up * 2f);
            //print("TargetPos : " + targetPos);
            //this.transform.position = targetPos;
            bool canSee = (targetPos.x < (border + threshold) || targetPos.x > Screen.width - (border + threshold) || targetPos.y < (border + threshold) || targetPos.y > Screen.height - (border + threshold));
            spriteImg.enabled = canSee;
            transform.position = new Vector3(Mathf.Clamp(targetPos.x, border, Screen.width - border), Mathf.Clamp(targetPos.y, border, Screen.height - border-120), 1f);
            //GetComponent<RectTransform>().anchoredPosition = new Vector2(Mathf.Clamp(targetPos.x, -Screen.width/2,Screen.width/2), Mathf.Clamp(targetPos.y, -Screen.height/2, Screen.height/2));
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}