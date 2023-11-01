using UnityEngine;

[CreateAssetMenu(fileName = "Shot Config", menuName = "Weapons/Guns/Shot Config", order = 2)]
public class ShootingConfigSO : ScriptableObject
{
    public float FireRate = 0.25f;
    public LayerMask HitMask;
    public float MaxDistance = 100f;

    [Header("Bullet Spread")]
    public bool UseRecoil;

    public float RecoilRecoverySpeed = 1f;
    public float MaxSpreadTime = 1f;
    public Vector2 Spread = new Vector2(0.1f, 0.1f);
}
