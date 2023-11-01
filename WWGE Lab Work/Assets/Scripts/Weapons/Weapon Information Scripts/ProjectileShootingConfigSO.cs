using UnityEngine;

public class ProjectileShootingConfigSO : ShootingConfigSO
{
    [Header("Projectile Shooting Info")]
    public GameObject BulletPrefab;
    public float BulletSpawnForce = 100f;
}
