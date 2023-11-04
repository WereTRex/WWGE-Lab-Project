using UnityEngine;

[CreateAssetMenu(fileName = "Ammo Config", menuName = "Weapons/Guns/Ammo Config", order = 3)]
public class AmmoConfigSO : ScriptableObject
{
    public int MaxAmmo;
    public int MaxClipAmmo;

    [Header("Reloading")]

    public float ReloadTime = 0.7f;
    public bool AutoReloadWhenAttacking = true;
    public bool IndividualReloading = false;
}
