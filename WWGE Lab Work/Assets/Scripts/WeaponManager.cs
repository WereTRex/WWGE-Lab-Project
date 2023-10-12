using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    private event Action OnStartedReloading;
    private event Action<int, int> OnWeaponAmmoChanged;
    
    
    private List<Gun> _playerWeapons = new List<Gun>();
    private int _selectedWeaponIndex;
    private int selectedWeaponIndexProperty
    {
        get => _selectedWeaponIndex;
        set
        {
            _selectedWeaponIndex = value;
            ActivateSelectedWeapon();
        }
    }

    private void Start()
    {
        selectedWeaponIndexProperty = 0;

        foreach (Transform weapon in transform)
        {
            if (weapon.TryGetComponent<Gun>(out Gun gunScript))
                _playerWeapons.Add(gunScript);
        }
    }

    private void Update()
    {
        // Weapon Swapping.
        if (Input.GetAxis("Mouse ScrollWheel") > 0f || Input.GetKeyDown(KeyCode.E))
        {
            if (selectedWeaponIndexProperty >= transform.childCount - 1)
                selectedWeaponIndexProperty = 0;
            else
                selectedWeaponIndexProperty++;
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0f || Input.GetKeyDown(KeyCode.Q))
        {
            if (selectedWeaponIndexProperty <= 0)
                selectedWeaponIndexProperty = transform.childCount - 1;
            else
                selectedWeaponIndexProperty--;
        }
    }


    void ActivateSelectedWeapon()
    {
        for (int i = 0; i < _playerWeapons.Count; i++)
        {
            _playerWeapons[i].gameObject.SetActive(i == _selectedWeaponIndex);
        }
    }


    #region (Temp) Adding and Removing Weapons
    public void AddNewWeapon(Gun newWeapon)
    {
        newWeapon.transform.SetParent(transform, false);
        _playerWeapons.Add(newWeapon);
    }
    public void AddNewWeapon(GameObject newWeapon)
    {
        Gun weaponInstance = Instantiate(newWeapon, transform).GetComponent<Gun>();
        _playerWeapons.Add(weaponInstance);
    }
    public void RemoveWeapon(int weaponIndex)
    {
        Destroy(_playerWeapons[weaponIndex].gameObject);
        _playerWeapons.RemoveAt(weaponIndex);

        selectedWeaponIndexProperty = Mathf.Clamp(selectedWeaponIndexProperty, 0, _playerWeapons.Count - 1);
        ActivateSelectedWeapon();
    }
    #endregion


    // REPLACE THESE WITH A BETTER METHOD.
    #region Replace ASAP with a better method
    public int GetCurrentAmmo() => transform.GetChild(_selectedWeaponIndex).GetComponent<Gun>().CurrentAmmoProperty;
    public int GetMaxClipAmmo() => transform.GetChild(_selectedWeaponIndex).GetComponent<Gun>().MaxClipSizeProperty;
    public bool GetIsReloading() => transform.GetChild(_selectedWeaponIndex).GetComponent<Gun>().GetIsReloading();
    #endregion
}
