using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthComponent : MonoBehaviour
{
    public event Action OnDead;
    public event Action<float> OnHealthChanged;
    

    [SerializeField] private float _maxHealth;
    public float MaxHealthProperty { get => _maxHealth; private set => _maxHealth = value; }

    private float _currentHealth;
    public float CurrentHealthProperty
    {
        get => _currentHealth;
        private set
        {
            _currentHealth = Mathf.Clamp(value, 0, MaxHealthProperty);
            OnHealthChanged?.Invoke(_currentHealth);

            if (_healthBar != null)
                _healthBar.UpdateProgressBar(maximum: _maxHealth, current: _currentHealth);
        }
    }

    public bool HasHealth { get => _currentHealth > 0; }
    public bool HasFullHealth { get => _currentHealth >= _maxHealth; }


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
    
    public void ResetHealth() => CurrentHealthProperty = MaxHealthProperty;
}
