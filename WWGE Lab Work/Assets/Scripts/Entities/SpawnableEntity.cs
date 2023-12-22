using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SpawnableEntity : MonoBehaviour
{
    public abstract void SetInitialTarget(Transform target);
}
