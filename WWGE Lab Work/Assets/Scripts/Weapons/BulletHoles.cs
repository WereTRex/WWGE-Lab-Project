using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletHoles : MonoBehaviour
{
    [SerializeField] private float _holeLifetimeTimer = 3f;
    private float _holeLifetimeTimerRemaining;

    private void Start()
    {
        _holeLifetimeTimerRemaining = _holeLifetimeTimer;
    }

    private void Update()
    {
        _holeLifetimeTimerRemaining -= Time.deltaTime;

        if (_holeLifetimeTimerRemaining < 0)
            Destroy(gameObject);
    }
}
