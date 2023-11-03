using System.Collections;
using System.IO.Pipes;
using UnityEngine;
using UnityEngine.Pool;

public class Gun : MonoBehaviour
{
    #region Variables
    [SerializeField] private DamageConfigSO _damageConfig;
    private bool _isAttacking = false;

    [SerializeField] private GameObject _model;
    [SerializeField] private ParticleSystem _muzzleFlashPS;


    [Header("Firing")]
    [SerializeField] private ShootingConfigSO _shootConfig;

    [SerializeField] private Transform _raycastOrigin;
    [SerializeField] private Transform _bulletOrigin;
    private float _lastShotTime = 0f;
    private float _spreadTime = 0f;

    private ObjectPool<Bullet> _bulletPool;
   

    [Header("Ammo and Reloading")]
    [SerializeField] private AmmoConfigSO _ammoConfig;
    private int _totalAmmoRemaining;
    private int _clipAmmoRemaining;

    private bool _isReloading = false;


    [Header("Alternate Fire")]
    [SerializeField] private AlternateFireSO _alternateFireSO;
    private float _lastAlternateFireTime;


    #region Effects
    [Header("Effects")]
    [SerializeField] private TrailConfigSO _trailConfig;
    private ObjectPool<TrailRenderer> _trailPool;

    [Space(5)]

    [SerializeField] private GameObject[] _bulletHolePrefabs;
    #endregion


    [Header("Gizmos")]
    [SerializeField] private bool _drawGizmos = false;
    #endregion


    #region Awake
    private void Awake()
    {
        _trailPool = new ObjectPool<TrailRenderer>(CreateTrail);

        if (!_shootConfig.IsHitscan)
            _bulletPool = new ObjectPool<Bullet>(CreateBullet);
    }
    #endregion

    #region Start
    private void Start()
    {
        _totalAmmoRemaining = _ammoConfig.MaxAmmo;
        _clipAmmoRemaining = _ammoConfig.MaxClipAmmo;
    }
    #endregion

    #region Update
    private void Update()
    {
        _spreadTime = Mathf.Clamp(_spreadTime + Time.deltaTime * (_isAttacking ? 1f : -_shootConfig.RecoilRecoverySpeed), 0, _shootConfig.MaxSpreadTime);

        if (_model != null)
        {
            _model.transform.localRotation = Quaternion.Lerp(_model.transform.localRotation, Quaternion.Euler(0f, 0f, 0f), Time.deltaTime * _shootConfig.RecoilRecoverySpeed);
        }

        if (_shootConfig.FiringType == FiringType.FullAuto && _isAttacking && !_isReloading)
            AttemptAttack();
    }
    #endregion

    #region Input
    public void StartAttacking()
    {
        if (_shootConfig.FiringType == FiringType.SingleFire)
        {
            AttemptAttack();
            _spreadTime = Mathf.Clamp(_spreadTime + _shootConfig.MaxSpreadTime / 2f, 0, _shootConfig.MaxSpreadTime);
        } else if (_shootConfig.FiringType == FiringType.FullAuto) {
            _isAttacking = true;
        }
    }
    public void StopAttacking()
    {
        _isAttacking = false;
    }
    #endregion

    #region Shooting
    void AttemptAttack()
    {
        // Check fireRate & if we are reloading.
        if (Time.time <= _shootConfig.FireDelay + _lastShotTime || _isReloading)
            return;

        // Check that we have ammo remaining.
        if (_clipAmmoRemaining <= 0)
        {
            if (_ammoConfig.AutoReloadWhenAttacking && _totalAmmoRemaining > 0)
            {
                StartReload();
            } else {
                // Click.
                return;
            }
        }

        // We can attack, so we do so.
        MakeAttack();
    }
    void MakeAttack()
    {
        // Fire Rate & Reduce Ammo.
        _lastShotTime = Time.time;
        _clipAmmoRemaining--;


        // Muzzle Flash & Audio.
        if (_muzzleFlashPS != null)
            _muzzleFlashPS.Play();


        // Calculate Fire Direction.
        Vector3 spreadAmount = _shootConfig.GetSpread(_spreadTime);
        Vector3 fireDirection;
        if (_model != null)
        {
            Debug.LogError("Applying recoil with this method has no constraints. We need to find a solution");
            _model.transform.forward += _model.transform.TransformDirection(spreadAmount);
            fireDirection = _model.transform.forward;
        }
        else
            fireDirection = (_raycastOrigin.forward + spreadAmount).normalized;


        for (int i = 0; i < _shootConfig.BulletsPerShot; i++)
        {
            // Bullet Deviation for things like Shotguns.
            Vector3 direction = GetShotDirection(fireDirection);

            Debug.DrawRay(_raycastOrigin.position, direction * 10f, Color.red, 1f);

            if (_shootConfig.IsHitscan)
                HitscanShoot(direction);
            else
                ProjectileShoot(direction);
        }
    }
    private Vector3 GetShotDirection(Vector3 fireDirection)
    {
        float radius = Mathf.Tan((_shootConfig.MaxBulletAngle / 2f) * Mathf.Deg2Rad);
        Vector3 circle;
        if (_shootConfig.UseWeightedSpread)
        {
            float circleAngle = Random.value * Mathf.PI * 2f;
            float circleRadius = Random.value;
            circle = new Vector2(Mathf.Cos(circleAngle) * circleRadius, Mathf.Sin(circleAngle) * circleRadius) * radius;
        } else {
            circle = Random.insideUnitCircle * radius;
        }
        Vector3 direction = (fireDirection + _raycastOrigin.rotation * new Vector3(circle.x, circle.y)).normalized;

        return direction;
    }



