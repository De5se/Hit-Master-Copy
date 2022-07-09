using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RagdollController))]
public class UnitStats : MonoBehaviour
{
    [SerializeField] private int hp;
    
    private RagdollController ragdollController;
    [SerializeField] private Slider hpBar;

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
            ragdollController.ActivateRagdoll(true);
        }
    }

    private void UpdateHpBar()
    {
        if (hp == 0)
        {
            hpBar.gameObject.SetActive(false);
        }
        hpBar.value = hp;
    }
}
