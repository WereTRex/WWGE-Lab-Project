using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

/// <summary> A class that processes information from ConfigSOs to function as a Gun.</summary>
public class Gun : MonoBehaviour
{
    #region Variables
    [SerializeField] private DamageConfigSO _damageConfig;
    private bool _isAttacking = false;

    [SerializeField] private GameObject _model;
    [SerializeField] private ParticleSystem _muzzleFlashPS;


    [Header("Firing")]
    [SerializeField] private List<ShootingConfigSO> _shootConfigs;
    private int _currentFiringIndex = 0;

    [SerializeField] private Transform _raycastOrigin;
    [SerializeField] private Transform _bulletOrigin;
    private float _lastShotTime = 0f;
    private float _spreadTime = 0f;

    private Dictionary<ShootingConfigSO, ObjectPool<Bullet>> _bulletPools;


    [Header("Ammo and Reloading")]
    [SerializeField] private AmmoConfigSO _ammoConfig;
    [ReadOnly] private int _totalAmmoRemaining;
    [ReadOnly] private int _clipAmmoRemaining;
    private int _clipAmmoRemainingProperty
    {
        get => _clipAmmoRemaining;
        set
        {
            _clipAmmoRemaining = value;
            OnWeaponAmmoChanged?.Invoke(_clipAmmoRemaining, _ammoConfig.MaxClipAmmo);
        }
    }

    [ReadOnly] private bool _isReloading = false;


    [Header("Alternate Fire")]
    [SerializeField] private AlternateFireSO _alternateFireSO;
    private float _lastAlternateFireTime;


    #region Effects
    [Header("Effects")]
    [SerializeField] private TrailConfigSO _trailConfig;
    [SerializeField] private BulletTrailManager _bulletTrailManager;

    [Space(5)]

    [SerializeField] private AudioConfigSO _audioConfig;
    [SerializeField] private AudioSource _audioSource;

    [Space(5)]

    [SerializeField] private GameObject[] _bulletHolePrefabs;
    #endregion


    [Header("Gizmos")]
    [SerializeField] private bool _drawGizmos = false;
    #endregion

    public event Action OnStartedReloading;
    public event Action OnHitPhysicsObject;

    public delegate void WeaponAmmoChanged(int currentClipAmmo, int maxClipAmmo);
    public event WeaponAmmoChanged OnWeaponAmmoChanged;

    public Action<FiringType> OnFiringTypeChanged;


    [ReadOnly] public Vector3 TopPos;
    [SerializeField] public RectTransform TestTransform;


    #region Awake
    private void Awake()
    {
        // Create bullet pools for if this weapon uses projectiles in any of its firing modes.
        _bulletPools = new Dictionary<ShootingConfigSO, ObjectPool<Bullet>>();
        for (int i = 0; i < _shootConfigs.Count; i++)
        {
            if (!_shootConfigs[i].IsHitscan)
                _bulletPools.Add(_shootConfigs[i], new ObjectPool<Bullet>(CreateBullet));
        }
    }
    #endregion

    #region Enable and Disable
    private void OnEnable()
    {
        // Prevent errors when changing weapons, such as being stuck reloading or the UI not updating correctly.
        OnWeaponAmmoChanged?.Invoke(_clipAmmoRemaining, _ammoConfig.MaxClipAmmo);
        OnFiringTypeChanged?.Invoke(_shootConfigs[_currentFiringIndex].FiringType);
        _isReloading = false;
    }
    #endregion

    #region Start
    private void Start()
    {
        // Set the Ammo Remaining values.
        _totalAmmoRemaining = _ammoConfig.MaxAmmo;
        _clipAmmoRemainingProperty = _ammoConfig.MaxClipAmmo;

        // Initial calling of these events in the case that the WeaponManager subscribes in OnEnable after the invoke has already been made.
        OnWeaponAmmoChanged?.Invoke(_clipAmmoRemaining, _ammoConfig.MaxClipAmmo);
        OnFiringTypeChanged?.Invoke(_shootConfigs[_currentFiringIndex].FiringType);
    }
    #endregion

