using UnityEngine;
using static UnityEngine.ParticleSystem;

/// <summary> Configuration Information for a Weapon's Damage</summary>
[CreateAssetMenu(fileName = "Damage Config", menuName = "Weapons/Guns/Damage Config", order = 1)]
public class DamageConfigSO : ScriptableObject
{
    public MinMaxCurve DamageCurve;
    public float HitForce;

    // Triggers when you reset the Scriptable Object in the inspector;
    private void Reset()
    {
        DamageCurve.mode = ParticleSystemCurveMode.Curve;
    }

    public int GetDamage(float distance = 0)
    {
        return Mathf.CeilToInt(DamageCurve.Evaluate(distance, Random.value));
    }
}
