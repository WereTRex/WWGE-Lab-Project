using UnityEngine;

/// <summary> A base abstract class for Alternate Firing Modes</summary>
public abstract class AlternateFireSO : ScriptableObject
{
    public float CooldownTime;

    public abstract void AlternateAttack(GameObject triggeringGameObject, Transform raycastOrigin, Transform bulletOrigin);
}
