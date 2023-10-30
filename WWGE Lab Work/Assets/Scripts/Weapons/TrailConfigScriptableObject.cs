using UnityEngine;

/// <summary>
/// A scriptable object used to contain the configuration information for bullet tracers.
/// </summary>
[CreateAssetMenu(fileName = "Trail Config", menuName = "Weapons/Guns/Trail Config", order = 4)]
public class TrailConfigScriptableObject : ScriptableObject
{
    public Material Material;
    public AnimationCurve WidthCurve;
    public float Duration = 0.5f;
    public float MinVertexDistance = 0.1f;
    public Gradient Color;

    public float MissDistance = 100f;
    public float SimulationSpeed = 100f;
}