    #region Update
    private void Update()
    {
        // Adjust the spread time based on whether the player is currently shooting or not.
        _spreadTime = Mathf.Clamp(_spreadTime + Time.deltaTime * (_isAttacking && _clipAmmoRemaining > 0 ? 1f : -_shootConfigs[_currentFiringIndex].RecoilRecoverySpeed), 0, _shootConfigs[_currentFiringIndex].MaxSpreadTime);

        // If we are using a model-based application of recoil, slowly move the model back to its original rotation (x = 0, y = 0, z = 0).
        if (_model != null)
            _model.transform.localRotation = Quaternion.Lerp(_model.transform.localRotation, Quaternion.Euler(0f, 0f, 0f), Time.deltaTime * _shootConfigs[_currentFiringIndex].RecoilRecoverySpeed);


        // If we are attacking, the current firing type is FullAuto, we are not currently reloading, then attempt to attack.
        if (_isAttacking && _shootConfigs[_currentFiringIndex].FiringType == FiringType.FullAuto && !_isReloading)
            AttemptAttack();
    }
    #endregion

    #region Input
    public void StartAttacking()
    {
        // If the firing type is FullAuto, set _isAttacking to true.
        if (_shootConfigs[_currentFiringIndex].FiringType == FiringType.FullAuto)
        {
            _isAttacking = true;
        }
        // Otherise attempt a single attack.
        else
        {
            AttemptAttack();
        }
    }
    public void StopAttacking() => _isAttacking = false; // Stop Attacking.


    public void SwitchFiringType()
    {
        // Ensure the firing index doesn't go out of range by looping it.
        if (_currentFiringIndex < _shootConfigs.Count - 1)
            _currentFiringIndex++;
        else
            _currentFiringIndex = 0;

        // Invoke the changed event.
        OnFiringTypeChanged?.Invoke(_shootConfigs[_currentFiringIndex].FiringType);
    }
    #endregion

    #region Shooting
    // Attempt to fire the gun, checking for if we can currently fire before calling the appropriate method.
    void AttemptAttack()
    {
        // Check fireRate & if we are reloading.
        if (Time.time <= _shootConfigs[_currentFiringIndex].FireDelay + _lastShotTime || _isReloading)
            return;

        // Check that we have ammo remaining.
        if (_clipAmmoRemainingProperty <= 0 && _clipAmmoRemaining != -1)
        {
            if (_ammoConfig.AutoReloadWhenAttacking && (_totalAmmoRemaining > 0 || _ammoConfig.MaxAmmo == -1))
                StartReload();
            else
            {
                _audioConfig?.PlayOutOfAmmoClip(_audioSource);
                _lastShotTime = Time.time;
            }

            return;
        }

        // We can attack, so we do so (With some logic for single & burst firing modes).
        switch (_shootConfigs[_currentFiringIndex].FiringType)
        {
            case FiringType.SingleFire:
                _spreadTime = Mathf.Clamp(_spreadTime + _shootConfigs[_currentFiringIndex].MaxSpreadTime / 2f, 0, _shootConfigs[_currentFiringIndex].MaxSpreadTime);
                MakeAttack();
                break;
            case FiringType.FullAuto:
                MakeAttack();
                break;
            case FiringType.ThreeRoundBurst:
                StartCoroutine(BurstAttack(3));
                break;
        }
    }

    // Trigger the MakeAttack script multiple times with a delay, simulating a burst fire mode. 
    private IEnumerator BurstAttack(int burstCount)
    {
        // Store certain values in variables so we only need to access them once.
        float burstDelay = _shootConfigs[_currentFiringIndex].FireDelay;
        float maxSpreadTime = _shootConfigs[_currentFiringIndex].MaxSpreadTime;

        int burstsRemaining = burstCount;
        while (burstsRemaining > 0 && _clipAmmoRemainingProperty > 0)
        {
            // If we have ammo, fire.
            if (_clipAmmoRemainingProperty > 0)
            {
                // Increment the spread.
                _spreadTime = Mathf.Clamp(_spreadTime + maxSpreadTime / 2f, 0, maxSpreadTime);

                // Fire the bullet.
                MakeAttack();
            }
            else
            {
                // Otherwise, play out of ammo sound (Allows for further clicks).
                _audioConfig?.PlayOutOfAmmoClip(_audioSource);
                _lastShotTime = Time.time;
            }

            // Wait burstDelay seconds and decrement the burstsRemaining.
            yield return new WaitForSeconds(burstDelay);
            burstsRemaining--;
        }
    }


