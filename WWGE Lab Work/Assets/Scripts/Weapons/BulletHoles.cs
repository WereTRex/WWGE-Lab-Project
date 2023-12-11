using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletHoles : MonoBehaviour
{
    [SerializeField] private float _holeLifetimeTimer = 3f;
    private float _holeLifetimeTimerRemaining;

    private void Start() => _holeLifetimeTimerRemaining = _holeLifetimeTimer; // On creation, set the timer.
    

    private void Update()
    {
        // Decrement the lifetime remaining.
        _holeLifetimeTimerRemaining -= Time.deltaTime;

        // If the lifetime has elapsed, destroy this bullet hole.
        if (_holeLifetimeTimerRemaining < 0)
            Destroy(gameObject);
    }
}
