using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoUI : MonoBehaviour
{
    [SerializeField] private WeaponManager _weaponManager;
    [SerializeField] private TMPro.TMP_Text _ammoText;


    private void OnEnable()
    {
        Gun.OnStartedReloading += OnWeaponReloading;
        Gun.OnWeaponAmmoChanged += OnAmmoValuesChanged;
    }
    private void OnDisable()
    {
        Gun.OnStartedReloading -= OnWeaponReloading;
        Gun.OnWeaponAmmoChanged -= OnAmmoValuesChanged;
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
