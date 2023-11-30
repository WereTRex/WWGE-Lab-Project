using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStagger : IState
{
    private float _staggerDurationElapsed;
    public float StaggerDurationElapsed { get => _staggerDurationElapsed; }

    public EnemyStagger()
    {
        
    }


    public void Tick()
    {
        _staggerDurationElapsed += Time.deltaTime;
    }

    public void OnEnter()
    {
        Debug.Log("Stagger Started"); 
        _staggerDurationElapsed = 0f;
    }
    public void OnExit() { Debug.Log("Stagger Elapsed"); }
}
