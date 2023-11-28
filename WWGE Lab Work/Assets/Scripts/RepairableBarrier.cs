using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RepairableBarrier : Repairable
{
    [SerializeField] private int _repairStages;
    [SerializeField] private Animator _animator;


    private void OnEnable() => _healthComponent.OnHealthChanged += UpdateAnimator;
    private void OnDisable() => _healthComponent.OnHealthChanged -= UpdateAnimator;
    private void Start()
    {
        UpdateAnimator(_healthComponent.CurrentHealthProperty);
    }


    protected override IEnumerator Repair()
    {
        Debug.Log("Repair Started");
        // Heal the associated health component until it is full.
        while (!_healthComponent.HasFullHealth)
        {
            yield return new WaitForSeconds(_repairTime / _repairStages);
            _healthComponent.ReceiveHealing(_healthComponent.MaxHealthProperty / _repairStages);
        }

        // Complete the repair.
        _repairCoroutine = null;
        Debug.Log("Repair Completed");
    }


    private void UpdateAnimator(float newHealth)
    {
        Debug.Log(_healthComponent.CurrentHealthProperty / _healthComponent.MaxHealthProperty);
        
        int stage;
        if (_healthComponent.HasFullHealth)
            stage = _repairStages;
        else if (!_healthComponent.HasHealth)
            stage = 0;
        else
            stage = Mathf.Clamp(Mathf.CeilToInt((_healthComponent.CurrentHealthProperty / _healthComponent.MaxHealthProperty) * _repairStages), 1, _repairStages - 1);

        if (_animator != null)
            _animator.SetInteger("Stage", stage);
    }
}