    // Make an attack with the weapon, passing off required values to the appropriate methods depending on whether we are using Hitscan or Projectile shooting.
    private void MakeAttack()
    {
        // Fire Rate.
        _lastShotTime = Time.time;

        // Reduce Ammo if not -1 (-1 is what we use for infinite clips).
        if (_clipAmmoRemaining != -1)
            _clipAmmoRemainingProperty--;


        // Muzzle Flash & Audio.
        if (_muzzleFlashPS != null)
            _muzzleFlashPS.Play();
        _audioConfig?.PlayerShootingClip(_audioSource, _clipAmmoRemaining == 1);


        // Calculate Fire Direction.
        Vector3 spreadAmount = _shootConfigs[_currentFiringIndex].GetSpread(_spreadTime);
        Vector3 fireDirection = (_raycastOrigin.forward + _raycastOrigin.rotation * new Vector3(spreadAmount.x, spreadAmount.y)).normalized;

        // (Effect) Recoil.
        ApplyRecoil();


        // Fire the bullets.
        for (int i = 0; i < _shootConfigs[_currentFiringIndex].BulletsPerShot; i++)
        {
            // Bullet Deviation for things like Shotguns.
            Vector3 direction = GetShotDirection(fireDirection);

            // Call the required method depending on if we are using Hitscan or Projectile firing.
            if (_shootConfigs[_currentFiringIndex].IsHitscan)
                HitscanShoot(direction);
            else
                ProjectileShoot(direction);
        }
    }



    // Calculate the direction that we are to shoot in (With calculations for spread for things like Shotguns).
    private Vector3 GetShotDirection(Vector3 fireDirection)
    {
        // Calculate the size of the circle at the end of our cone, which has an angle in degrees of MaxBulletAngle.
        float radius = Mathf.Tan((_shootConfigs[_currentFiringIndex].MaxBulletAngle / 2f) * Mathf.Deg2Rad);

        Vector3 circle;
        if (_shootConfigs[_currentFiringIndex].UseWeightedSpread)
        {
            // Calculates a spread more weighted towards the centre of the circle.
            float circleAngle = UnityEngine.Random.value * Mathf.PI * 2f;
            float circleRadius = UnityEngine.Random.value;
            circle = new Vector2(Mathf.Cos(circleAngle) * circleRadius, Mathf.Sin(circleAngle) * circleRadius) * radius;
        }
        else
        {
            // Calculates a spread that is uniform within the circle
            circle = UnityEngine.Random.insideUnitCircle * radius;
        }

        // Calculate the direction the shot should travel.
        Vector3 direction = (fireDirection + _raycastOrigin.rotation * new Vector3(circle.x, circle.y)).normalized;

        return direction;
    }


    // Logic for shooting with Hitscan weapons.
    private void HitscanShoot(Vector3 fireDirection)
    {
        // Raycast to check if we hit something.
        if (Physics.Raycast(_raycastOrigin.position, fireDirection, out RaycastHit hit, float.MaxValue, _shootConfigs[_currentFiringIndex].HitMask))
        {
            // (Effect) Bullet Tracers.
            _bulletTrailManager.SpawnTrail(_bulletOrigin.position, hit.point, _trailConfig);

            // Hit logic.
            HandleShotHit(hit.distance, hit.point, hit.normal, hit.collider);
        }
        // We missed!
        else
        {
            // (Effect) Bullet Tracers.
            _bulletTrailManager.SpawnTrail(_bulletOrigin.position, _raycastOrigin.position + (fireDirection * _trailConfig.MissDistance), _trailConfig);
        }
    }

