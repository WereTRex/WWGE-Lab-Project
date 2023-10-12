using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Manager : MonoBehaviour
{
    private bool _paused;

    [SerializeField] private Slider _fovSlider;
    [SerializeField] private Camera _cam;


    private void Start()
    {
        _paused = false;
        _fovSlider.gameObject.SetActive(false);
        _cam.fieldOfView = _fovSlider.value;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            _paused = !_paused;
            TogglePauseEffects();
        }

        _cam.fieldOfView = _fovSlider.value;
    }

    void TogglePauseEffects()
    {
        if (_paused)
        {
            // Pause Game.
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;

            // Show Pause Menu.
            _fovSlider.gameObject.SetActive(true);
        }
        else
        {
            // Resume Game.
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;

            // Hide Pause Menu.
            _fovSlider.gameObject.SetActive(false);
        }
    }
}
