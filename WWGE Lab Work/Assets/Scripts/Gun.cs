using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [SerializeField] private Camera _playerCamera;

    
    [Header("Fire Rate")]
    [Tooltip("The minimum time between when the player fires the gun and when they can next fire it")]
        [SerializeField] private float _fireDelay = 0.2f;
    private float _fireRateDelayRemaining = 0f;

    [Tooltip("If true, the player can just hold down the fire button and will continuously fire")]
        [SerializeField] private bool _fullAuto = false;


    [Header("Ammo & Reloading")]
    [Tooltip("The maximum number of bullets the player can hold for this gun")]
        [SerializeField] private int _maxAmmo = 20;
    private int _ammoRemaining;

    [Tooltip("The maximum number of bullets in each magazine")]
        [SerializeField] private int _maxClipSize = 8;
    private int _currentAmmo; // The current number of bullets in this clip.

    [Space(5)]

    [SerializeField] private float _reloadTime;
    private float _reloadTimeRemaining;
    private bool _isReloading = false;


    #region Public Accessors
    public int MaxClipSizeProperty { get => _maxClipSize; }
    public int CurrentAmmoProperty { get => _currentAmmo; }
    public bool GetIsReloading() => _isReloading;
    #endregion


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
        if (((Input.GetMouseButton(0) && _fullAuto) || Input.GetMouseButtonDown(0)) && _fireRateDelayRemaining <= 0f && !_isReloading && _currentAmmo > 0)
        {
            Fire();

            _fireRateDelayRemaining = _fireDelay;
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
