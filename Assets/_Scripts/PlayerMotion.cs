using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class PlayerMotion : MonoBehaviour
{
    private NavMeshAgent navMeshAgent;
    [SerializeField] private Transform wayPoints;
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


    private void WinGame()
    {
        Debug.Log("Win Game)");
    }
}
