using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RagdollController))]
public class UnitStats : MonoBehaviour
{
    [SerializeField] private int hp;
    [SerializeField] private Slider hpBar;
    
    private RagdollController ragdollController;
    
    private void Start()
    {
        hpBar.maxValue = hp;
        UpdateHpBar();
        
        ragdollController = GetComponent<RagdollController>();
    }

    public void GetDamage(int damage)
    {
        hp = Mathf.Max(hp - damage, 0);
        UpdateHpBar();
        
        if (hp == 0)
        {
            Death();
        }
    }

    private void UpdateHpBar()
    {
        hpBar.value = hp;
    }


    private void Death()
    {
        ragdollController.ActivateRagdoll(true);
        hpBar.gameObject.SetActive(false);
        WayPoints.Instance.OnUnitDeath();
    }

    public bool IsAlive()
    {
        return hp > 0;
    }
}
