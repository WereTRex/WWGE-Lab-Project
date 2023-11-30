using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public abstract class EnemyAttack
{
    [SerializeField] protected float _attackDamage;
    [Tooltip("Can this attack hit the same target more than once?")]
        [SerializeField] protected bool _onlyHitOnce;


    [SerializeField] protected float _maxAttackRange;
    public float MaxAttackRange { get => _maxAttackRange; }


    [SerializeField] protected float _attackDuration;



    protected List<Collider> _hitColliders = new List<Collider>();
    public abstract IEnumerator MakeAttack();
}

[System.Serializable]
public class HurtboxAttack : EnemyAttack
{
    private Coroutine _currentAttackCoroutine;


    [Space(20)]
    [SerializeField] private Hurtbox _hurtbox;
    [SerializeField] private Vector3 _hurtboxSize;

    [Space(15)]

    [SerializeField] private Vector3 _startingAngle;
    [SerializeField] private Vector3 _sweepAxis;
    [SerializeField] private float _totalSweepAngle;


    public override IEnumerator MakeAttack()
    {
        Debug.Log("Attack Started");
        
        // Setup stuff for the attack.
        SetupAttack();

        // Get the speed we should sweep at.
        float sweepSpeed = _totalSweepAngle / _attackDuration;


        // Sweep the hurtbox.
        float sweepDurationRemaining = _attackDuration;
        while(sweepDurationRemaining > 0)
        {
            // Rotate the Hurtbox.
            _hurtbox.transform.Rotate(_sweepAxis, sweepSpeed * Time.deltaTime, Space.Self);
            Debug.Log("New Rotation: " + _hurtbox.transform.localRotation);

            // Decrement time Remaining.
            yield return null;
            sweepDurationRemaining -= Time.deltaTime;
            Debug.Log("Duration Remaining: " + sweepDurationRemaining);
        }


        // The attack has completed.
        AttackComplete();
    }
    private void SetupAttack()
    {
        // Setup the Hurtbox.
        _hurtbox.transform.localRotation = Quaternion.Euler(_startingAngle);
        _hurtbox.SetupHurtbox(_hurtboxSize.x, _hurtboxSize.y, _hurtboxSize.z);

        // Subscribe to events.
        _hurtbox.OnColliderEntered += HandleHit;

    }
    private void AttackComplete()
    {
        _hurtbox.OnColliderEntered -= HandleHit;
        _hitColliders.Clear();
    }

    private void HandleHit(Collider hit)
    {
        if (_hitColliders.Contains(hit))
            return;
        _hitColliders.Add(hit);


        if (hit.TryGetComponent<HealthComponent>(out HealthComponent healthComponent)) {
            healthComponent.TakeDamage(_attackDamage);
        }
    }
}