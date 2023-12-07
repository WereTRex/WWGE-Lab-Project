using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Repairable : MonoBehaviour, IInteractable
{
    [SerializeField] protected HealthComponent _healthComponent;
    [SerializeField] protected EntityFaction _entityFaction;
    protected Transform _interactingTransform;

    [Space(5)]
    [SerializeField] protected float _repairTime;
    protected Coroutine _repairCoroutine;


    public virtual void Interact(Transform interactorTransform)
    {
        if (interactorTransform.TryGetComponent<EntityFaction>(out EntityFaction entityFaction))
            if (_entityFaction.IsOpposingFaction(entityFaction.Faction))
                return;

        if (_repairCoroutine == null)
        {
            _interactingTransform = interactorTransform;
            _repairCoroutine = StartCoroutine(Repair());
        }
    }

    protected virtual IEnumerator Repair()
    {
        Debug.Log("Repair Started");
        yield return new WaitForSeconds(_repairTime);
        _healthComponent.ResetHealth();
        _repairCoroutine = null;
        _interactingTransform = null;
        Debug.Log("Repair Completed");
    }


    public void Focused(Transform interactorTransform) { }
    public void Unfocused(Transform interactorTransform)
    {
        if (_repairCoroutine != null && _interactingTransform == interactorTransform)
            StopCoroutine(_repairCoroutine);
    }


    public void InRange(Transform interactorTransform) { }
    public void OutOfRange(Transform interactorTransform)
    {
        if (_repairCoroutine != null && _interactingTransform == interactorTransform)
            StopCoroutine(_repairCoroutine);
    }


    public bool GetInteractionAvailability()
    {
        return this.enabled;
    }
}
