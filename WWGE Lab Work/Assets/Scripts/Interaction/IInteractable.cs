using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> An interface for all scripts that can be interacted with.</summary>
public interface IInteractable
{
    /// <summary> Start the interaction with this object</summary>
    /// <param name="interactorTransform"> The transform starting the interaction.</param>
    public void Interact(Transform interactorTransform);


    /// <summary> Should be triggered when an interactor object has started looking at this object.</summary>
    /// <param name="interactorTransform"> The interactor transform looking at us.</param>
    public void Focused(Transform interactorTransform);
    /// <summary> Should be triggered when an interactor object has looked away from this object.</summary>
    /// <param name="interactorTransform"> The interactor transform that looked away.</param>
    public void Unfocused(Transform interactorTransform);


    /// <summary> Should be triggered when this object first enteres the interaction range of an interactor object.</summary>
    /// <param name="interactorTransform"> The interactor transform that entered range.</param>
    public void InRange(Transform interactorTransform);
    /// <summary> Should be triggered when this object leaves the interaction range of an interactor object</summary>
    /// <param name="interactorTransform"> The interactor transform that left range.</param>
    public void OutOfRange(Transform interactorTransform);


    /// <summary> Returns true if this object can currently be interacted with, and false if it cannot.</summary>
    public bool GetInteractionAvailability();
}
