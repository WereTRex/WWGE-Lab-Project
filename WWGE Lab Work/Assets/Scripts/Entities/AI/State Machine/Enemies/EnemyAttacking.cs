using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary> A state for when a enemy is attacking the player</summary>
public class EnemyAttacking : IState
{
    private readonly StandardEnemy _brain;
    private readonly EnemyAttack[] _attacks;
    private readonly float _maxAttackRange;


    private Coroutine _attackCoroutine;
    private Coroutine _currentAttack;


    private const float GLOBAL_COOLDOWN = 0.2f;
    private float _globalCooldownRemaining;


    public float MaxAttackRange { get => _maxAttackRange; }
    public bool IsAttacking { get => _currentAttack != null; }


    public EnemyAttacking(StandardEnemy brain, EnemyAttack[] attacks)
    {
        // Set readonly values.
        this._brain = brain;
        this._attacks = attacks;

        // Cache the max attack range.
        _maxAttackRange = 0f;
        foreach (EnemyAttack attack in _attacks)
        {
            if (attack.GetAttackRange() > _maxAttackRange)
                _maxAttackRange = attack.GetAttackRange();
        }
    }


    public void Tick()
    {
        // If we are currently attacking, return.
        if (_currentAttack != null)
            return;

        // If we are still in cooldown from the previous attack, return.
        if (_globalCooldownRemaining > 0)
        {
            _globalCooldownRemaining -= Time.deltaTime;
            return;
        }


        // Check if we are within the maximum attack range.
        /*float distanceToTarget = Vector3.Distance(_brain.transform.position, _brain.GetTarget().position);
        if (distanceToTarget < _maxAttackRange)
        {
            // Attempt to attack
            _attackCoroutine = _brain.StartCoroutine(Attack(distanceToTarget));
        }*/
    }

    public void OnEnter() { }
    public void OnExit()
    {
        // Stop attacking coroutines.
        if (_attackCoroutine != null)
            _brain.StopCoroutine(_attackCoroutine);
        if (_currentAttack != null)
            _brain.StopCoroutine(_currentAttack);

        // Reset the global cooldown.
        _globalCooldownRemaining = 0;
    }

    private IEnumerator Attack(float distanceToTarget)
    {
        // Get a valid attack.
        EnemyAttack attack = GetAttack(distanceToTarget);

        // If there were no valid attacks, return.
        if (attack == null)
            yield break;

        // Make the attack & wait till it is completed.
        yield return (_currentAttack = _brain.StartCoroutine(attack.MakeAttack()));

        // Set the global cooldown & stop attacking.
        _globalCooldownRemaining = GLOBAL_COOLDOWN;
        _currentAttack = null;
    }
    private EnemyAttack GetAttack(float distanceToTarget)
    {
        // Get all attacks that are within range and are not on cooldown.
        List<EnemyAttack> attackList = _attacks.Where(attack => distanceToTarget <= _maxAttackRange && !attack.IsInCooldown).ToList();
        
        // If we have at least one attack, return a random attack. Otherwise, return null.
        return attackList.Count > 0 ? attackList[Random.Range(0, attackList.Count)] : null;
    }
}
