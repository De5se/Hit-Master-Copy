using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bulet : MonoBehaviour, ObjectFromPool
{
    [SerializeField] private float speed;
    [SerializeField] private int damage;
    [SerializeField] private float explosionForce;
    [SerializeField] private float explosionRadius;
    [SerializeField] private float liveTime;

    private void Update()
    {
        var direction = transform.rotation * Vector3.forward;
        transform.position += direction * speed * Time.deltaTime;
    }

    private void OnCollisionEnter(Collision collision)
    {
        var collisionTransform = collision.transform;
        collisionTransform.GetComponent<UnitStats>()?.GetDamage(damage);
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
