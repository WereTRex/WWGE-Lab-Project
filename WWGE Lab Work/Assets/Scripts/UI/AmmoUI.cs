using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AmmoUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _ammoText;


    private void OnEnable()
    {
        WeaponManager.OnPlayerStartedReloading += OnWeaponReloading;
        WeaponManager.OnPlayerAmmoChanged += OnAmmoValuesChanged;
    }
    private void OnDisable()
    {
        WeaponManager.OnPlayerStartedReloading -= OnWeaponReloading;
        WeaponManager.OnPlayerAmmoChanged -= OnAmmoValuesChanged;
    }


    private void OnWeaponReloading()
    {
        _ammoText.text = "...";
    }
    private void OnAmmoValuesChanged(int newCurrentAmmo, int newAmmoRemaining)
    {
        _ammoText.text = newCurrentAmmo + "/" + newAmmoRemaining;
    }
}
