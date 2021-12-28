using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Island_Info : MonoBehaviour
{
    GameManager GM;
    System.Random random = new System.Random();

    public enum Island_Type { normal, cake, ice, mushroom, ruins, toy};

    public int min_Wood_Count;
    public int max_Wood_Count;
    public int min_Rock_Count;
    public int max_Rock_Count;
    public int min_SpecialResource_Count;
    public int max_SpecialResource_Count;

    public Island_Type type;
    bool ResourceCreate;

    void Start()
    {
        ResourceCreate = false;
    }

    private void Update()
    {
        if(PhotonNetwork.IsMasterClient && PhotonNetwork.IsConnected && !ResourceCreate)
        {
            ResourceCreate = true;
            Calc_Resources_Count();
        }
    }

    public void Calc_Resources_Count()
    {
        int woodCount = random.Next(min_Wood_Count, max_Wood_Count);
        int RockCount = random.Next(min_Rock_Count, max_Rock_Count);
        int specialCount = random.Next(min_SpecialResource_Count, max_SpecialResource_Count);

        if(type == Island_Type.normal)
        {
            specialCount = 0;
        }
        else if(type == Island_Type.ice)
        {
            woodCount = 0;
        }
        else if(type == Island_Type.mushroom)
        {
            RockCount = 0;
        }

        Debug.Log(type + "������ �ڿ� ����, " + woodCount + ", " + RockCount + ", " + specialCount);
        SpawnResources(woodCount, RockCount, specialCount);
    }

    public void SpawnResources(int woodCount, int RockCount, int SpecialResourceCount)
    {
        float Minx = transform.position.x - 10;
        float Maxx = transform.position.x + 10;

        float Minz = transform.position.z - 10;
        float Maxz = transform.position.z + 10;

        for(int i = 0; i < woodCount; i++)
        {
            float LocationX = random.Next((int)Minx, (int)Maxx);
            float LocationZ = random.Next((int)Minz, (int)Maxz);

            Vector3 location = new Vector3(LocationX, 0, LocationZ);

            GameObject tmpObj;
            tmpObj = PhotonNetwork.Instantiate("Wood", location, Quaternion.identity).gameObject;
            tmpObj.transform.parent = this.transform;
        }

        for(int i = 0; i < RockCount; i++)
        {
            float LocationX = random.Next((int)Minx, (int)Maxx);
            float LocationZ = random.Next((int)Minz, (int)Maxz);

            Vector3 location = new Vector3(LocationX, 0, LocationZ);

            GameObject tmpObj;
            tmpObj = PhotonNetwork.Instantiate("Rock", location, Quaternion.identity).gameObject;
            tmpObj.transform.parent = this.transform;
        }

        for(int i = 0; i < SpecialResourceCount; i++)
        {
            float LocationX = random.Next((int)Minx, (int)Maxx);
            float LocationZ = random.Next((int)Minz, (int)Maxz);

            Vector3 location = new Vector3(LocationX, 0, LocationZ);

            GameObject tmpObj;
            tmpObj = PhotonNetwork.Instantiate("SpecialResource", location, Quaternion.identity).gameObject;
            tmpObj.transform.parent = this.transform;
        }
    }
}