using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> A class representing a level entrance barrier that can be repaired.</summary>
public class RepairableBarrier : Repairable
{
    [SerializeField] private int _repairStages; // How many stages this barrier's health is broken up into.
    [SerializeField] private Animator _animator;


    private void OnEnable() => HealthComponent.OnHealthChanged += UpdateAnimator;
    private void OnDisable() => HealthComponent.OnHealthChanged -= UpdateAnimator;
    private void Start() => UpdateAnimator(HealthComponent.CurrentHealthProperty); // Set default values of the animator.
    


    // Override this Coroutine so that we only repair in stages.
    protected override IEnumerator Repair()
    {
        Debug.Log("Repair Started");
        // Heal the associated health component until it is full.
        while (!HealthComponent.HasFullHealth)
        {
            yield return new WaitForSeconds(RepairTime / _repairStages);
            HealthComponent.ReceiveHealing(HealthComponent.MaxHealthProperty / _repairStages);
        }

        // Complete the repair.
        RepairCoroutine = null;
        Debug.Log("Repair Completed");
    }


    /// <summary> Update the animator with the current repair stage.</summary>
    private void UpdateAnimator(float newHealth)
    {
        Debug.Log(HealthComponent.CurrentHealthProperty / HealthComponent.MaxHealthProperty);
        
        // Get the current repair stage.
        int stage;
        if (HealthComponent.HasFullHealth)
            stage = _repairStages;
        else if (!HealthComponent.HasHealth)
            stage = 0;
        else
            stage = Mathf.Clamp(Mathf.CeilToInt((HealthComponent.CurrentHealthProperty / HealthComponent.MaxHealthProperty) * _repairStages), 1, _repairStages - 1);

        // Update the animator.
        if (_animator != null)
            _animator.SetInteger("Stage", stage);
    }
}
