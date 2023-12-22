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

    [SerializeField] private bool _useWindupValues = false;
    // Position and Rotation of the Hurtbox when winding up
    [SerializeField] private Vector3 _windupPos;
    [SerializeField] private Vector3 _windupRot;

    [Space(5)]

    // Ending position and rotation of the hurtbox.
    [SerializeField] private Vector3 _endPos;
    [SerializeField] private Vector3 _endRot;

    [SerializeField] private bool _resetPositionOnRecovery = true;


    public override IEnumerator MakeAttack()
    {
        Debug.Log("Attack Started");

        // Setup the Attack.
        SetupAttack();

        // Windup the attack.
        if (_startPos == _windupPos && _startRot == _windupRot)
            yield return new WaitForSeconds(WindupDuration);
        else
        {
            for (float timeElapsed = 0; timeElapsed < WindupDuration; timeElapsed += Time.deltaTime)
            {
                _hurtbox.transform.localPosition = Vector3.Lerp(_startPos, _windupPos, timeElapsed / WindupDuration);
                _hurtbox.transform.localRotation = Quaternion.Lerp(Quaternion.Euler(_startRot), Quaternion.Euler(_windupRot), timeElapsed / WindupDuration);

                yield return null;
            }
        }


        // Commit the Attack.
        for (float timeElapsed = 0; timeElapsed < AttackDuration; timeElapsed += Time.deltaTime)
        {
            _hurtbox.transform.localPosition = Vector3.Lerp(_useWindupValues ? _windupPos : _startPos, _endPos, timeElapsed / AttackDuration);
            _hurtbox.transform.localRotation = Quaternion.Lerp(Quaternion.Euler(_useWindupValues ? _windupRot : _startRot), Quaternion.Euler(_endRot), timeElapsed / AttackDuration);

            yield return null;
        }

        // Recovery.
        if (!_resetPositionOnRecovery)
            yield return new WaitForSeconds(RecoveryDuration);
        else
        {
            for (float timeElapsed = 0; timeElapsed < RecoveryDuration; timeElapsed += Time.deltaTime)
            {
                _hurtbox.transform.localPosition = Vector3.Lerp(_endPos, _startPos, timeElapsed / RecoveryDuration);
                _hurtbox.transform.localRotation = Quaternion.Lerp(Quaternion.Euler(_endRot), Quaternion.Euler(_startRot), timeElapsed / RecoveryDuration);

                yield return null;
            }
        }


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
        if (hit.TryGetComponentThroughParents<HealthComponent>(out HealthComponent healthComponent))
        {
            healthComponent.TakeDamage(AttackDamage);
        }
    }
}