using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooting : MonoBehaviour
{
    [SerializeField] private Transform bulletPosition;
    
    public void Shoot()
    {
        Quaternion targetRotation = transform.rotation;

        BulletPool.Instance.SpawnFromPool(bulletPosition.position, targetRotation);
    }
}
