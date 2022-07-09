using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(NavMeshAgent))]
public class PlayerMotion : MonoBehaviour
{
    private NavMeshAgent navMeshAgent;
    [SerializeField] private Transform wayPoints;
    [SerializeField] private Animator animator;
    private int currentPoint;

    public bool nextPoint;

    private void OnValidate()
    {
        if (!nextPoint) return;
        nextPoint = false;
        NextPoint();
    }

    private void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        currentPoint = -1;
        
        NextPoint();
    }

    private void NextPoint()
    {
        currentPoint++;
        if (wayPoints.childCount <= currentPoint)
        {
            WinGame();
            return;
        }
        
        var targetPoint = wayPoints.GetChild(currentPoint).position;
        navMeshAgent.SetDestination(targetPoint);
    }

    private void Update()
    {
        AnimateWalking();

        if (Input.GetMouseButtonDown(0))
        {
            Shoot();
        }
    }

    private static void WinGame()
    {
        LevelManager.Restart();
    }


    private void AnimateWalking()
    {
        var isWalking = navMeshAgent.velocity.magnitude > 0;
        
        animator.SetBool("IsWalking", isWalking);
    }

    private void Shoot()
    {
        Debug.Log("Shoot");
    }
}
