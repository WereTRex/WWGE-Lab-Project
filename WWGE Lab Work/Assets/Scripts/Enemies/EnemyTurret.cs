using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTurret : MonoBehaviour
{
    [SerializeField] private Vector3 _targetDir;
    [SerializeField] private GameObject _player;
    [SerializeField] private float _speed;

    [SerializeField] private float _maxFollowAngle = 0.45f;
    [SerializeField] private float _maxVisibilityDistance;


    [Space(5)]
    [SerializeField] private ParticleSystem _alertPS;
    private bool _playerPreviouslySighted;

    private void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player");
    }


    private void Update()
    {
        if (PlayerWithinSightDistance() && PlayerVisible())
        {
            _targetDir = (_player.transform.position - transform.position).normalized;

            // Rotate to face the player.
            Vector3 newDirection = Vector3.RotateTowards(transform.forward, _targetDir, _speed * Time.deltaTime, 0.0f);
            transform.rotation = Quaternion.LookRotation(newDirection);


            if (_playerPreviouslySighted != true)
            {
                _alertPS.Play();
                _playerPreviouslySighted = true;
            }
        }
        else
            _playerPreviouslySighted = false;
    }


    private bool PlayerWithinSightDistance()
    {
        if (Vector3.Distance(_player.transform.position, transform.position) <= _maxVisibilityDistance)
            return true;
        else
            return false;
    }
    private bool PlayerVisible()
    {
        float dot = Vector3.Dot(transform.forward, (_player.transform.position - transform.position).normalized);

        if (dot > _maxFollowAngle)
            return true;
        else
            return false;
    }
}
