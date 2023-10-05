using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [SerializeField] private Camera _playerCamera;

    
    [Header("Fire Rate")]
    [SerializeField] private float _fireRate = 5; // How many bullets this weapon can fire in a second.
    private float _fireRateDelayRemaining = 0f;


    [Header("Ammo & Reloading")]
    [SerializeField] private int _maxAmmo = 20; // The maximum number of bullets the player can hold for this gun.
    private int _ammoRemaining;

    [SerializeField] private int _maxClipSize = 8; // The maximum number of bullets in each clip of this gun.
    private int _currentAmmo; // The current number of bullets in this clip.

    [Space(5)]

    [SerializeField] private float _reloadTime;
    private float _reloadTimeRemaining;
    private bool _isReloading = false;


    private void Start()
    {
        _ammoRemaining = _maxAmmo;
        _currentAmmo = _maxClipSize;
    }


    void Update()
    {
        if (_isReloading)
        {
            _reloadTimeRemaining -= Time.deltaTime;
            if (_reloadTimeRemaining < 0)
                FinishedReload();
            else
                return;
        }
        
        if (_fireRateDelayRemaining > 0f)
            _fireRateDelayRemaining -= Time.deltaTime;

        // Note: We don't need to check if we are reloading, but we shall do it anyway to prevent possible errors if we change the reload function.
        if (Input.GetMouseButtonDown(0) && _fireRateDelayRemaining <= 0f && !_isReloading && _currentAmmo > 0)
        {
            Fire();

            _fireRateDelayRemaining = 1f / _fireRate;
            _currentAmmo--;
        }

        if (Input.GetKeyDown(KeyCode.R) && (_currentAmmo != _maxClipSize && _ammoRemaining > 0))
        {
            StartReload();
        }
    }

    private void Fire()
    {
        // Shooting.
        Ray ray = _playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        if (Physics.Raycast(ray, out RaycastHit hit))
            Debug.Log("I'm looking at " + hit.transform.name);
        else
            Debug.Log("I'm looking at nothing!");
    }


    private void StartReload()
    {
        _ammoRemaining += _currentAmmo;
        _currentAmmo = 0;
        
        _reloadTimeRemaining = _reloadTime;
        _isReloading = true;
    }

    private void FinishedReload()
    {
        _isReloading = false;
        
        if (_ammoRemaining > _maxClipSize)
        {
            _ammoRemaining -= _maxClipSize;
            _currentAmmo = _maxClipSize;
        } else {
            _currentAmmo = _ammoRemaining;
            _ammoRemaining = 0;
        }
    }
}
