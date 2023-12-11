using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> A Manager script used to keep track of player related setting information. </summary>
public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;

    private void Awake()
    {
        // Set the singleton instance.
        if (Instance == null)
            Instance = this;
    }
    private void OnEnable()
    {
        if (Instance == null)
            Instance = this;
    }


    // Camera Related Stuff.
    [ReadOnly] public float FieldOfView;

    [ReadOnly] public Vector2 MouseSensitivity;
    [ReadOnly] public bool InvertMouseY = false;

    [ReadOnly] public Vector2 GamepadSensitivity;
    [ReadOnly] public bool InvertGamepadY = false;


    // UI Related Stuff.
    [ReadOnly] public Color CrosshairColour;
    [ReadOnly] public float CrosshairBorderThickness;
    [ReadOnly] public Color CrosshairBorderColour;
}
