using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class AttackJoyStick : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    [SerializeField] Image joyStickBackground;
    [SerializeField] Image joyStick;

    private Vector2 joyStickInput;
    public Vector2 GetJoyStickInput()
    {
        return joyStickInput;
    }

    private void Start()
    {
        joyStickBackground = GetComponent<Image>();
        joyStick = transform.GetChild(0).GetComponent<Image>();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if(RectTransformUtility.ScreenPointToLocalPointInRectangle(joyStickBackground.rectTransform,
            eventData.position,eventData.pressEventCamera,out joyStickInput))
        {
            if (joyStickInput.magnitude > joyStickBackground.rectTransform.sizeDelta.x/2f)
                joyStickInput = joyStickInput.normalized* joyStickBackground.rectTransform.sizeDelta.x/2f;

            joyStick.rectTransform.anchoredPosition = joyStickInput;
            print("OnDrag");
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        joyStickInput = Vector2.zero;
        joyStick.rectTransform.anchoredPosition = Vector2.zero;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
    }
}
