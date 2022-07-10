using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UnitStats))]
public class RagdollController : MonoBehaviour
{
    private Rigidbody[] rigidbodies;
    [SerializeField] private Animator animator;
    private CapsuleCollider capsuleCollider;
    
    private void Start()
    {
        rigidbodies = GetComponentsInChildren<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        ActivateRagdoll(false);
    }

    public void ActivateRagdoll(bool isActive)
    {
        capsuleCollider.enabled = !isActive;
        animator.enabled = !isActive;
        
        foreach (var bone in rigidbodies)
        {
            bone.isKinematic = !isActive;
        }
    }
}
