using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

[CreateAssetMenu(fileName = "Gun", menuName = "Weapons/Guns/New Gun", order = 0)]
public class GunSO : ScriptableObject
{
    public string Name;
    public GameObject ModelPrefab;
    public Vector3 SpawnPoint;
    public Vector3 SpawnRotation;

    public GameObject[] BulletHolePrefabs;

    public DamageConfigSO DamageConfig;
    public ShootingConfigSO ShootConfig;
    public TrailConfigSO TrailConfig;
}
