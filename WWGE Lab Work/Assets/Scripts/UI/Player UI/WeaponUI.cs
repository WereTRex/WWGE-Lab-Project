using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WeaponUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _ammoText;

    [SerializeField] private TMP_Text _weaponNameText;

    [SerializeField] private TMP_Text _currentFiringTypeText;


    private void OnEnable()
    {
        WeaponManager.OnPlayerStartedReloading += OnWeaponReloading;
        WeaponManager.OnPlayerAmmoChanged += OnAmmoValuesChanged;

        WeaponManager.OnWeaponChanged += WeaponChanged;

        WeaponManager.OnFiringTypeChanged += FiringTypeChanged;
    }
    private void OnDisable()
    {
        WeaponManager.OnPlayerStartedReloading -= OnWeaponReloading;
        WeaponManager.OnPlayerAmmoChanged -= OnAmmoValuesChanged;

        WeaponManager.OnWeaponChanged -= WeaponChanged;

        WeaponManager.OnFiringTypeChanged -= FiringTypeChanged;
    }


    private void OnWeaponReloading() => _ammoText.text = "...";
    private void OnAmmoValuesChanged(int newCurrentAmmo, int newAmmoRemaining) => _ammoText.text = newCurrentAmmo + "/" + newAmmoRemaining;
    
    private void WeaponChanged(string newName) => _weaponNameText.text = newName;
    
    private void FiringTypeChanged(FiringType newFiringType) => _currentFiringTypeText.text = newFiringType.ToString();
}
