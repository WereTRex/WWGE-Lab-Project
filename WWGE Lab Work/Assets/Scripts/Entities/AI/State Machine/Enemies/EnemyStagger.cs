using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Note: The script that creates and manages this Stagger State will handle exiting of the stagger via comparison of the StaggerDurationElapsed public accessor.
/// <summary> A state for when an enemy has been staggered.</summary>
public class EnemyStagger : IState
{
    private float _staggerDurationElapsed;
    public float StaggerDurationElapsed { get => _staggerDurationElapsed; }

    public EnemyStagger() { }


    public void Tick() => _staggerDurationElapsed += Time.deltaTime;

    public void OnEnter() => _staggerDurationElapsed = 0f; // Reset the stager duration variable.
    public void OnExit() { }
}