    // Logic for shooting with Projectile Weapons
    private void ProjectileShoot(Vector3 fireDirection)
    {
        // Create the bullet.
        Bullet bullet = _bulletPools[_shootConfigs[_currentFiringIndex]].Get();
        bullet.gameObject.SetActive(true);

        // Setup collision handling.
        bullet.OnCollision += HandleBulletCollision;

        // Finish bullet setup.
        bullet.transform.position = _bulletOrigin.transform.position;
        bullet.Spawn(fireDirection * _shootConfigs[_currentFiringIndex].BulletLaunchForce);


        // (Effect) Bullet Tracers.
        _bulletTrailManager.AttachBulletTrail(_trailConfig, bullet.transform);
    }
    private void HandleBulletCollision(Bullet bullet, Collision collision)
    {
        // (Effect) Disable the bullet tracers.
        TrailRenderer trail = bullet.GetComponentInChildren<TrailRenderer>();

        if (trail != null)
            _bulletTrailManager.ReleaseTrail(_trailConfig, trail);


        bullet.gameObject.SetActive(false);
        _bulletPools[_shootConfigs[_currentFiringIndex]].Release(bullet);


        // (Logic) Handle impact with a collider.
        if (collision != null)
        {
            // Get the point of contact to use in our distance calculation.
            ContactPoint contactPoint = collision.GetContact(0);

            HandleShotHit(Vector3.Distance(contactPoint.point, bullet.SpawnLocation),
                contactPoint.point,
                contactPoint.normal,
                contactPoint.otherCollider);
        }
    }


    private void HandleShotHit(float distanceTravelled, Vector3 hitLocation, Vector3 hitNormal, Collider hitCollider)
    {
        // (Effect) Spawn & Parent Impact Effects.
        GameObject bulletHole = Instantiate(_bulletHolePrefabs[UnityEngine.Random.Range(0, _bulletHolePrefabs.Length)], hitLocation, Quaternion.LookRotation(hitNormal));
        bulletHole.transform.parent = hitCollider.transform;


        // (Logic) Apply Damage.
        if (hitCollider.transform.TryGetComponentThroughParents<HealthComponent>(out HealthComponent healthComponent))
            healthComponent.TakeDamage(transform.position, _damageConfig.GetDamage(distanceTravelled));


        // (Logic) Apply Force.
        if (hitCollider.TryGetComponentThroughParents<Rigidbody>(out Rigidbody rigidbody))
        {
            Vector3 direction = (hitCollider.transform.position - transform.position).normalized;
            rigidbody.AddForceAtPosition(direction * _damageConfig.HitForce, hitLocation);

            OnHitPhysicsObject?.Invoke();
        }
    }


    private Bullet CreateBullet() => Instantiate(_shootConfigs[_currentFiringIndex].BulletPrefab);
    #endregion

    #region Reloading
    public void StartReload()
    {
        // Check that the current clip isn't already full, that we aren't completely out of ammo, and that we aren't already reloading.
        if (_clipAmmoRemainingProperty >= _ammoConfig.MaxClipAmmo || (_totalAmmoRemaining <= 0 && _ammoConfig.MaxAmmo != -1) || _isReloading)
            return;

        // Start Reloading.
        if (_ammoConfig.IndividualReloading)
            StartCoroutine(SteppedReload()); // For we reload individual bullets.
        else
            StartCoroutine(Reload()); // For if we reload the entire magazine at once.
    }

    private IEnumerator Reload()
    {
        // Add any remaining bullets back to the totalAmmoRemaining.
        _totalAmmoRemaining += _clipAmmoRemainingProperty;
        _clipAmmoRemainingProperty = 0;

        // Play the reload clip.
        _audioConfig?.PlayReloadClip(_audioSource);

        // Invoke the OnStartedReloading event.
        OnStartedReloading?.Invoke();

        // Wait ReloadTime seconds.
        _isReloading = true;
        yield return new WaitForSeconds(_ammoConfig.ReloadTime);
        _isReloading = false;

        // Replenish ammo.
        if (_ammoConfig.MaxAmmo == -1)
            _clipAmmoRemaining = _ammoConfig.MaxClipAmmo;
        else if (_totalAmmoRemaining < _ammoConfig.MaxClipAmmo)
        {
            _clipAmmoRemainingProperty = _totalAmmoRemaining;
            _totalAmmoRemaining = 0;
        }
        else
        {
            _clipAmmoRemainingProperty = _ammoConfig.MaxClipAmmo;
            _totalAmmoRemaining -= _ammoConfig.MaxClipAmmo;
        }
    }
    private IEnumerator SteppedReload()
    {
        // Invoke the OnStartedReloading event.
        OnStartedReloading?.Invoke();
        _isReloading = true;

        // Repeat until we have finished reloading or are out of ammo.
        while (_clipAmmoRemainingProperty < _ammoConfig.MaxClipAmmo && _totalAmmoRemaining > 0)
        {
            // Play the reload clip.
            _audioConfig.PlayIntermediateReload(_audioSource);

            // Wait one reloadStep.
            yield return new WaitForSeconds(_ammoConfig.ReloadTime);

            // Add a bullet to the clip.
            _clipAmmoRemainingProperty++;
            _totalAmmoRemaining--;
        }

        // Play the final reload clip.
        _audioConfig.PlayReloadClip(_audioSource);

        // Wait one more time.
        yield return new WaitForSeconds(_ammoConfig.ReloadTime);


        // Finished reloading.
        _isReloading = false;
    }
    #endregion

