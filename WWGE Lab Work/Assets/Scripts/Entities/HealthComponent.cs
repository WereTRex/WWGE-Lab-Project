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
            // Clamp the new health between 0 & the entity's maximum health.
            _currentHealth = Mathf.Clamp(value, 0, MaxHealthProperty);
            OnHealthChanged?.Invoke(_currentHealth);

            // If it exists, update the Health Bar.
            if (_healthBar != null)
                _healthBar.UpdateProgressBar(maximum: _maxHealth, current: _currentHealth);
        }
    }

    public bool HasHealth { get => _currentHealth > 0; }
    public bool HasFullHealth { get => _currentHealth >= _maxHealth; }


    [Header("UI")]
    [SerializeField] private ProgressBar _healthBar;



    private void Start() => CurrentHealthProperty = _maxHealth;


    /// <summary> Deal damage to this HealthComponent</summary>
    public void TakeDamage(float damage)
    {
        // Reduce the health by damage.
        CurrentHealthProperty -= damage;

        // Check if we are dead.
        if (CurrentHealthProperty < 0)
        {
            OnDead?.Invoke();
        }
    }
    /// <summary> Heal the HealthComponent by healing</summary>
    public void ReceiveHealing(float healing) => CurrentHealthProperty += healing;
    
    /// <summary> Reset this component's health to its maximum</summary>
    public void ResetHealth() => CurrentHealthProperty = MaxHealthProperty;
}
