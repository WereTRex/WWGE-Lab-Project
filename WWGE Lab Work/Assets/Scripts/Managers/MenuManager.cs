using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private GameObject _pauseMenuRoot;
    [SerializeField] private GameObject _mainPauseRoot;

    private void Awake() => ClosePauseMenu();


    public void OnMenuOpenPressed(InputAction.CallbackContext context)
    {
        if (context.performed)
            Pause();
    }
    public void OnMenuClosedPressed(InputAction.CallbackContext context)
    {
        if (context.performed)
            Unpause();
    }
    public void OnToggleMenuPressed(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (PauseManager.Instance.IsPaused)
                Unpause();
            else
                Pause();
        }
    }


    public void Pause()
    {
        OpenPauseMenu();
        PauseManager.Instance.PauseGame();
    }
    public void Unpause()
    {
        ClosePauseMenu();
        PauseManager.Instance.UnpauseGame();
    }


    private void OpenPauseMenu()
    {
        // Ensure that only the main pause menu opens.
        foreach (Transform menu in _pauseMenuRoot.transform)
        {
            menu.gameObject.SetActive(false);
        }
        _mainPauseRoot.SetActive(true);

        // Open the pause menu.
        _pauseMenuRoot.SetActive(true);
    }
    public void ClosePauseMenu()
    {
        // Close the pause menu.
        _pauseMenuRoot.SetActive(false);
    }
}