    #region Alternate Fire
    public void AttemptAlternateFire()
    {
        // If we don't have an Alternate Fire Mode, or it is on cooldown, then return.
        if (_alternateFireSO == null)
            return;
        if (Time.time <= _alternateFireSO.CooldownTime + _lastAlternateFireTime)
            return;

        // Set the last fire time (For Cooldown).
        _lastAlternateFireTime = Time.time;

        // Trigger the Alternate Fire mode.
        _alternateFireSO.AlternateAttack(gameObject, _raycastOrigin, _bulletOrigin);
    }
    #endregion

    #region Effects
    /// <summary> Apply recoil to the gun's model</summary>
    private void ApplyRecoil()
    {
        // We can't apply recoil to a model that doesn't exist.
        if (_model == null)
            return;

        _model.transform.localRotation = Quaternion.Euler(
            Mathf.Clamp(-(_model.transform.localRotation.eulerAngles.x + _shootConfigs[_currentFiringIndex].RecoilStrength), -_shootConfigs[_currentFiringIndex].MaxRecoil, 0f),
                _model.transform.localRotation.eulerAngles.y,
                _model.transform.localRotation.eulerAngles.z);
    }
    #endregion

    #region Gizmos
    private void OnDrawGizmosSelected()
    {
        /*float distanceToTarget = Physics.Raycast(_raycastOrigin.position, _raycastOrigin.forward, out RaycastHit hitInfo, 100f, _shootConfigs[_currentFiringIndex].HitMask) ? hitInfo.distance : 100f;
        float crosshairRadius = Mathf.Tan((_shootConfigs[_currentFiringIndex].MaxSpreadAngle / 2f) * Mathf.Deg2Rad) * distanceToTarget;
        TopPos = _raycastOrigin.forward * distanceToTarget + _raycastOrigin.up * crosshairRadius;

        float bottomRadius = Mathf.Tan(Mathf.Deg2Rad * _shootConfigs[_currentFiringIndex].MaxSpreadAngle / 2f);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(_raycastOrigin.position + TopPos, 0.25f);
        Gizmos.DrawSphere(_raycastOrigin.position + (_raycastOrigin.forward + -_raycastOrigin.up * bottomRadius), 0.1f);

        Gizmos.DrawSphere(Camera.main.ViewportToScreenPoint(_raycastOrigin.forward + _raycastOrigin.up * crosshairRadius), 25f);*/

        if (!_drawGizmos)
            return;


        // (Gizmos) Display the spread angle.
        if (_raycastOrigin != null)
        {
            Gizmos.color = Color.red;

            float halfAngle = _shootConfigs[_currentFiringIndex].MaxSpreadAngle / 2f;
            Vector3 upDirection = Quaternion.AngleAxis(halfAngle, _raycastOrigin.right) * _raycastOrigin.forward;
            Vector3 downDirection = Quaternion.AngleAxis(-halfAngle, _raycastOrigin.right) * _raycastOrigin.forward;
            Vector3 rightDirection = Quaternion.AngleAxis(halfAngle, _raycastOrigin.up) * _raycastOrigin.forward;
            Vector3 leftDirection = Quaternion.AngleAxis(-halfAngle, _raycastOrigin.up) * _raycastOrigin.forward;

            Gizmos.DrawRay(_raycastOrigin.position, upDirection * 10f);
            Gizmos.DrawRay(_raycastOrigin.position, downDirection * 10f);
            Gizmos.DrawRay(_raycastOrigin.position, rightDirection * 10f);
            Gizmos.DrawRay(_raycastOrigin.position, leftDirection * 10f);
        }
    }
    #endregion


    public float GetCrosshairSize() => _shootConfigs[_currentFiringIndex].CrosshairCurve.Evaluate(_spreadTime / _shootConfigs[_currentFiringIndex].MaxSpreadTime); // Get what the crosshair size should be.
}