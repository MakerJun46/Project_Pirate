using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FloatingTextController : MonoBehaviour
{
    private static FloatingText popupText;
    private static GameObject canvas;

    public static void Initialize()
    {
        canvas = GameObject.Find("UI_Canvas");
        if (!popupText)
        {
            popupText = Resources.Load<FloatingText>("PopupTextParent");
        }
    }
    public static void CreateFloatingText(string text, Transform location, Color color)
    {
        Initialize();
        FloatingText instance = Instantiate(popupText);
        Vector2 screenPosition = Camera.main.WorldToScreenPoint(new Vector3(location.position.x + Random.Range(-0.1f, 0.1f), location.position.y + 1.5f, location.position.z + Random.Range(-0.1f, 0.1f)));
        instance.transform.SetParent(canvas.transform, false);
        instance.transform.position = screenPosition;
        instance.gameObject.GetComponentInChildren<Text>().color = color;
        instance.SetText(text);
    }
}