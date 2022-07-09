using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bulet : MonoBehaviour, ObjectFromPool
{
    [SerializeField] private float speed;
    [SerializeField] private float liveTime;
    
    private void Update()
    {
        var direction = transform.rotation * Vector3.forward;
        transform.position += direction * speed * Time.deltaTime;
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collision");
        ReturnToPool();
    }


    public void ReturnToPool()
    {
        gameObject.SetActive(false);
        BulletPool.Instance.AddObjectToPool(gameObject);
    }

    public void OnObjectSpawned()
    {
        Invoke(nameof(ReturnToPool), liveTime);
    }
}
