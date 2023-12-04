using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttackingBarrier : IState
{
    private readonly Enemy _brain;
    private readonly float _attackDamage;
    private readonly float _attackCooldown;

    private float _attackCooldownRemaining;
    
    public EnemyAttackingBarrier(Enemy brain, float attackDamage, float attackCooldown)
    {
        this._brain = brain;
        this._attackDamage = attackDamage;
        this._attackCooldown = attackCooldown;
    }

    
    public void Tick()
    {
        if (_attackCooldownRemaining > 0) {
            _attackCooldownRemaining -= Time.deltaTime;
            return;
        }

        MakeAttack();
    }

    private void MakeAttack()
    {
        // Damage the initial target.
        if (_brain.InitialTarget.TryGetComponent<HealthComponent>(out HealthComponent healthComponent))
        {
            healthComponent.TakeDamage(_attackDamage);
        }

        // Set the attack cooldown.
        _attackCooldownRemaining = _attackCooldown;
    }


    public void OnEnter()
    {
        _attackCooldownRemaining = 0.2f;
    }
    public void OnExit()
    {

    }
}
