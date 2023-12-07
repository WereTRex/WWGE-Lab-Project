using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldCanvasConnector : MonoBehaviour
{
    [field: SerializeField] public Transform WorldCanvas { get; private set; }
    public static WorldCanvasConnector Instance;

    private void Awake()
    {
        if (Instance ==  null)
            Instance = this;
    }
}
