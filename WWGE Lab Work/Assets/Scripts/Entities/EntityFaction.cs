using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> A script that allows the representation of an entity's faction.</summary>
[DisallowMultipleComponent]
public class EntityFaction : MonoBehaviour
{
    [field:SerializeField] public Faction Faction { get; private set; }

    public void SetFaction(Faction newFaction, Object setter)
    {
        Debug.Log(setter.name + " has changed the faction of " + this.name + " to: " + newFaction);
        this.Faction = newFaction;
    }

    /// <summary> A function to check whether this entity is part of an inputted faction. </summary>
    /// <returns> True if the factions don't contain an ally, false if they do.</returns>
    public bool IsOpposingFaction(Faction factionToCheck)
    {
        // Check if either faction is unaligned (Will never team with another entity).
        if (Faction == Faction.Unaligned || factionToCheck == Faction.Unaligned)
            return true;

        // Check if this entity is a part of the factionToCheck.
        if ((Faction & factionToCheck) != 0)
            return false;

        // This entity is not a part of the factionToCheck.
        return true;
    }
}
