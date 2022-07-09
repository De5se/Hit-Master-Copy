using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooting : MonoBehaviour
{
    [SerializeField] private Transform bulletPosition;

    private Camera mainCamera;
    
    private void Start()
    {
        mainCamera = Camera.main;
    }

    public void Shoot()
    {
        var relativePos = GetTapPosition() - transform.position;
        var targetRotation = Quaternion.LookRotation(relativePos);
        
        BulletPool.Instance.SpawnFromPool(bulletPosition.position, targetRotation);
    }

    private Vector3 GetTapPosition()
    {
        var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        Vector3 targetPosition;
        
        if (Physics.Raycast(ray, out var hitInfo, 30f))
        {
            targetPosition = hitInfo.point;
        }
        else
        {
            var worldTarget = new Vector3(Input.mousePosition.x, Input.mousePosition.y, mainCamera.farClipPlane);
            targetPosition = mainCamera.ScreenToWorldPoint(worldTarget);
        }

        return targetPosition;
    }
}
