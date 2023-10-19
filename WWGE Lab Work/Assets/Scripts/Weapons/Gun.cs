using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public static event Action OnStartedReloading;
    public static event Action<int, int> OnWeaponAmmoChanged;
    public static event Action OnHitRigidbodyObject;


    [SerializeField] private Camera _playerCamera;
    [SerializeField] private float _force;
    [SerializeField] private GameObject[] _bulletHoldPrefabs;


    private bool _isAttacking = false;

    #region Fire Rate
    [Header("Fire Rate")]
    [Tooltip("The minimum time between when the player fires the gun and when they can next fire it")]
        [SerializeField] private float _fireDelay = 0.2f;
    private float _fireDelayRemaining = 0f;

    [SerializeField] private FireType _fireType;
    #endregion

    #region Ammo and Reloading
    [Header("Ammo & Reloading")]
    [Tooltip("The maximum number of bullets the player can hold for this gun")]
        [SerializeField] private int _maxAmmo = 20;
    private int _ammoRemaining;

    [Tooltip("The maximum number of bullets in each magazine")]
        [SerializeField] private int _maxClipSize = 8;
    private int _currentAmmo; // The current number of bullets in this clip.
    private int currentAmmoProperty
    {
        get => _currentAmmo;
        set
        {
            _currentAmmo = value;
            OnWeaponAmmoChanged?.Invoke(currentAmmoProperty, _maxClipSize);
        }
    }

    [Space(5)]

    [SerializeField] private float _reloadTime;
    private bool _isReloading = false;
    #endregion

    #region Alternate Fire
    [Header("Alt Fire")]
    [SerializeField] private GameObject _grenadePrefab;
    [SerializeField] private float _grenadeFireDelay;
    [SerializeField] private float _grenadeLaunchForce;
    #endregion



    #region Public Accessors
    public int MaxClipSizeProperty { get => _maxClipSize; }
    public int CurrentAmmoProperty { get => currentAmmoProperty; }
    public bool GetIsReloading() => _isReloading;
    #endregion


    private void Start()
    {
        _ammoRemaining = _maxAmmo;
        currentAmmoProperty = _maxClipSize;
    }
    private void OnEnable()
    {
        OnWeaponAmmoChanged?.Invoke(currentAmmoProperty, _maxClipSize);
    }
    private void OnDisable()
    {
        _isReloading = false;
        _isAttacking = false;
    }



    public void StartAttacking()
    {
        if (_fireType == FireType.FullAuto)
            _isAttacking = true;
        else {
            AttemptFire();
        }
    }
    public void StopAttacking()
    {
        _isAttacking = false;
    }

    public void StartReloading()
    {
        if (currentAmmoProperty != _maxClipSize && _ammoRemaining > 0)
            StartCoroutine(Reload());
    }



    void Update()
    {
        if (Time.timeScale == 0f)
            return;

        if (_fireDelayRemaining > 0f)
            _fireDelayRemaining -= Time.deltaTime;

        if (_isAttacking)
            AttemptFire();
    }


    private void AttemptFire()
    {
        if (_fireDelayRemaining > 0f || _isReloading)
            return;

        if (currentAmmoProperty > 0)
        {
            if (_fireType == FireType.ThreeRoundBurst)
            {
                StartCoroutine(BurstFire());
            } else if (_fireType == FireType.SingleFire) {
                Fire();
                _fireDelayRemaining = _fireDelay;
                currentAmmoProperty--;
            }
        } else
            StartCoroutine(Reload());
    }
    private IEnumerator BurstFire()
    {
        throw new NotImplementedException();
    }
    private void Fire()
    {
        // Shooting.
        Ray ray = _playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // Create a bullet hole.
            GameObject chosenBulletHole = _bulletHoldPrefabs[UnityEngine.Random.Range(0, _bulletHoldPrefabs.Length)];
            var tempBullet = Instantiate(chosenBulletHole, hit.point, Quaternion.LookRotation(hit.normal));


            // Add force to the object if it has a rigidbody.
            if (hit.rigidbody)
            {
                // Get the direction of the force to be applied, and apply it.
                var direction = new Vector3(hit.transform.position.x - transform.position.x,
                    hit.transform.position.y - transform.position.y,
                    hit.transform.position.z - transform.position.z);
                hit.rigidbody.AddForceAtPosition(_force * Vector3.Normalize(direction), hit.point);

                OnHitRigidbodyObject?.Invoke();

                // Make the bullet a child of the physics object so that it travels with it.
                tempBullet.transform.parent = hit.transform;
            }
        }
    }


    // Currently the same for all guns.
    public void AttemptAlternateFire()
    {
        if (_fireDelayRemaining > 0f || _isReloading)
            return;

        AltFire();
        _fireDelayRemaining = _grenadeFireDelay;
    }
    private void AltFire()
    {
        Debug.Log("Alt Fire");
        
        GameObject grenadeInstance = Instantiate(_grenadePrefab, _playerCamera.transform.position, _playerCamera.transform.rotation);

        if (grenadeInstance.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.AddForce(_playerCamera.transform.forward * _grenadeLaunchForce, ForceMode.Impulse);
        }
    }


    private IEnumerator Reload()
    {
        _ammoRemaining += currentAmmoProperty;
        currentAmmoProperty = 0;
        
        _isReloading = true;
        OnStartedReloading?.Invoke();
        yield return new WaitForSeconds(_reloadTime);
        
        _isReloading = false;
        FinishedReload();
    }

    private void FinishedReload()
    {        
        if (_ammoRemaining > _maxClipSize)
        {
            _ammoRemaining -= _maxClipSize;
            currentAmmoProperty = _maxClipSize;
        } else {
            currentAmmoProperty = _ammoRemaining;
            _ammoRemaining = 0;
        }
    }
}


[System.Serializable]
public enum FireType
{
    SingleFire,
    TwoRoundBurst,
    ThreeRoundBurst,
    FullAuto
}