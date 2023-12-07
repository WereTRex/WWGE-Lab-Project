using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> A state for when an entity dies.</summary>
public class EnemyDead : IState
{
    private readonly GameObject _enemy;
    
    public EnemyDead(GameObject enemy)
    {
        // Set readonly values.
        this._enemy = enemy;
    }


    public void Tick() { }

    public void OnEnter() => GameObject.Destroy(this._enemy, 0.5f); // On entry, destroy the enemy GameObject after 0.5 seconds.
    public void OnExit() { }
}
