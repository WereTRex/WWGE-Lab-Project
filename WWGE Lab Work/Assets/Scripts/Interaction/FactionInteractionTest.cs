using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> (Test Script) A script only allowing interaction between entities of the same faction.</summary>
public class FactionInteractionTest : MonoBehaviour, IInteractable
{
    [SerializeField] private EntityFaction _factionScript;
    private Transform _currentInteractionTransform;


    #region IInteractable Methods
    public void Interact(Transform interactingTransform)
    {
        // If the interacting object is not an ally of this one, then return.
        if (interactingTransform.TryGetComponent<EntityFaction>(out EntityFaction entityFaction))
            if (_factionScript.IsAllyOf(entityFaction.Faction) == false)
                return;

        _currentInteractionTransform = interactingTransform;
        Debug.Log("Interacting Objects share a faction");
    }


    public void Focused(Transform interactingTransform) { }
    public void Unfocused(Transform interactingTransform)
    {
        if (interactingTransform != _currentInteractionTransform)
            return;

        StopInteraction();
    }


    public void InRange(Transform interactingTransform) { }
    public void OutOfRange(Transform interactingTransform)
    {
        if (interactingTransform != _currentInteractionTransform)
            return;

        StopInteraction();
    }


    public bool GetInteractionAvailability()
    {
        return true;
    }
#endregion


    private void StopInteraction()
    {
        _currentInteractionTransform = null;
        Debug.Log("Interaction has Stopped");
    }
}
