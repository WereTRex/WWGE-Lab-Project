using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public static event Action OnStartedReloading;
    public static event Action<int, int> OnWeaponAmmoChanged;
    public static event Action OnHitRigidbodyObject;


    [SerializeField] private float _weaponDamage;

    [Space(5)]

    [SerializeField] private Camera _playerCamera;
    [SerializeField] private Transform _barrelPosition;

    [SerializeField] private float _hitForce;
    [SerializeField] private GameObject[] _bulletHoldPrefabs;


    private bool _isAttacking = false;
    private bool _firing = false;

    #region Fire Rate
    [Header("Fire Rate")]
    [Tooltip("The minimum time between when the player fires the gun and when they can next fire it")]
        [SerializeField] private float _fireDelay = 0.2f;
    private float _fireDelayRemaining = 0f;


    // We could use Enum Flags here instead, but as _currentFireType would then also use Flags, that would make our code more annoying to work with.
    [SerializeField] private FireType[] _availableFireTypes;
    private int _currentFireIndex;
    private int currentFireIndexProperty
    {
        get => _currentFireIndex;
        set
        {
            if (value < 0)
                _currentFireIndex = _availableFireTypes.Length - 1;
            else if (value > _availableFireTypes.Length - 1)
                _currentFireIndex = 0;
            else
                _currentFireIndex = value;
        }
    }
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
    private float _grenadeDelayRemaining;

    [SerializeField] private float _grenadeLaunchForce;
    #endregion



    private void Start()
    {
        _ammoRemaining = _maxAmmo;
        currentAmmoProperty = _maxClipSize;
        currentFireIndexProperty = 0;
    }
    private void OnEnable()
    {
        OnWeaponAmmoChanged?.Invoke(currentAmmoProperty, _maxClipSize);
    }
    private void OnDisable()
    {
        _isReloading = false;
        _isAttacking = false;

        if (_firing)
        {
            _firing = false;
            _fireDelayRemaining = _fireDelay;
        }
    }


    public void SelectNextFiringType()
    {
        currentFireIndexProperty++;
    }


    public void StartAttacking()
    {
        if (_availableFireTypes[currentFireIndexProperty] == FireType.FullAuto)
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

        if (_fireDelayRemaining > 0f && !_firing)
            _fireDelayRemaining -= Time.deltaTime;
        if (_grenadeDelayRemaining > 0f)
            _grenadeDelayRemaining -= Time.deltaTime;

        if (_isAttacking)
            AttemptFire();
    }


    private void AttemptFire()
    {
        if (_fireDelayRemaining > 0f || _isReloading || _firing)
            return;

        if (currentAmmoProperty > 0)
        {
            switch (_availableFireTypes[currentFireIndexProperty])
            {
                case FireType.TwoRoundBurst:
                    StartCoroutine(BurstFire(2));
                    break;
                case FireType.ThreeRoundBurst:
                    StartCoroutine(BurstFire(3));
                    break;
                default:
                    Fire();

                    _fireDelayRemaining = _fireDelay;
                    currentAmmoProperty--;
                    break;
            }
        } else
            StartCoroutine(Reload());
    }
    private IEnumerator BurstFire(int bursts)
    {
        int burstsRemaining = bursts;
        float _timeBetweenBursts = 0.15f;

        _firing = true;
        while (burstsRemaining > 0)
        {
            if (currentAmmoProperty > 0)
            {
                Fire();

                currentAmmoProperty--;
                burstsRemaining--;
                yield return new WaitForSeconds(_timeBetweenBursts);
            } else
                break;
        }

        _fireDelayRemaining = _fireDelay;
        _firing = false;
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
                hit.rigidbody.AddForceAtPosition(_hitForce * Vector3.Normalize(direction), hit.point);

                OnHitRigidbodyObject?.Invoke();

                // Make the bullet a child of the physics object so that it travels with it.
                tempBullet.transform.parent = hit.transform;
            }


            // Deal damage to the object, if it has a health component.
            if (hit.transform.TryGetComponent<HealthComponent>(out HealthComponent healthComponent))
            {
                healthComponent.TakeDamage(_weaponDamage);
            }
        }
    }


    // Currently the same for all guns.
    public void AttemptAlternateFire()
    {
        if (_grenadeDelayRemaining > 0f || _isReloading || _firing)
            return;

        AltFire();
        _grenadeDelayRemaining = _grenadeFireDelay;
    }
    private void AltFire()
    {
        Debug.Log("Alt Fire");
        
        GameObject grenadeInstance = Instantiate(_grenadePrefab, _barrelPosition.position, _barrelPosition.rotation);

        if (grenadeInstance.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.AddForce(_barrelPosition.forward * _grenadeLaunchForce, ForceMode.Impulse);
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