using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> Allows an object to be interacted with by an entity of the same faction and regain health.</summary>
public class Repairable : MonoBehaviour, IInteractable
{
    [SerializeField] protected HealthComponent HealthComponent;
    [SerializeField] protected EntityFaction EntityFaction;
    protected Transform InteractingTransform;

    [Space(5)]
    [SerializeField] protected float RepairTime;
    protected Coroutine RepairCoroutine;


    /// <summary> Start interacting with this object</summary>
    public virtual void Interact(Transform interactorTransform)
    {
        if (interactorTransform.TryGetComponent<EntityFaction>(out EntityFaction entityFaction))
            if (EntityFaction.IsOpposingFaction(entityFaction.Faction))
                return;

        if (RepairCoroutine == null)
        {
            InteractingTransform = interactorTransform;
            RepairCoroutine = StartCoroutine(Repair());
        }
    }

    protected virtual IEnumerator Repair()
    {
        Debug.Log("Repair Started");
        yield return new WaitForSeconds(RepairTime);
        HealthComponent.ResetHealth();
        RepairCoroutine = null;
        InteractingTransform = null;
        Debug.Log("Repair Completed");
    }


    public void Focused(Transform interactorTransform) { }
    public void Unfocused(Transform interactorTransform)
    {
        if (RepairCoroutine != null && InteractingTransform == interactorTransform)
            StopCoroutine(RepairCoroutine);
    }


    public void InRange(Transform interactorTransform) { }
    public void OutOfRange(Transform interactorTransform)
    {
        if (RepairCoroutine != null && InteractingTransform == interactorTransform)
            StopCoroutine(RepairCoroutine);
    }


    public bool GetInteractionAvailability()
    {
        return this.enabled;
    }
}
