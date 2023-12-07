using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable
{
    public void Interact(Transform interactorTransform);

    public void Focused(Transform interactorTransform);
    public void Unfocused(Transform interactorTransform);

    public void InRange(Transform interactorTransform);
    public void OutOfRange(Transform interactorTransform);

    public bool GetInteractionAvailability();
}
