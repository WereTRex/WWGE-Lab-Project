using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponManager : MonoBehaviour
{
    public static event Action OnPlayerStartedReloading;
    public static event Action OnPlayerHitPhysicsObject;
    public static event Gun.WeaponAmmoChanged OnPlayerAmmoChanged;
    
    public static event Action<string> OnWeaponChanged;
    public static event Action<FiringType> OnFiringTypeChanged;
    
    
    private List<Gun> _playerWeapons = new List<Gun>();
    private int _selectedWeaponIndex;
    private int selectedWeaponIndexProperty
    {
        get => _selectedWeaponIndex;
        set
        {
            // Ensure that the index doesn't go past 0 or the weapon count by having it loop.
            if (value < 0)
                _selectedWeaponIndex = _playerWeapons.Count - 1;
            else if (value > _playerWeapons.Count - 1)
                _selectedWeaponIndex = 0;
            else
                _selectedWeaponIndex = value;

            // Enable the gameobject of the selected index.
            ActivateSelectedWeapon();

            // Call relevant events.
            OnWeaponChanged?.Invoke(_playerWeapons[_selectedWeaponIndex].name);
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


    private void Awake()
    {
        foreach (Transform weapon in transform)
        {
            if (weapon.TryGetComponent<Gun>(out Gun gunScript))
                _playerWeapons.Add(gunScript);
        }

        selectedWeaponIndexProperty = 0;
    }

    private void OnEnable()
    {
        // Subscribe to all weapon events.
        for (int i = 0; i < _playerWeapons.Count; i++)
        {
            _playerWeapons[i].OnStartedReloading += OnPlayerStartedReloading;
            _playerWeapons[i].OnWeaponAmmoChanged += OnPlayerAmmoChanged;
            _playerWeapons[i].OnHitPhysicsObject += OnPlayerHitPhysicsObject;

            _playerWeapons[i].OnFiringTypeChanged += OnFiringTypeChanged;
        }

        // Ensure the current selected index doesn't exceed the number of weapons.
        selectedWeaponIndexProperty = Mathf.Clamp(selectedWeaponIndexProperty, 0, _playerWeapons.Count - 1);
    }
    private void OnDisable()
    {
        // Unsubscribe from all weapon events.
        for (int i = 0; i < _playerWeapons.Count; i++)
        {
            _playerWeapons[i].OnStartedReloading -= OnPlayerStartedReloading;
            _playerWeapons[i].OnWeaponAmmoChanged -= OnPlayerAmmoChanged;
            _playerWeapons[i].OnHitPhysicsObject -= OnPlayerHitPhysicsObject;

            _playerWeapons[i].OnFiringTypeChanged -= OnFiringTypeChanged;
        }
    }


    void ActivateSelectedWeapon()
    {
        // Enable the selected weapon, disable all others.
        for (int i = 0; i < _playerWeapons.Count; i++)
        {
            _playerWeapons[i].gameObject.SetActive(i == selectedWeaponIndexProperty);
        }
    }


    public void AddWeaponFromPrefab(Gun newWeaponPrefab)
    {
        // Instantiate the prefab.
        Gun instance = Instantiate(newWeaponPrefab, transform);

        // Subscribe to events
        instance.OnStartedReloading += OnPlayerStartedReloading;
        instance.OnWeaponAmmoChanged += OnPlayerAmmoChanged;
        instance.OnHitPhysicsObject += OnPlayerHitPhysicsObject;

        // Add the new weapon to the weapon list.
        _playerWeapons.Add(instance);
    }
    public void AddWeaponFromInstance(Gun instance)
    {
        // Set the parent & reset the transform.
        instance.transform.SetParent(transform, false);

        // Subscribe to events.
        instance.OnStartedReloading += OnPlayerStartedReloading;
        instance.OnWeaponAmmoChanged += OnPlayerAmmoChanged;
        instance.OnHitPhysicsObject += OnPlayerHitPhysicsObject;
        
        // Add the new weapon to the weapon list.
        _playerWeapons.Add(instance);
    }


    public float GetCrosshairSize()
    {
        return _playerWeapons[selectedWeaponIndexProperty].GetCrosshairSize();
    }
}
