using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDead : IState
{
    public EnemyDead()
    {

    }


    public void Tick()
    {

    }

    public void OnEnter()
    {
        Debug.Log("Dead");
    }
    public void OnExit() { }
}
