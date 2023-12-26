using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

/// <summary> A class representing a level entrance barrier that can be repaired.</summary>
public class RepairableBarrier : Repairable
{
    [SerializeField] private int _repairStages; // How many stages this barrier's health is broken up into.

    [Space(10)]

    [SerializeField] private NavMeshLink _link;


    [Header("Animation")]
    [SerializeField] private Animator _anim;


    public bool IsActive => HealthComponent.HasHealth;


    private void OnEnable()
    {
        HealthComponent.OnHealthIncreased += UpdateBarrier;
        HealthComponent.OnHealthDecreased += UpdateBarrier;
    }
    private void OnDisable()
    {
        HealthComponent.OnHealthIncreased -= UpdateBarrier;
        HealthComponent.OnHealthDecreased -= UpdateBarrier;
    }
    private void Start() => UpdateBarrier(Vector3.zero, HealthComponent.CurrentHealthProperty); // Set default values of the animator.
    


    // Override this Coroutine so that we only repair in stages.
    protected override IEnumerator Repair()
    {
        Debug.Log("Repair Started");
        // Heal the associated health component until it is full.
        while (!HealthComponent.HasFullHealth)
        {
            yield return new WaitForSeconds(RepairTime / _repairStages);
            HealthComponent.ReceiveHealing(transform.position, HealthComponent.MaxHealthProperty / _repairStages);
        }

        // Complete the repair.
        RepairCoroutine = null;
        Debug.Log("Repair Completed");
    }


    /// <summary> Update the animator with the current repair stage.</summary>
    private void UpdateBarrier(Vector3 origin, float newHealth)
    {
        // Calculate the percentage of health remaining (1 = 0% remaining, 1 = 100% remaining).
        float blend = 1f - Mathf.Clamp01(HealthComponent.CurrentHealthProperty / HealthComponent.MaxHealthProperty);
        
        // Enable the NavMeshLink if the barrier is destroyed.
        if (blend == 1)
            _link.enabled = true;
        else
            _link.enabled = false;

        // Update the animator.
        if (_anim != null)
            _anim.SetFloat("Stage", blend);
    }
}
