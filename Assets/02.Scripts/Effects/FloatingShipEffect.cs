using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingShipEffect : MonoBehaviour
{
    public float amplitude = 0.5f;
    public float WaveSpeed = 0.01f;
    public float waveSize = 0.01f;

    public float yOffset = -1f;

    private void Start()
    {
        transform.position -= new Vector3(0, yOffset, 0);
    }

    private void FixedUpdate()
    {
        Vector3 pos = transform.position;

        pos.y += amplitude * Mathf.Sin(WaveSpeed * Time.time) * 0.01f;

        transform.position = pos;
    }
}
