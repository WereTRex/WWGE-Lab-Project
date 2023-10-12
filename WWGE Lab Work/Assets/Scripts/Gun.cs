using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [SerializeField] private Camera _playerCamera;
    [SerializeField] private float _force;
    [SerializeField] private GameObject[] _bulletHoldPrefabs;
    

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
        if (Time.timeScale == 0f)
            return;

        if (_fireRateDelayRemaining > 0f)
            _fireRateDelayRemaining -= Time.deltaTime;
        if (_isReloading)
            return;



        // Note: We don't need to check if we are reloading, but we shall do it anyway to prevent possible errors if we change the reload function.
        if (((Input.GetMouseButton(0) && _fullAuto) || Input.GetMouseButtonDown(0)) && _fireRateDelayRemaining <= 0f && !_isReloading)
        {
            if (_currentAmmo > 0)
            {
                Fire();

                _fireRateDelayRemaining = _fireDelay;
                _currentAmmo--;
            } else
                StartCoroutine(Reload());
        }

        if (Input.GetKeyDown(KeyCode.R) && (_currentAmmo != _maxClipSize && _ammoRemaining > 0))
        {
            StartCoroutine(Reload());
        }
    }

    private void Fire()
    {
        // Shooting.
        Ray ray = _playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            GameObject chosenBulletHole = _bulletHoldPrefabs[Random.Range(0, _bulletHoldPrefabs.Length)];

            // Add force.
            if (hit.rigidbody)
            {
                var direction = new Vector3(hit.transform.position.x - transform.position.x,
                    hit.transform.position.y - transform.position.y,
                    hit.transform.position.z - transform.position.z);
                hit.rigidbody.AddForceAtPosition(_force * Vector3.Normalize(direction), hit.point);

                // Create Bullet Holes and make them a child of the Physics Object.
                var tempBullet = Instantiate(chosenBulletHole, hit.point, Quaternion.LookRotation(hit.normal));
                tempBullet.transform.parent = hit.transform;
            }
            else
            {
                // Create Bullet Holes if it isn't a physics object.
                Instantiate(chosenBulletHole, hit.point, Quaternion.LookRotation(hit.normal));
            }
        }
        else
            Debug.Log("I'm looking at nothing!");
    }


    private IEnumerator Reload()
    {
        _ammoRemaining += _currentAmmo;
        _currentAmmo = 0;
        
        _isReloading = true;
        yield return new WaitForSeconds(_reloadTime);
        
        _isReloading = false;
        FinishedReload();
    }

    private void FinishedReload()
    {        
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
