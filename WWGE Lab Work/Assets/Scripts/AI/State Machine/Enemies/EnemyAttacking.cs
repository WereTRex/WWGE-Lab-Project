using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyAttacking : IState
{
    private readonly StandardEnemy _brain;
    private readonly EnemyAttack[] _attacks;
    private readonly float _maxAttackRange;


    private Coroutine _attackCoroutine;
    private Coroutine _currentAttack;

    // Temp.
    private const float GLOBAL_COOLDOWN = 0.3f;
    private float _globalCooldownRemaining;


    public float MaxAttackRange { get => _maxAttackRange; }
    public bool IsAttacking { get => _currentAttack != null; }


    public EnemyAttacking(StandardEnemy brain, EnemyAttack[] attacks)
    {
        this._brain = brain;
        this._attacks = attacks;

        // Calculate the max range.
        _maxAttackRange = 0f;
        foreach (EnemyAttack attack in _attacks)
        {
            if (attack.MaxAttackRange > _maxAttackRange)
                _maxAttackRange = attack.MaxAttackRange;
        }
    }


    public void Tick()
    {
        if (_currentAttack != null)
            return;
        if (_globalCooldownRemaining > 0f) {
            _globalCooldownRemaining -= Time.deltaTime;
            return;
        }

        float distanceToTarget = Vector3.Distance(_brain.transform.position, _brain.GetTarget().position);
        if (distanceToTarget < _maxAttackRange)
        {
            _attackCoroutine = _brain.StartCoroutine(Attack(distanceToTarget));
        }
    }

    public void OnEnter()
    {
        Debug.Log("Attacking");
    }
    public void OnExit()
    {
        if (_attackCoroutine != null)
            _brain.StopCoroutine(_attackCoroutine);
        if (_currentAttack != null)
            _brain.StopCoroutine(_currentAttack);

        _globalCooldownRemaining = 0f;
    }

    private IEnumerator Attack(float distanceToTarget)
    {
        EnemyAttack attack = GetAttack(distanceToTarget);

        yield return (_currentAttack = _brain.StartCoroutine(attack.MakeAttack()));

        _globalCooldownRemaining = GLOBAL_COOLDOWN;
        _currentAttack = null;
    }
    private EnemyAttack GetAttack(float distanceToTarget)
    {
        return _attacks.Where(attack => distanceToTarget <= _maxAttackRange).FirstOrDefault();
    }
}
