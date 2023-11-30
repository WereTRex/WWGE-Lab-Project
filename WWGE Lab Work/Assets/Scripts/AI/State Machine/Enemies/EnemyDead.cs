using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDead : IState
{
    private readonly GameObject _enemy;
    
    public EnemyDead(GameObject enemy)
    {
        this._enemy = enemy;
    }


    public void Tick()
    {

    }

    public void OnEnter()
    {
        Debug.Log("Dead");
        GameObject.Destroy(this._enemy, 0.5f);
    }
    public void OnExit() { }
}
