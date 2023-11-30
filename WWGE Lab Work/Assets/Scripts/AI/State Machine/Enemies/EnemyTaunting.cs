using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTaunting : IState
{
    private float _tauntDurationElapsed;
    public float TauntDurationElapsed { get => _tauntDurationElapsed; }
    
    public EnemyTaunting()
    {
        
    }


    public void Tick()
    {
        _tauntDurationElapsed += Time.deltaTime;
    }

    public void OnEnter()
    {
        Debug.Log("Taunt Started");
        _tauntDurationElapsed = 0;
    }
    public void OnExit() { Debug.Log("Taunt Elapsed"); }
}
