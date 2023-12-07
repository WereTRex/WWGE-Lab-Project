using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> An interface that every state will inherit from.</summary>
public interface IState
{
    public void Tick();

    public void OnEnter();
    public void OnExit();
}
