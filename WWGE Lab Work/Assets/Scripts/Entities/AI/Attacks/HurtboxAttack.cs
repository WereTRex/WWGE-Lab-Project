using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HurtboxAttack : EnemyAttack
{
    private Coroutine _currentAttackCoroutine;


    [Space(20)]
    [SerializeField] private Hurtbox _hurtbox;

    [Space(15)]

    // Starting position & rotation of the hurtbox.
    [SerializeField] private Vector3 _startPos;
    [SerializeField] private Vector3 _startRot;

    [Space(5)]

    // Ending position and rotation of the hurtbox.
    [SerializeField] private Vector3 _endPos;
    [SerializeField] private Vector3 _endRot;


    public override IEnumerator MakeAttack()
    {
        Debug.Log("Attack Started");

        // Setup the Attack.
        SetupAttack();

        // Commit the Attack.
        float durationElapsed = 0;
        while (durationElapsed <= AttackDuration)
        {
            _hurtbox.transform.localPosition = Vector3.Lerp(_startPos, _endPos, durationElapsed / AttackDuration);
            _hurtbox.transform.localRotation = Quaternion.Lerp(Quaternion.Euler(_startRot), Quaternion.Euler(_endRot), durationElapsed / AttackDuration);

            yield return null;
            durationElapsed += Time.deltaTime;
        }
        yield return new WaitForSeconds(AttackRecovery);

        // The Attack has concluded.
        AttackComplete();
    }

    private void SetupAttack()
    {
        // Setup the Hurtbox.
        _hurtbox.transform.localPosition = _startPos;
        _hurtbox.transform.localRotation = Quaternion.Euler(_startRot);

        // Subscribe to Hurtbox events.
        _hurtbox.OnColliderEntered += HandleHit;
    }
    private void AttackComplete()
    {
        // Set Cooldowm.
        AttackCooldownCompletionTime = Time.time + AttackCooldown;

        // Unsubscribe from Hurtbox events.
        _hurtbox.OnColliderEntered -= HandleHit;

        // Reset the Hurtbox's Position & Rotation.
        _hurtbox.transform.localPosition = _startPos;
        _hurtbox.transform.localRotation = Quaternion.Euler(_startRot);

        // Clear HitColliders list for next attack.
        HitColliders.Clear();
    }

    private void HandleHit(Collider hit)
    {
        // If we should only hit a collider once during the attack, and we have already hit this collider, return.
        if (onlyHitOnce && HitColliders.Contains(hit))
            return;

        // Add this collider to the list of colliders we have already hit.
        HitColliders.Add(hit);


        // Deal Damage (& Eventually Force).
        if (hit.TryGetComponent<HealthComponent>(out HealthComponent healthComponent))
        {
            healthComponent.TakeDamage(AttackDamage);
        }
    }
}