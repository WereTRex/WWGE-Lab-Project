using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthComponent : MonoBehaviour
{
    public static event Action OnDead;
    

    [SerializeField] private float _maxHealth;

    private float _currentHealth;
    public float CurrentHealthProperty
    {
        get => _currentHealth;
        private set
        {
            _currentHealth = value;

            if (_healthBar != null)
                _healthBar.UpdateProgressBar(maximum: _maxHealth, current: _currentHealth);
        }
    }

    [Header("UI")]
    [SerializeField] private ProgressBar _healthBar;



    private void Start() => CurrentHealthProperty = _maxHealth;


    public void TakeDamage(float damage)
    {
        CurrentHealthProperty -= damage;

        if (CurrentHealthProperty < 0)
        {
            OnDead?.Invoke();
        }
    }
    public void ReceiveHealing(float healing) => CurrentHealthProperty += healing;
    
    public void ResetHealth() => CurrentHealthProperty = _maxHealth;
}
