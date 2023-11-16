using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance;

    [SerializeField] private PlayerInput _playerInput;
    public bool IsPaused;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;

        IsPaused = false;
    }


    public void PauseGame()
    {
        // Pause Logic.
        _playerInput.SwitchCurrentActionMap("UI");
        Cursor.lockState = CursorLockMode.None;
        AudioListener.pause = true;
        Time.timeScale = 0f;
        
        // Set IsPaused.
        IsPaused = true;
    }
    public void UnpauseGame()
    {
        // Unpause Logic.
        _playerInput.SwitchCurrentActionMap("Default");
        Cursor.lockState = CursorLockMode.Locked;
        AudioListener.pause = false;
        Time.timeScale = 1f;
        
        // Set IsPaused.
        IsPaused = false;
    }
}
