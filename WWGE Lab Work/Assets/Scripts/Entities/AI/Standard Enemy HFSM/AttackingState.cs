using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityHFSM;

namespace EnemyStates.Standard
{
    public abstract class AttackingState : State
    {
        public override abstract string Name { get; }


        protected readonly MonoBehaviour Mono;
        protected readonly EnemyAttack[] EnemyAttacks; // Note: Arrays are reference values, so changing the default changes this :P


        protected Coroutine AttackCoroutine;
        public float MaxAttackRange { get; protected set; }

        protected Coroutine _cachedAttackUpdateCoroutine;
        private const float CACHED_INFO_UPDATE_DELAY = 0.3f;

        public AttackingState(MonoBehaviour mono, EnemyAttack[] attacks)
        {
            this.Mono = mono;
            this.EnemyAttacks = attacks;
        }


        public override void Init()
        {
            base.Init();

            SetCachedValues();
        }
        public override void OnEnter()
        {
            base.OnEnter();

            _cachedAttackUpdateCoroutine = Mono.StartCoroutine(UpdateCachedAttackValues());
        }
        public override void OnExit()
        {
            base.OnExit();

            if (_cachedAttackUpdateCoroutine != null)
                Mono.StopCoroutine(_cachedAttackUpdateCoroutine);
        }

        // Prevent transitioning if we are in the middle of an attack (Unless the transition uses forceInstantly).
        protected override bool CanExit() => AttackCoroutine != null;


        protected IEnumerator Attack(float distanceToTarget, float angleToTarget)
        {
            // Check for a valid attack.
            if (TryGetAttack(distanceToTarget, angleToTarget, out EnemyAttack attack))
            {
                // Start the attack & wait until it has completed (Note: This includes recovery time).
                yield return Mono.StartCoroutine(attack.MakeAttack());

                // Allow further attacks.
                AttackCoroutine = null;
            }
        }
        private bool TryGetAttack(float distanceToTarget, float angleToTarget, out EnemyAttack foundAttack)
        {
            // Get all attacks that are within range & are not on cooldown.
            List<EnemyAttack> attackList = EnemyAttacks.Where(attack => (distanceToTarget <= attack.GetAttackRange()) && (angleToTarget <= attack.GetAttackAngle() / 2f) && !attack.IsInCooldown).ToList();

            if (attackList.Count > 0)
            {
                // If we have any valid attacks, output a random one and return true.
                foundAttack = attackList[Random.Range(0, attackList.Count)];
                return true;
            } else {
                // There were no valid attacks, return false.
                foundAttack = null;
                return false;
            }
        }



        private IEnumerator UpdateCachedAttackValues()
        {
            while (true)
            {
                SetCachedValues();

                // Wait for the next cached info update time.
                yield return new WaitForSeconds(CACHED_INFO_UPDATE_DELAY);
            }
        }
        private void SetCachedValues()
        {
            // Update the maximum attack range.
            MaxAttackRange = EnemyAttacks[0].GetAttackRange();
            for (int i = 0; i < EnemyAttacks.Length; i++)
            {
                if (EnemyAttacks[i].GetAttackRange() > MaxAttackRange)
                    MaxAttackRange = EnemyAttacks[i].GetAttackRange();
            }
        }
    }
}