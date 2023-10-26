using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthComponent : MonoBehaviour
{
    public static event Action OnDead;
    

    [SerializeField] private float _maxHealth;

    private float _currentHealth;
    public float CurrentHealth => _currentHealth;


    private void Start()
    {
        _currentHealth = _maxHealth;
    }


    public void TakeDamage(float damage)
    {
        _currentHealth -= damage;

        if (_currentHealth < 0)
        {
            OnDead?.Invoke();
        }
    }

    public void ReceiveHealing(float healing)
    {
        _currentHealth += healing;
    }



    public void ResetHealth()
    {
        _currentHealth = _maxHealth;
    }
}
