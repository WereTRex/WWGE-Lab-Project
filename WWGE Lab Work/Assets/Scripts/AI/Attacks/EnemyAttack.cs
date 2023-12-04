using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public abstract class EnemyAttack
{
    [SerializeField] private string name;
    
    [SerializeField] protected float _attackDamage;
    [Tooltip("Can this attack hit the same target more than once?")]
        [SerializeField] protected bool _onlyHitOnce;

    [SerializeField] protected float _maxAttackRange;
    public float MaxAttackRange { get => _maxAttackRange; }

    [SerializeField] protected float _attackDuration;

    [SerializeField] protected float _attackRecovery;
    [SerializeField] protected float _attackCooldown;
    protected float _attackCooldownComplete;
    public bool IsInCooldown { get => _attackCooldownComplete > Time.time; }


    protected List<Collider> _hitColliders = new List<Collider>();
    public abstract IEnumerator MakeAttack();
}

[System.Serializable]
public class HurtboxAttack : EnemyAttack
{
    private Coroutine _currentAttackCoroutine;


    [Space(20)]
    [SerializeField] private Hurtbox _hurtbox;

    [Space(15)]
    
    [SerializeField] private Vector3 _startPos;
    [SerializeField] private Vector3 _startRot;
    
    [Space(5)]
    
    [SerializeField] private Vector3 _endPos;
    [SerializeField] private Vector3 _endRot;


    public override IEnumerator MakeAttack()
    {
        Debug.Log("Attack Started");
        
        // Setup the Attack.
        SetupAttack();

        // Commit the Attack.
        float durationElapsed = 0;
        while(durationElapsed <= _attackDuration)
        {
            _hurtbox.transform.localPosition = Vector3.Lerp(_startPos, _endPos, durationElapsed / _attackDuration);
            _hurtbox.transform.localRotation = Quaternion.Lerp(Quaternion.Euler(_startRot), Quaternion.Euler(_endRot), durationElapsed / _attackDuration);

            yield return null;
            durationElapsed += Time.deltaTime;
        }
        yield return new WaitForSeconds(_attackRecovery);

        // The Attack has concluded.
        AttackComplete();
    }
    
    private void SetupAttack()
    {
        // Setup the Hurtbox.
        //_hurtbox.SetupHurtbox(_hurtboxSize);
        _hurtbox.transform.localPosition = _startPos;
        _hurtbox.transform.localRotation = Quaternion.Euler(_startRot);

        // Subscribe to Hurtbox events.
        _hurtbox.OnColliderEntered += HandleHit;
    }
    private void AttackComplete()
    {
        // Set Cooldowm.
        _attackCooldownComplete = Time.time + _attackCooldown;

        // Unsubscribe from Hurtbox events.
        _hurtbox.OnColliderEntered -= HandleHit;

        // Reset the Hurtbox's Position & Rotation.
        _hurtbox.transform.localPosition = _startPos;
        _hurtbox.transform.localRotation = Quaternion.Euler(_startRot);

        // Clear HitColliders list for next attack.
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