    private void HitscanShoot(Vector3 fireDirection)
    {
        // Raycast to find if we hit something.
        if (Physics.Raycast(_raycastOrigin.position, fireDirection, out RaycastHit hit, float.MaxValue, _shootConfig.HitMask))
        {
            // We hit something!

            // (Effect) Bullet Tracers.
            StartCoroutine(PlayTrail(_bulletOrigin.position, hit.point, hit));

            // Hit logic.
            HandleShotHit(hit.distance, hit.point, hit.normal, hit.collider);
        }
        else
        {
            // We missed!

            // (Effect) Bullet trails.
            StartCoroutine(PlayTrail(_bulletOrigin.position, _raycastOrigin.position + (fireDirection * _trailConfig.MissDistance), new RaycastHit()));
        }
    }
    
    private void ProjectileShoot(Vector3 fireDirection)
    {
        // Create the bullet.
        Bullet bullet = _bulletPool.Get();
        bullet.gameObject.SetActive(true);
        // Setup collision handling.
        bullet.OnCollision += HandleBulletCollision;
        // Finish bullet setup.
        bullet.transform.position = _bulletOrigin.transform.position;
        bullet.Spawn(fireDirection * _shootConfig.BulletLaunchForce);

        // (Effect) Bullet Tracers.
        TrailRenderer trail = _trailPool.Get();
        if (trail != null)
        {
            trail.transform.SetParent(bullet.transform, false);
            trail.transform.localPosition = Vector3.zero;
            trail.emitting = true;
            trail.gameObject.SetActive(true);
        }
    }
    private void HandleBulletCollision(Bullet bullet, Collision collision)
    {
        // (Effect) Disable the bullet tracers.
        TrailRenderer trail = bullet.GetComponentInChildren<TrailRenderer>();

        if (trail != null)
        {
            trail.transform.SetParent(null, true);
            StartCoroutine(DelayedDisableTrail(trail));
        }

        bullet.gameObject.SetActive(false);
        _bulletPool.Release(bullet);


        // (Logic) Handle impact with a collider.
        if (collision != null)
        {
            ContactPoint contactPoint = collision.GetContact(0);

            HandleShotHit(Vector3.Distance(contactPoint.point, bullet.SpawnLocation),
                contactPoint.point,
                contactPoint.normal,
                contactPoint.otherCollider);
        }
    }
    private IEnumerator DelayedDisableTrail(TrailRenderer trail)
    {
        yield return new WaitForSeconds(_trailConfig.Duration);
        yield return null;

        trail.emitting = false;
        trail.gameObject.SetActive(false);
        _trailPool.Release(trail);
    }


    private void HandleShotHit(float distanceTravelled, Vector3 hitLocation, Vector3 hitNormal, Collider hitCollider)
    {
        // (Effect) Ready Impact Effects.
        GameObject bulletHole = Instantiate(_bulletHolePrefabs[Random.Range(0, _bulletHolePrefabs.Length)], hitLocation, Quaternion.LookRotation(hitNormal));


        // (Logic) Apply Damage.
        if (hitCollider.transform.TryGetComponent<HealthComponent>(out HealthComponent healthComponent))
            healthComponent.TakeDamage(_damageConfig.GetDamage(distanceTravelled));


        // (Logic) Apply Force.
        if (hitCollider.TryGetComponent<Rigidbody>(out Rigidbody rigidbody))
        {
            Vector3 direction = (hitCollider.transform.position - transform.position).normalized;
            rigidbody.AddForceAtPosition(direction * _damageConfig.HitForce, hitLocation);

            // (Effect) Attach impact effects to physics object.
            bulletHole.transform.parent = hitCollider.transform;
        }
    }


    private Bullet CreateBullet() => Instantiate(_shootConfig.BulletPrefab);
    #endregion

    #region Reloading
    public void StartReload()
    {
        // Check that the current clip isn't already full, that we aren't completely out of ammo, and that we aren't already reloading.
        if (_clipAmmoRemaining >= _ammoConfig.MaxClipAmmo || _totalAmmoRemaining <= 0 || _isReloading)
            return;

        // Start Reloading.
        StartCoroutine(Reload());
    }

