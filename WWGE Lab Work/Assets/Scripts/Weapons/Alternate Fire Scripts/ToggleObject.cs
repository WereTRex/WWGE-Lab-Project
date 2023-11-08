using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Toggle Alt Fire", menuName = "Weapons/Guns/Alternate Fire/Toggle Object")]
public class ToggleObject : AlternateFireSO
{
    [Space(5)]
    public string ToggleTag = "Toggleable";

    // Note: We are not storing the triggering gameObject as I haven't prepared for cloning Scriptable Objects.
    // Note: This is a bad way to do this, but it'll work for now.
    public override void AlternateAttack(GameObject triggeringGameObject, Transform raycastOrigin, Transform bulletOrigin)
    {
        // Find the Toggleable Object in the child of the triggering.
        GameObject toggleObject = null;
        foreach (Transform child in triggeringGameObject.transform)
        {
            if (child.CompareTag(ToggleTag))
            {
                toggleObject = child.gameObject;
                break;
            }
        }

        toggleObject.SetActive(!toggleObject.activeSelf);
    }
}
