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
        List<GameObject> toggleObjects = FindChildrenWithTag(triggeringGameObject.transform, ToggleTag);

        // Loop through each found child & toggle whether they are active.
        for (int i = 0; i < toggleObjects.Count; i++)
        {
            toggleObjects[i].SetActive(!toggleObjects[i].activeSelf);
        }
    }

    /// <summary> Find all children with a certain tag.</summary>
    private List<GameObject> FindChildrenWithTag(Transform parent, string tag)
    {
        List<GameObject> childrenWithTag = new List<GameObject>();

        // Loop through each child.
        foreach (Transform child in parent)
        {
            // If this child has the appropriate tag, add it to the list.
            if (child.CompareTag(tag))
                childrenWithTag.Add(child.gameObject);

            // If this child has children, check them for the tag.
            if (child.childCount > 0)
                childrenWithTag.AddRange(FindChildrenWithTag(child, tag));
        }

        // Return the found children.
        return childrenWithTag;
    }
}