    private IEnumerator Reload()
    {
        _totalAmmoRemaining += _clipAmmoRemaining;
        _clipAmmoRemaining = 0;

        _isReloading = true;
        yield return new WaitForSeconds(_ammoConfig.ReloadTime);
        _isReloading = false;

        if (_totalAmmoRemaining < _ammoConfig.MaxClipAmmo)
        {
            _clipAmmoRemaining = _totalAmmoRemaining;
            _totalAmmoRemaining = 0;
        } else {
            _clipAmmoRemaining = _ammoConfig.MaxClipAmmo;
            _totalAmmoRemaining -= _ammoConfig.MaxClipAmmo;
        }
    }
    #endregion

    #region Fire Modes
    public void SwitchFiringType()
    {
        /*if (_currentFiringTypeIndex < _availableFiringTypes.Length - 1)
            _currentFiringTypeIndex++;
        else
            _currentFiringTypeIndex = 0;*/
        throw new System.NotImplementedException();
    }
    #endregion

    #region Alternate Fire
    public void AttemptAlternateFire()
    {
        if (Time.time <= _alternateFireSO.CooldownTime + _lastAlternateFireTime)
            return;

        _lastAlternateFireTime = Time.time;
        _alternateFireSO.AlternateAttack(gameObject, _raycastOrigin, _bulletOrigin);
    }
    #endregion

    #region Effects
    // Setup a TrailRender for the ObjectPool.
    private TrailRenderer CreateTrail()
    {
        GameObject instance = new GameObject("Bullet Trail");
        TrailRenderer trail = instance.AddComponent<TrailRenderer>();

        trail.colorGradient = _trailConfig.Colour;
        trail.material = _trailConfig.Material;
        trail.widthCurve = _trailConfig.WidthCurve;
        trail.time = _trailConfig.Duration;
        trail.minVertexDistance = _trailConfig.MinVertexDistance;

        trail.emitting = false;
        trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        return trail;
    }
    private IEnumerator PlayTrail(Vector3 startPoint, Vector3 endPoint, RaycastHit hit)
    {
        // Get a TrailRenderer from the pool.
        TrailRenderer instance = _trailPool.Get();
        instance.gameObject.SetActive(true);
        instance.transform.position = startPoint;

        // Avoid a potential position carry-over from the last frame if the instance was reused.
        yield return null;

        instance.emitting = true;

        // Move the TrailRenderer from the startPoint to the endPoint.
        float distance = Vector3.Distance(startPoint, endPoint);
        float remainingDistance = distance;
        while (remainingDistance > 0)
        {
            instance.transform.position = Vector3.Lerp(startPoint, endPoint, Mathf.Clamp01(1 - (remainingDistance / distance)));
            remainingDistance -= _trailConfig.SimulationSpeed * Time.deltaTime;

            yield return null;
        }

        instance.transform.position = endPoint;

        // Note: If we wish, we could move the logic for Hitscan collision to here to simulate the bullet travelling (This is why we have the hit variable, just I am undecided).
        
        // Allow for the trail die.
        yield return new WaitForSeconds(_trailConfig.Duration);
        yield return null;

        // Disable the trail and pass it back to the object pool.
        instance.emitting = false;
        instance.gameObject.SetActive(false);
        _trailPool.Release(instance);
    }
    #endregion

    #region Gizmos
    private void OnDrawGizmosSelected()
    {
        if (!_drawGizmos)
            return;


        if (_raycastOrigin != null)
        {
            Gizmos.color = Color.red;

            // Get direction within a cone.
            float radius = Mathf.Tan((_shootConfig.MaxBulletAngle / 2f) * Mathf.Deg2Rad);
            Vector2 verticalCircle = Vector3.up * radius;
            Vector3 upDirection = _raycastOrigin.forward + _raycastOrigin.rotation * new Vector3(verticalCircle.x, verticalCircle.y);
            Vector3 downDirection = _raycastOrigin.forward + _raycastOrigin.rotation * new Vector3(verticalCircle.x, -verticalCircle.y);
            
            Gizmos.DrawRay(_raycastOrigin.position, upDirection * 10f);
            Gizmos.DrawRay(_raycastOrigin.position, downDirection * 10f);
            
            Vector2 horizontalCircle = Vector3.right * radius;
            Vector3 rightDirection = _raycastOrigin.forward + _raycastOrigin.rotation * new Vector3(horizontalCircle.x, horizontalCircle.y);
            Vector3 leftDirection = _raycastOrigin.forward + _raycastOrigin.rotation * new Vector3(-horizontalCircle.x, horizontalCircle.y);

            Gizmos.DrawRay(_raycastOrigin.position, leftDirection * 10f);
            Gizmos.DrawRay(_raycastOrigin.position, rightDirection * 10f);
        }
    }
    #endregion
}