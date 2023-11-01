using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponManager : MonoBehaviour
{
    private List<Gun> _playerWeapons = new List<Gun>();
    private int _selectedWeaponIndex;
    private int selectedWeaponIndexProperty
    {
        get => _selectedWeaponIndex;
        set
        {
            if (value < 0)
                _selectedWeaponIndex = _playerWeapons.Count - 1;
            else if (value > _playerWeapons.Count - 1)
                _selectedWeaponIndex = 0;
            else
                _selectedWeaponIndex = value;

            ActivateSelectedWeapon();
        }
    }


    #region New Input System
    public void OnFire(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            _playerWeapons[_selectedWeaponIndex].StartAttacking();
        } else if (context.canceled) {
            _playerWeapons[_selectedWeaponIndex].StopAttacking();
        }
    }
    public void OnAlternateFire(InputAction.CallbackContext context)
    {
        if (context.performed)
            _playerWeapons[_selectedWeaponIndex].AttemptAlternateFire();
        
    }
    public void OnReload(InputAction.CallbackContext context)
    {
        if (context.performed)
            _playerWeapons[_selectedWeaponIndex].StartReload();
    }
    public void OnSwapWeapon(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (context.ReadValue<Vector2>().y > 0)
                selectedWeaponIndexProperty++;
            else
                selectedWeaponIndexProperty--;
        }
    }
    public void OnSwapFiringMode(InputAction.CallbackContext context)
    {
        if (context.performed)
            _playerWeapons[_selectedWeaponIndex].SwitchFiringType();
    }
    #endregion


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
    }


    void ActivateSelectedWeapon()
    {
        for (int i = 0; i < _playerWeapons.Count; i++)
        {
            _playerWeapons[i].gameObject.SetActive(i == selectedWeaponIndexProperty);
        }
    }
}
