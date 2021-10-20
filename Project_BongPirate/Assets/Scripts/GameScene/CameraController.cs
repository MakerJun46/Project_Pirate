using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject player;
    public bool isConnected;

    void Start()
    {
        isConnected = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(isConnected)
        {
        }
    }
}
