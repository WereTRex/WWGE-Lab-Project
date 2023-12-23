using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary> A base class from which all types of enemy attack inherit.</summary>
[System.Serializable]
public abstract class EnemyAttack
{
    [SerializeField] private string name; // The attack's name (Used for organisation in the inspector).

    [Space(5)]

    [SerializeField] protected float AttackDamage;
    [Tooltip("Can this attack hit the same target more than once?")]
        [SerializeField] protected bool onlyHitOnce;

    [Space(5)]

    [SerializeField] protected bool OnlyHitEnemies;
    [SerializeField] protected EntityFaction EntityFaction;

    [Space(5)]

    [SerializeField] protected float MaxAttackRange; // The maximum range that this attack can be made from.
    public float GetAttackRange() => MaxAttackRange;

    [SerializeField] protected float MaxAttackAngle = 360f;
    public float GetAttackAngle() => MaxAttackAngle;

    [Space(5)]

    [SerializeField] protected float WindupDuration; // How long after starting this attack until it can deal damage.
    [SerializeField] protected float AttackDuration; // How long this attack lasts (Time between the Windup and Recovery).
    [SerializeField] protected float RecoveryDuration; // How long after making the attack that the attacker must wait until they can act again.

    [Space(5)]

    [SerializeField] protected float AttackCooldown; // How long after use until this attack can be used again.
    protected float AttackCooldownCompletionTime; // The time that the attack cooldown will have completed by.
    public bool IsInCooldown { get => AttackCooldownCompletionTime > Time.time; }



    protected List<Collider> HitColliders = new List<Collider>(); // A list of colliders already hit during the current instance of the attack.
    public abstract IEnumerator MakeAttack();
}

public static class EnemyAttackExtensions
{
    public static float GetMaximumRange(this EnemyAttack[] enemyAttacks)
    {
        if (enemyAttacks.Length == 0)
            return 0f;

        float maxRange = enemyAttacks[0].GetAttackRange();
        for (int i = 1; i < enemyAttacks.Length; i++)
        {
            if (enemyAttacks[i].GetAttackRange() > maxRange)
                maxRange = enemyAttacks[i].GetAttackRange();
        }

        return maxRange;
    }
}