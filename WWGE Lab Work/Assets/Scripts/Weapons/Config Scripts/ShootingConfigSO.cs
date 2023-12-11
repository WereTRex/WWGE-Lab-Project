using System.Linq;
using UnityEngine;

/// <summary> Configuration information for one of a Gun's Firing Modes</summary>
[CreateAssetMenu(fileName = "Shooting Config", menuName = "Weapons/Guns/Shooting Config", order = 2)]
public class ShootingConfigSO : ScriptableObject
{
    public FiringType FiringType;

    [Space(5)]

    public bool IsHitscan = true;
    public Bullet BulletPrefab;
    public float BulletLaunchForce = 1000f;

    [Space(5)]

    public LayerMask HitMask;
    public float FireDelay = 0.25f;


    [Header("Bullet Spread")]
    [Min(1)] public int BulletsPerShot = 1;
    [Range(0f, 180f)]
    public float MaxBulletAngle = 0f;
    public bool UseWeightedSpread = false;


    [Header("Recoil")]
    public float RecoilStrength = 5f;
    public float MaxRecoil = 15f;

    public float RecoilRecoverySpeed = 1.5f;
    public float MaxSpreadTime = 1f;
    [Range(0, 1)]public float MinSpreadPercent = 0.1f;

    [Space(5)]

    [Tooltip("An AnimationCurve for the size of the dynamic crosshair depending on the current firing time")]
        public AnimationCurve CrosshairCurve;

    [Space(5)]

    [Header("Fire Spread")]
    public float MaxSpreadAngle = 10f;


    /// <summary> Calculate the current spread from the inputted time.</summary>
    public Vector3 GetSpread(float shootTime = 0)
    {
        float spreadRadius = Mathf.Tan((MaxSpreadAngle / 2f) * Mathf.Deg2Rad);
        Vector3 spreadDirection = Vector3.Lerp(
            a: Vector3.zero,
            b: Random.insideUnitCircle * spreadRadius,
            t: Mathf.Clamp(shootTime / MaxSpreadTime, MinSpreadPercent, 1f));

        return spreadDirection;
    }
}
