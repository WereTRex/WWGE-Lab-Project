using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary> A base class from which all types of enemy attack inherit.</summary>
[System.Serializable]
public abstract class EnemyAttack
{
    [SerializeField] private string name; // The attack's name (Used for organisation in the inspector).
    
    
    [SerializeField] protected float AttackDamage;
    [Tooltip("Can this attack hit the same target more than once?")]
        [SerializeField] protected bool onlyHitOnce;


    [SerializeField] protected float maxAttackRange; // The maximum range that this attack can be made from.
    public float GetMaxAttackRange() => maxAttackRange;


    [SerializeField] protected float AttackDuration; // How long this attack lasts.


    [SerializeField] protected float AttackRecovery; // How long after making the attack that the attacker must wait until they can act again.
    [SerializeField] protected float AttackCooldown; // How long after use until this attack can be used again.
    protected float AttackCooldownCompletionTime; // The time that the attack cooldown will have completed by.
    public bool IsInCooldown { get => AttackCooldownCompletionTime > Time.time; }



    protected List<Collider> HitColliders = new List<Collider>(); // A list of colliders already hit during the current instance of the attack.
    public abstract IEnumerator MakeAttack();
}