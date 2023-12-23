using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> A class to represent an Entity's Health</summary>
public class HealthComponent : MonoBehaviour
{
    // Events.
    public event Action OnDead;
    public event Action<Vector3, float> OnHealthDecreased; // Vector3 source, float newValue.
    public event Action<Vector3, float> OnHealthIncreased; // Vector3 source, float newValue.


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

            // If it exists, update the Health Bar.
            if (_healthBar != null)
                _healthBar.UpdateProgressBar(maximum: _maxHealth, current: _currentHealth);
        }
    }


    // Useful booleans to have access to.
    public bool HasHealth { get => _currentHealth > 0; }
    public bool HasFullHealth { get => _currentHealth >= _maxHealth; }


    [Header("UI")]
    [SerializeField] private ProgressBar _healthBar;



    private void Awake() => CurrentHealthProperty = _maxHealth;


    /// <summary> Deal damage to this HealthComponent</summary>
    public void TakeDamage(Vector3 damageOrigin, float damage)
    {
        // Reduce the health by 'damage'.
        CurrentHealthProperty -= damage;
        OnHealthDecreased?.Invoke(damageOrigin, CurrentHealthProperty);

        Debug.Log("Take Damage: " + damage);

        // Check if we are dead.
        if (CurrentHealthProperty <= 0)
        {
            OnDead?.Invoke();
        }
    }
    /// <summary> Heal the HealthComponent by healing</summary>
    public void ReceiveHealing(Vector3 healingOrigin, float healing)
    {
        // Increase the health by 'healing'.
        CurrentHealthProperty += healing;
        OnHealthIncreased?.Invoke(healingOrigin, CurrentHealthProperty);
    }

    /// <summary> Reset this component's health to its maximum</summary>
    public void ResetHealth()
    {
        CurrentHealthProperty = MaxHealthProperty;
        OnHealthIncreased?.Invoke(transform.position, CurrentHealthProperty);
    }
}
