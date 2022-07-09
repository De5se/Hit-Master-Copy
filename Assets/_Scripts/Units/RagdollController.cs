using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UnitStats))]
public class RagdollController : MonoBehaviour
{
    private Rigidbody[] rigidbodies;
    [SerializeField] private Animator animator;
    
    private void Start()
    {
        rigidbodies = GetComponentsInChildren<Rigidbody>();
        ActivateRagdoll(false);
    }

    public void ActivateRagdoll(bool isActive)
    {
        foreach (var bone in rigidbodies)
        {
            bone.isKinematic = !isActive;
        }

        animator.enabled = !isActive;
    }
}
