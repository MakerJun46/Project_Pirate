using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;


public class ObjectPoolManager : MonoBehaviour
{
    [System.Serializable]
    public class ObjInfo
    {
        public string objName;

        public Queue<GameObject> poolingProjectileQueue = new Queue<GameObject>();
    }


    private static ObjectPoolManager instance;
    public static ObjectPoolManager GetInstance()
    {
        if (instance == null)
        {
            instance = FindObjectOfType<ObjectPoolManager>();
        }
        return instance;
    }

    [SerializeField] public List<ObjInfo> objs = new List<ObjInfo>();

    private GameObject CreateNewObject(string _name)
    {
        int index = -1;
        for (int i = 0; i < GetInstance().objs.Count; i++)
        {
            if (GetInstance().objs[i].objName == _name)
            {
                index = i;
                break;
            }
        }

        if (index < 0)
        {
            print("There is no Pool");
            return null;
        }

        var newObj = PhotonNetwork.Instantiate(GetInstance().objs[index].objName,Vector3.zero,Quaternion.identity);
        newObj.SetActive(false);
        newObj.transform.SetParent(transform);
        return newObj;
    }
    public static GameObject GetObject(string _name)
    {
        int index = -1;
        for (int i = 0; i < GetInstance().objs.Count; i++)
        {
            if (GetInstance().objs[i].objName == _name)
            {
                index = i;
                break;
            }
        }

        if (index < 0)
        {
            print("There is no Pool");
            return null;
        }

        if (GetInstance().objs[index].poolingProjectileQueue.Count > 0)
        {
            var obj = GetInstance().objs[index].poolingProjectileQueue.Dequeue();
            obj.transform.SetParent(null);
            obj.gameObject.SetActive(true);
            return obj;
        }
        else
        {
            var newObj = GetInstance().CreateNewObject(_name);
            newObj.gameObject.SetActive(true);
            newObj.transform.SetParent(null);
            return newObj;
        }
    }

    public static void ReturnObject(string _name, GameObject obj)
    {
        int index = -1;
        for (int i = 0; i < GetInstance().objs.Count; i++)
        {
            if (GetInstance().objs[i].objName == _name)
            {
                index = i;
                break;
            }
        }

        if (index < 0)
        {
            print("There is no Pool");
            return;
        }

        obj.gameObject.SetActive(false);
        obj.transform.SetParent(GetInstance().transform);
        GetInstance().objs[index].poolingProjectileQueue.Enqueue(obj);
    }
}
