using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTaunting : IState
{
    public float TauntDurationElapsed { get; private set; }

    public EnemyTaunting() { }


    public void Tick()
    {
        TauntDurationElapsed += Time.deltaTime;
    }

    public void OnEnter()
    {
        Debug.Log("Taunt Started");
        TauntDurationElapsed = 0;
    }
    public void OnExit() { Debug.Log("Taunt Elapsed"); }
}
