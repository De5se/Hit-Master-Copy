using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WayPoints : MonoBehaviour
{
    [SerializeField] private List<UnitsList> enemiesForNextWayPoint = new List<UnitsList>();
    private int currentPoint = 0;
    
    #region Singleton
    
    public static WayPoints Instance;
    
    private void Awake()
    {
        Instance = this;
    }
    #endregion
    
    
    public void OnUnitDeath()
    {
        GoToNextIfUnitsDead();
    }

    private void GoToNextIfUnitsDead()
    {
        if (enemiesForNextWayPoint.Count <= currentPoint){return;}
        
        foreach (var unit in enemiesForNextWayPoint[currentPoint].unitStats)
        {
            if (unit.IsAlive()) {return;}
        }

        currentPoint++;
        PlayerController.Instance.NextPoint();
        GoToNextIfUnitsDead();
    }
}

[System.Serializable]
public class UnitsList
{
    public List<UnitStats> unitStats = new List<UnitStats>();
}
