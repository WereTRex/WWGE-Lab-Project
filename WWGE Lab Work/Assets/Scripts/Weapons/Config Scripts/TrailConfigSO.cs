using UnityEngine;

/// <summary> Configuration Information for the projectile trail of a Weapon.</summary>
[CreateAssetMenu(fileName = "Trail Config", menuName = "Weapons/Guns/Trail Config", order = 4)]
public class TrailConfigSO : ScriptableObject
{
    public Material Material;
    public AnimationCurve WidthCurve;
    public float Duration = 0.5f;
    public float MinVertexDistance = 0.1f;
    public Gradient Colour;

    [Space(5)]

    public float SimulationSpeed = 100f;
    public float MissDistance = 100f;
}
