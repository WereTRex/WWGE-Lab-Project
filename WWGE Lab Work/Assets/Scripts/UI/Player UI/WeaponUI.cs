using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary> The Player's Weapon UI</summary>
public class WeaponUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _ammoText;

    [SerializeField] private TMP_Text _weaponNameText;

    [SerializeField] private TMP_Text _currentFiringTypeText;


    private void OnEnable()
    {
        // Subscribe to WeaponManager events.
        WeaponManager.OnPlayerStartedReloading += OnWeaponReloading;
        WeaponManager.OnPlayerAmmoChanged += OnAmmoValuesChanged;

        WeaponManager.OnWeaponChanged += WeaponChanged;

        WeaponManager.OnFiringTypeChanged += FiringTypeChanged;
    }
    private void OnDisable()
    {
        // Unsubscribe to WeaponManager events.
        WeaponManager.OnPlayerStartedReloading -= OnWeaponReloading;
        WeaponManager.OnPlayerAmmoChanged -= OnAmmoValuesChanged;

        WeaponManager.OnWeaponChanged -= WeaponChanged;

        WeaponManager.OnFiringTypeChanged -= FiringTypeChanged;
    }


    private void OnWeaponReloading() => _ammoText.text = "..."; // Display the default reloading text.
    private void OnAmmoValuesChanged(int newCurrentAmmo, int newAmmoRemaining) => _ammoText.text = newCurrentAmmo + "/" + newAmmoRemaining; // Display the current ammo in the form "CurrentAmmo/MaxClipAmmo".
    
    private void WeaponChanged(string newName) => _weaponNameText.text = newName; // Display the weapon's name.
    
    private void FiringTypeChanged(FiringType newFiringType) => _currentFiringTypeText.text = newFiringType.ToString(); // Display the Firing Type.
}
