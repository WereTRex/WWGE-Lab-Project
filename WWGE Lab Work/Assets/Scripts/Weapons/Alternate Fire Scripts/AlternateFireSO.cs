using UnityEngine;

public abstract class AlternateFireSO : ScriptableObject
{
    public float CooldownTime;

    public abstract void AlternateAttack(GameObject triggeringGameObject, Transform raycastOrigin, Transform bulletOrigin);
}
