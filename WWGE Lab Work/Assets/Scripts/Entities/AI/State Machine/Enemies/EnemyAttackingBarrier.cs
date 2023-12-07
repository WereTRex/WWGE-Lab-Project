using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> A state for when a enemy is attacking a level entrance barrier</summary>
public class EnemyAttackingBarrier : IState
{
    private readonly Enemy _brain;
    private readonly float _attackDamage;
    private readonly float _attackCooldown;

    private float _attackCooldownRemaining;
    
    public EnemyAttackingBarrier(Enemy brain, float attackDamage, float attackCooldown)
    {
        // Set readonly values.
        this._brain = brain;
        this._attackDamage = attackDamage;
        this._attackCooldown = attackCooldown;
    }

    
    public void Tick()
    {
        // If our attack is on cooldown, return.
        if (_attackCooldownRemaining > 0) {
            _attackCooldownRemaining -= Time.deltaTime;
            return;
        }

        // Otherwise, make an attack.
        MakeAttack();
    }

    private void MakeAttack()
    {
        // Damage the initial target (Typically the barrier).
        if (_brain.InitialTarget.TryGetComponent<HealthComponent>(out HealthComponent healthComponent))
        {
            healthComponent.TakeDamage(_attackDamage);
        }

        // Set the attack cooldown.
        _attackCooldownRemaining = _attackCooldown;
    }


    public void OnEnter() => _attackCooldownRemaining = 0.2f; // On entry, set the initial attack cooldown.
    public void OnExit() { }
}
