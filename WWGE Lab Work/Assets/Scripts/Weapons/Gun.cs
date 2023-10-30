using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class Gun : MonoBehaviour
{
    #region - Variables -
    #region General
    [Header("General Variables")]
    [SerializeField] private float _weaponDamage;
    [SerializeField] private float _weaponHitForce;

    private bool _isAttacking = false;
    #endregion

    #region Firing Variables
    [Header("Firing Variables")]
    [SerializeField] private Transform _raycastOriginPoint;
    [SerializeField] private Transform _bulletOriginPoint;
    [SerializeField] private LayerMask _shootingMask;
    [SerializeField] private float _maxShootingDistance = float.PositiveInfinity;

    #region Fire Rate
    [Header("Fire Rate")]
    [SerializeField] private float _fireDelay;
    private float _lastShotTime;
    #endregion

    #region Weapon Spread
    [Header("Weapon Spread")]
    [SerializeField] private bool _useBulletSpread = true;
    [SerializeField] private Vector2 _bulletSpreadVariance = new Vector2(0.1f, 0.1f);
    #endregion

    #region Firing Type
    [System.Serializable]
    enum FiringType
    {
        SingleFire = 0,
        FullAuto = 1
    }

    [Header("Firing Types")]
    [SerializeField] private FiringType[] _availableFiringTypes;
    private int _currentFireIndex = 0;
    #endregion
    #endregion

    #region Effects
    [Header("Weapon VFX")]
    [SerializeField] private ParticleSystem _muzzleFlashParticleSystem;

    [SerializeField] private TrailConfigScriptableObject _bulletTrailConfig;
    private ObjectPool<TrailRenderer> _trailPool;

    [SerializeField] private ParticleSystem _impactParticleSystem;
    [SerializeField] private GameObject[] _bulletHolePrefabs;
    #endregion

    #region Ammunition and Reloading
    [Header("Ammunition and Reloading")]
    [SerializeField] private int _maxAmmo;
    private int _ammoRemaining;
    [SerializeField] private int _clipAmmo;
    private int _currentAmmo;

    [SerializeField] private float _reloadTime = 0.5f;
    [SerializeField] private bool _autoReloadWhenFiring = true;

    private bool _isReloading;
    #endregion

    #region Alternate Fire
    [Header("Alternate Fire")]
    [SerializeField] private float _placeholder;
    #endregion
    #endregion


    private void Awake()
    {
        _lastShotTime = 0f;
        _trailPool = new ObjectPool<TrailRenderer>(CreateTrail);

        _ammoRemaining = _maxAmmo;
        _currentAmmo = _clipAmmo;
    }

    private void OnDisable()
    {
        _isReloading = false;
        _isAttacking = false;
    }

    private void Update()
    {
        if (_availableFiringTypes[_currentFireIndex] == FiringType.FullAuto && _isAttacking && !_isReloading)
            AttemptAttack();
    }


    #region - Input -
    public void StartAttacking()
    {
        _isAttacking = true;
        if (_availableFiringTypes[_currentFireIndex] == FiringType.FullAuto)
            return;
        
        AttemptAttack();
    }
    public void StopAttacking()
    {
        _isAttacking = false;
    }
    #endregion

    #region - Attacking -
    /// <summary>
    /// Attempt to make an attack with this weapon.
    /// Handles things like attack speed, ammunition/durability, etc
    /// </summary>
    public void AttemptAttack()
    {
        // Check whether we can fire (Fire Rate, Ammunition, etc).
        if (Time.time < _fireDelay + _lastShotTime || _isReloading)
            return;


        // Force a reload if out of ammo in clip.
        if (_currentAmmo <= 0)
        {
            if (_ammoRemaining > 0 && _autoReloadWhenFiring)
                StartReload();
            else {
                Debug.Log("Click");
                _lastShotTime = Time.time;
            }

            return;
        }


        // Fire the weapon.
        MakeAttack();
    }
    private void MakeAttack()
    {
        _lastShotTime = Time.time;
        _currentAmmo--;

        if (_muzzleFlashParticleSystem != null)
            _muzzleFlashParticleSystem.Play();

        Vector3 fireDirection = GetShotDirection();

        // Get what we are shooting at.
        if (Physics.Raycast(_raycastOriginPoint.position, fireDirection, out RaycastHit hit, _maxShootingDistance, _shootingMask))
        {
            // (Effect) Bullet Trails.
            StartCoroutine(PlayTrail(_bulletOriginPoint.position, hit.point, hit));

            // (Effect) Ready Hit Effects.
            GameObject bulletHole = null;
            if (_bulletHolePrefabs.Length > 0)
                 bulletHole = Instantiate(_bulletHolePrefabs[Random.Range(0, _bulletHolePrefabs.Length)], hit.point, Quaternion.LookRotation(hit.normal));


            // (Logic) Deal damage to the hit object, if applicable.
            if (hit.transform.TryGetComponent<HealthComponent>(out HealthComponent healthComponent))
                healthComponent.TakeDamage(_weaponDamage);

            // (Logic) Apply a force to the hit object, if applicable.
            if (hit.transform.TryGetComponent<Rigidbody>(out Rigidbody rigidbody))
            {
                Vector3 direction = new Vector3(hit.transform.position.x - transform.position.x,
                    hit.transform.position.y - transform.position.y,
                    hit.transform.position.z - transform.position.z).normalized;

                rigidbody.AddForceAtPosition(direction * _weaponHitForce, hit.point);

                // (Effect) Make the bullet hole a child of the physics object, so that it travels with it.
                if (bulletHole != null)
                    bulletHole.transform.parent = hit.transform;
            }
        }
        // We missed.
        else {
            // (Effect) Bullet Trails.
            StartCoroutine(PlayTrail(_bulletOriginPoint.position, _raycastOriginPoint.position + (fireDirection * _bulletTrailConfig.MissDistance), new RaycastHit()));
        }
    }


    private Vector3 GetShotDirection()
    {
        Vector3 direction = _raycastOriginPoint.forward;

        if (_useBulletSpread)
        {
            direction += new Vector3(Random.Range(-_bulletSpreadVariance.x, _bulletSpreadVariance.x),
                Random.Range(-_bulletSpreadVariance.y, _bulletSpreadVariance.y));

            direction.Normalize();
        }

        return direction;
    }
    private TrailRenderer CreateTrail()
    {
        GameObject instance = new GameObject("Bullet Trail");
        TrailRenderer trail = instance.AddComponent<TrailRenderer>();
        trail.colorGradient = _bulletTrailConfig.Color;
        trail.material = _bulletTrailConfig.Material;
        trail.widthCurve = _bulletTrailConfig.WidthCurve;
        trail.time = _bulletTrailConfig.Duration;
        trail.minVertexDistance = _bulletTrailConfig.MinVertexDistance;

        trail.emitting = false;
        trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        return trail;
    }
    private IEnumerator PlayTrail(Vector3 startPoint, Vector3 endPoint, RaycastHit hit)
    {
        TrailRenderer instance = _trailPool.Get();
        instance.gameObject.SetActive(true);
        instance.transform.position = startPoint;
        yield return null;

        instance.emitting = true;

        float distance = Vector3.Distance(startPoint, endPoint);
        float remainingDistance = distance;
        while(remainingDistance > 0)
        {
            instance.transform.position = Vector3.Lerp(startPoint, endPoint, Mathf.Clamp01(1 - (remainingDistance / distance)));
            remainingDistance -= _bulletTrailConfig.SimulationSpeed * Time.deltaTime;

            yield return null;
        }

        instance.transform.position = endPoint;

        yield return new WaitForSeconds(_bulletTrailConfig.Duration);
        yield return null;

        instance.emitting = false;
        instance.gameObject.SetActive(false);
        _trailPool.Release(instance);
    }
    #endregion

    #region - Reloading -
    public void StartReload()
    {
        if (_ammoRemaining <= 0 || _isReloading)
            return;

        StartCoroutine(Reload());
    }

    private IEnumerator Reload()
    {
        _ammoRemaining += _currentAmmo;
        _currentAmmo = 0;

        _isReloading = true;
        yield return new WaitForSeconds(_reloadTime);
        _isReloading = false;
        
        if (_ammoRemaining < _clipAmmo)
        {
            _currentAmmo = _ammoRemaining;
            _ammoRemaining = 0;
        } else {
            _currentAmmo = _clipAmmo;
            _ammoRemaining -= _clipAmmo;
        }
    }
    #endregion

    #region Firing Types
    public void SwitchFiringType()
    {
        if (_currentFireIndex < _availableFiringTypes.Length - 1)
            _currentFireIndex++;
        else
            _currentFireIndex = 0;
    }
    #endregion


    #region - Gizmos -
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        // Vertical.
        Gizmos.DrawRay(_raycastOriginPoint.position, (_raycastOriginPoint.forward + new Vector3(0, _bulletSpreadVariance.y, 0)).normalized * _maxShootingDistance);
        Gizmos.DrawRay(_raycastOriginPoint.position, (_raycastOriginPoint.forward + new Vector3(0, -_bulletSpreadVariance.y, 0)).normalized * _maxShootingDistance);

        // Horizontal.
        Gizmos.DrawRay(_raycastOriginPoint.position, (_raycastOriginPoint.forward + new Vector3(_bulletSpreadVariance.x, 0, 0)).normalized * _maxShootingDistance);
        Gizmos.DrawRay(_raycastOriginPoint.position, (_raycastOriginPoint.forward + new Vector3(-_bulletSpreadVariance.x, 0, 0)).normalized * _maxShootingDistance);
    }
    #endregion
}



/*public class Gun : MonoBehaviour
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
*/

/*
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
}*/