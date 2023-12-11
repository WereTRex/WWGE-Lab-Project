using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary> A class representing the brain of a Standard Enemy.</summary>
public class StandardEnemy : Enemy
{
    [Header("Outside Variables")]
    [SerializeField] private float _stateTransitionDistance;

    [Header("Barrier Destruction Variables")]
    [SerializeField] private float _attackDamage;
    [SerializeField] private float _attackCooldown;

    [Header("Taunting Variables")]
    [SerializeField] private float _tauntDuration;

    [Header("Stagger Variables")]
    [SerializeField] private float _baseStaggerDuration;

    [Header("Chasing Variables")]
    [SerializeField] private NavMeshAgent _agent;

    [Header("Attacking Variables")]
    [SerializeField] private HurtboxAttack[] _attacks;

    
    private new void Awake()
    {
        base.Awake();
        
        #region State Machine Setup
        // States.
        var outsideState = new EnemyOutside(this, _agent);
        var attackingBarrierState = new EnemyAttackingBarrier(this, _attackDamage, _attackCooldown);
        var tauntingState = new EnemyTaunting();
        var chasingState = new EnemyChasing(this, _agent);
        var attackingState = new EnemyAttacking(this, _attacks);
        var staggerState = new EnemyStagger();
        var deathState = new EnemyDead(this.gameObject);

        // Transitions.
        StateMachine.AddAnyTransition(deathState, EnemyDead());

        At(outsideState, attackingBarrierState, ReachedBarrier());

        At(attackingBarrierState, tauntingState, DestroyedBarrier());

        At(tauntingState, chasingState, TauntCompleted());
        At(tauntingState, staggerState, Staggered());

        At(chasingState, attackingState, WithinAttackRange());
        At(chasingState, staggerState, Staggered());

        At(attackingState, chasingState, OutOfAttackRange());
        At(attackingState, staggerState, Staggered());

        At(staggerState, chasingState, OutsideStaggerCompleted());
        At(staggerState, chasingState, InsideStaggerCompleted());


        void At(IState from, IState to, Func<bool> condition) => StateMachine.AddTransition(from, to, condition);

        // Set the initial state.
        StateMachine.SetState(outsideState);


        // Transition Conditions.
        Func<bool> ReachedBarrier() => () => InitialTarget != null && Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), new Vector3(InitialTarget.position.x, 0, InitialTarget.position.z)) < _stateTransitionDistance;
        Func<bool> DestroyedBarrier() => () => InitialTargetHealth != null && InitialTargetHealth.HasHealth == false;
        
        Func<bool> TauntCompleted() => () => tauntingState.TauntDurationElapsed >= _tauntDuration;

        Func<bool> OutsideStaggerCompleted() => () => staggerState.StaggerDurationElapsed >= _baseStaggerDuration;
        Func<bool> InsideStaggerCompleted() => () => staggerState.StaggerDurationElapsed >= _baseStaggerDuration;
        Func<bool> Staggered() => () => false;

        Func<bool> WithinAttackRange() => () => Vector3.Distance(transform.position, Target.position) < attackingState.MaxAttackRange;
        Func<bool> OutOfAttackRange() => () => (Vector3.Distance(transform.position, Target.position) > attackingState.MaxAttackRange) && attackingState.IsAttacking == false;

        Func<bool> EnemyDead() => () => HealthComponent.HasHealth == false;
        #endregion
    }
}
