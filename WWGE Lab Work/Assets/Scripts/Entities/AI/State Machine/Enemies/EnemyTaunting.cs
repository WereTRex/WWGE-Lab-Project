using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Note: Similarly to EnemyStagger, the script that creates this state will handle figuring out when the duration has elapsed.
/// <summary> A state for when an enemy is doing a taunt.</summary>
public class EnemyTaunting : IState
{
    public float TauntDurationElapsed { get; private set; }

    public EnemyTaunting() { }


    public void Tick() => TauntDurationElapsed += Time.deltaTime; // Increment the currently elapsed taunt time.
    

    public void OnEnter() => TauntDurationElapsed = 0; // Reset the currently elapsed taunt time.
    public void OnExit() { }
}
