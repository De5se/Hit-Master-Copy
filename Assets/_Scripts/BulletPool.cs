using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;

public class BulletPool : MonoBehaviour
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private int size;
    private Queue<GameObject> bullets = new Queue<GameObject>();
    
    #region Singleton
    
    public static BulletPool Instance;
    
    private void Awake()
    {
        Instance = this;
    }
    #endregion
    
    private void Start()
    {
        for (int i = 0; i < size; i++)
        {
            var obj = Instantiate(prefab, transform);
            obj.SetActive(false);
            bullets.Enqueue(obj);
        }
    }


    public GameObject SpawnFromPool(Vector3 instantiatePosition, Quaternion instantiateRotation)
    {
        if (bullets.Count == 0)
        {
            Debug.LogError("Queue is empty!");
            return null;
        }
        
        var objectToSpawn = bullets.Dequeue();
        
        objectToSpawn.transform.position = instantiatePosition;
        objectToSpawn.transform.rotation = instantiateRotation;
        objectToSpawn.SetActive(true);
        objectToSpawn.GetComponent<ObjectFromPool>()?.OnObjectSpawned();

        return objectToSpawn;
    }

    public void AddObjectToPool(GameObject objectToSpawn)
    {
        bullets.Enqueue(objectToSpawn);
    }
}
