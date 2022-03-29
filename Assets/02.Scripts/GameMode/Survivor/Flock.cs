using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flock : MonoBehaviour
{
    [SerializeField] private FlockUnit flockUnitPrefab;
    [SerializeField] private int flockSize;
    [SerializeField] private Vector3 spawnBounds;

    [Range(0, 10)]
    public float _cohesionDistance;
    [Range(0, 10)]
    public float _avoidanceDistance;
    [Range(0, 10)]
    public float _alignmentDistance;
    [Range(0, 100)]
    public float _boundsDistance;
    [Range(0, 10)]
    public float _obstacleDistance;

    [Range(0, 10)]
    public float _cohesionWeight;
    [Range(0, 10)]
    public float _avoidanceWeight;
    [Range(0, 10)]
    public float _alignmentWeight;
    [Range(0, 10)]
    public float _boundsWeight;
    [Range(0, 100)]
    public float _obstacleWeight;

    public Vector2 speedRange;

    public Vector2 MinMaxYParam;


    public FlockUnit[] allUnits { get; set; }


    private void Start()
    {
        GenerateUnits();
    }

    private void Update()
    {
        for (int i = 0; i < allUnits.Length; i++)
        {
            allUnits[i].MoveUnit();
        }
    }

    private void GenerateUnits()
    {
        allUnits = new FlockUnit[flockSize];
        for(int i = 0; i < flockSize; i++)
        {
            Vector3 randomVec = Random.insideUnitSphere;
            randomVec = new Vector3(randomVec.x * spawnBounds.x, randomVec.y * spawnBounds.y, randomVec.z * spawnBounds.z);
            Vector3 spawnPosition = transform.position + randomVec;
            Quaternion rotation = Quaternion.Euler(0, Random.Range(0, 360f), 0f);
            allUnits[i] = Instantiate(flockUnitPrefab, spawnPosition, rotation);
            allUnits[i].transform.SetParent(this.transform);
            allUnits[i].AssignFlock(this);
            allUnits[i].InitializeSpeed(Random.Range(speedRange.x, speedRange.y));
        }
    }
}
