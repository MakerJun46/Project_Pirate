using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapCamera : MonoBehaviour
{
    public GameObject Player;

    Vector3 angle;

    private void Awake()
    {
        angle = new Vector3(90, 0, 0);
        Player = null;
    }

    void Update()
    {
        if(Player != null)
            updatePos();
    }

    private void updatePos()
    {
        gameObject.transform.position = Player.transform.position + new Vector3(0, 80, 0);
        gameObject.transform.eulerAngles = angle;
    }
}
