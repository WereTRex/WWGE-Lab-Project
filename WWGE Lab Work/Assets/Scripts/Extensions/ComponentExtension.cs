using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ComponentExtension
{
    public static bool TryGetComponentThroughParents<T>(this Component child, out T component)
    {
        Transform targetTransform = child.transform;
        do
        {
            if (targetTransform.TryGetComponent<T>(out component))
                return true;

            targetTransform = targetTransform.parent;
        } while (targetTransform != null);

        return false;
    }
}
