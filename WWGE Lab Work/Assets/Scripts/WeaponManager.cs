using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
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
        if (Time.timeScale == 0f)
            return;
        
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
            _playerWeapons[i].gameObject.SetActive(i == selectedWeaponIndexProperty);
        }
    }


    // REPLACE THESE WITH A BETTER METHOD.
    #region Replace ASAP with a better method
    public int GetCurrentAmmo() => transform.GetChild(_selectedWeaponIndex).GetComponent<Gun>().CurrentAmmoProperty;
    public int GetMaxClipAmmo() => transform.GetChild(_selectedWeaponIndex).GetComponent<Gun>().MaxClipSizeProperty;
    public bool GetIsReloading() => transform.GetChild(_selectedWeaponIndex).GetComponent<Gun>().GetIsReloading();
    #endregion
}
