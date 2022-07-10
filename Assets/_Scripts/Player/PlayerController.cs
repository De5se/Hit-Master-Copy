using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(NavMeshAgent), typeof(Shooting))]
public class PlayerController : MonoBehaviour
{
    private NavMeshAgent navMeshAgent;
    private Shooting shooting;
    
    [SerializeField] private Transform wayPoints;
    [SerializeField] private Animator animator;
    private int currentPoint;

    #region Singleton
    
    public static PlayerController Instance;
    
    private void Awake()
    {
        Instance = this;
    }
    #endregion
    
    private void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        shooting = GetComponent<Shooting>();
        currentPoint = -1;
        
        NextPoint();
    }

    public void NextPoint()
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
            shooting.Shoot();
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
}
