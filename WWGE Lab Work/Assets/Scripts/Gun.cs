using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [SerializeField] private Camera _playerCamera;

    [SerializeField] private float _fireRate = 5; // How many bullets this weapon can fire in a second.
    private float _fireRateDelayRemaining = 0f;

    
    void Update()
    {
        if (_fireRateDelayRemaining > 0f)
            _fireRateDelayRemaining -= Time.deltaTime;

        if (Input.GetMouseButtonDown(0) && _fireRateDelayRemaining <= 0f)
        {
            Fire();
        }
    }

    private void Fire()
    {
        // Fire rate.
        _fireRateDelayRemaining = 1f / _fireRate;


        Ray ray = _playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        if (Physics.Raycast(ray, out RaycastHit hit))
            Debug.Log("I'm looking at " + hit.transform.name);
        else
            Debug.Log("I'm looking at nothing!");
    }
}
