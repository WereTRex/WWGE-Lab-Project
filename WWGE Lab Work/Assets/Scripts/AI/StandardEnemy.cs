using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class StandardEnemy : MonoBehaviour
{
    [SerializeField] private Transform _target;
    public Transform GetTarget() => _target;
    
    [SerializeField] private HealthComponent _healthComponent;


    [Header("Taunting Variables")]
    [SerializeField] private float _tauntDuration;

    [Header("Stagger Variables")]
    [SerializeField] private float _baseStaggerDuration;

    [Header("Chasing Variables")]
    [SerializeField] private NavMeshAgent _agent;

    [Header("Attacking Variables")]
    [SerializeField] private HurtboxAttack[] _attacks;

    
    private StateMachine _stateMachine;
    private void Awake()
    {
        _stateMachine = new StateMachine();

        // States.
        var tauntingState = new EnemyTaunting();
        var chasingState = new EnemyChasing(this, _agent);
        var attackingState = new EnemyAttacking(this, _attacks);
        var staggerState = new EnemyStagger();
        var deathState = new EnemyDead(this.gameObject);

        // Transitions.
        _stateMachine.AddAnyTransition(deathState, EnemyDead());

        At(tauntingState, chasingState, TauntCompleted());
        At(tauntingState, staggerState, Staggered());

        At(chasingState, attackingState, WithinAttackRange());
        At(chasingState, staggerState, Staggered());

        At(attackingState, chasingState, OutOfAttackRange());
        At(attackingState, staggerState, Staggered());

        At(staggerState, chasingState, OutsideStaggerCompleted());
        At(staggerState, chasingState, InsideStaggerCompleted());


        void At(IState from, IState to, Func<bool> condition) => _stateMachine.AddTransition(from, to, condition);

        // Set the initial state.
        _stateMachine.SetState(tauntingState);

        // Transition Conditions.
        Func<bool> TauntCompleted() => () => tauntingState.TauntDurationElapsed >= _tauntDuration;

        Func<bool> OutsideStaggerCompleted() => () => staggerState.StaggerDurationElapsed >= _baseStaggerDuration;
        Func<bool> InsideStaggerCompleted() => () => staggerState.StaggerDurationElapsed >= _baseStaggerDuration;
        Func<bool> Staggered() => () => false;

        Func<bool> WithinAttackRange() => () => Vector3.Distance(transform.position, _target.position) < attackingState.MaxAttackRange;
        Func<bool> OutOfAttackRange() => () => (Vector3.Distance(transform.position, _target.position) > attackingState.MaxAttackRange) && attackingState.IsAttacking == false;

        Func<bool> EnemyDead() => () => _healthComponent.HasHealth == false;
    }

    private void Update() => _stateMachine.Tick();
}
