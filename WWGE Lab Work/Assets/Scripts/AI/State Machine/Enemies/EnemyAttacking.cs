using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttacking : IState
{
    public EnemyAttacking()
    {

    }


    public void Tick()
    {

    }

    public void OnEnter()
    {
        Debug.Log("Attacking");
    }
    public void OnExit()
    {

    }
}
