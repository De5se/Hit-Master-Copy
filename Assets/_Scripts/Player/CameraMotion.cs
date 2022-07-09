using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMotion : MonoBehaviour
{
    [SerializeField] private Transform targetObject;
    [SerializeField] private float speed;
    [SerializeField] private Vector3 offset;

    private void LateUpdate()
    {
        var targetPosition = targetObject.position + targetObject.forward * offset.z + targetObject.up * offset.y + targetObject.right * offset.x;
        transform.position = Vector3.Lerp(transform.position, targetPosition, speed);
        
        transform.LookAt(targetObject);
    }
}
