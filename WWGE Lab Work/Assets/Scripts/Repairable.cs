using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Repairable : MonoBehaviour, IInteractable
{
    [SerializeField] private HealthComponent _healthComponent;
    [SerializeField] private EntityFaction _entityFaction;

    [Space(5)]
    [SerializeField] private float _repairTime;
    private Coroutine _repairCoroutine;

    public void Interact(Transform interactorTransform)
    {
        if (interactorTransform.TryGetComponent<EntityFaction>(out EntityFaction entityFaction))
            if (_entityFaction.IsOpposingFaction(entityFaction.Faction))
                return;

        if (_repairCoroutine == null)
            _repairCoroutine = StartCoroutine(Repair());
    }

    private IEnumerator Repair()
    {
        Debug.Log("Repair Started");
        yield return new WaitForSeconds(_repairTime);
        _healthComponent.ResetHealth();
        _repairCoroutine = null;
        Debug.Log("Repair Completed");
    }


    public void Focused(Transform interactorTransform) { }
    public void Unfocused(Transform interactorTransform)
    {
        if (_repairCoroutine != null)
            StopCoroutine(_repairCoroutine);
    }


    public void InRange(Transform interactorTransform) { }
    public void OutOfRange(Transform interactorTransform)
    {
        if (_repairCoroutine != null)
            StopCoroutine(_repairCoroutine);
    }


    public bool GetInteractionAvailability()
    {
        return this.enabled;
    }
}
