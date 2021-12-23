using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Island_Landing_UI : MonoBehaviour
{
    public Text SailorCount;
    public int Count;

    private void Start()
    {
        Count = 0;
    }
    public void countReset()
    {
        Count = 0;
    }

    public void UpButton()
    {
        Count++;
        SailorCount.text = "���� ���� : " + Count;
    }

    public void DownButton()
    {
        if(Count > 0)
        {
            Count--;
            SailorCount.text = "���� ���� : " + Count;
        }
    }

    public void Landing()
    {

    }
}
