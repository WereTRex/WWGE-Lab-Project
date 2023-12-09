using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

/// <summary> A Manager Script for the Pause Menu</summary>
public class MenuManager : MonoBehaviour
{
    #region Variables
    [Header("Pause Menu Variables")]
    [SerializeField] private GameObject _pauseMenuRoot;
    [SerializeField] private GameObject _mainPauseRoot;
    [SerializeField] private GameObject _settingsMenuRoot;


    [Header("First Selected Options")]
    [SerializeField] private GameObject _mainPauseFirst;
    [SerializeField] private GameObject _settingsMenuFirst;


    [Header("Confirmation Variables")]
    [SerializeField] private GameObject _confirmationPrompt;
    [SerializeField] private float _confirmationDelay = 2f;


    [Header("Audio Variables")]
    [SerializeField] private AudioMixer _audioMixer;

    [Space(5)]

    [SerializeField] private Slider _masterSlider;
    [SerializeField] private Slider _musicSlider;
    [SerializeField] private Slider _soundEffectsSlider;


    [Header("Video Variables")]
    // FoV.
    [SerializeField] private Slider _fovSlider;
    [SerializeField] private float _defaultFOV;
    [Space(5)]

    // Resolution.
    [SerializeField] private TMP_Dropdown _resolutionDropdown;
    private Resolution[] _resolutions;
    private int _defaultResolutionIndex;
    [Space(5)]

    // Fullscreen.
    [SerializeField] private Toggle _fullscreenToggle;
    [Space(5)]


    // Crosshair.
    [SerializeField] private TMP_Dropdown _crosshairColourDropdown;
    [SerializeField] private Image _crosshairColourImage;
    [SerializeField] private Slider _crosshairBorderThicknessSlider;
    [SerializeField] private TMP_Dropdown _crosshairBorderColourDropdown;
    [SerializeField] private Image _crosshairBorderColourImage;


    [Header("Controls Variables")]
    // Mouse.
    [SerializeField] private Slider _mouseSensitivityHorizontalSlider;
    [SerializeField] private Slider _mouseSensitivityVerticalSlider;
    [SerializeField] private Vector2 _defaultMouseSensitivity = new Vector2(5f, 5f);

    [SerializeField] private Toggle _invertMouseYToggle;
    [Space(5)]

    // Gamepad.
    [SerializeField] private Slider _gamepadSensitivityHorizontalSlider;
    [SerializeField] private Slider _gamepadSensitivityVerticalSlider;
    [SerializeField] private Vector2 _defaultGamepadSensitivity = new Vector2(5f, 5f);

    [SerializeField] private Toggle _invertGamepadYToggle;


    [Header("Keybinding Variables")]
    private float _temp;
    #endregion


    #region Awake
    private void Awake()
    {
        #region Video Initialization.
        // Get the available resolutions of the screen.
        _resolutions = Screen.resolutions;

        // Setup the Resolution Dropdown.
        _resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();
        int currentResolutionIndex = 0;
        for (int i = 0; i < _resolutions.Length; i++)
        {
            options.Add(_resolutions[i].width + " x " + _resolutions[i].height);

            if (_resolutions[i].width == Screen.currentResolution.width && _resolutions[i].height == Screen.currentResolution.height)
                currentResolutionIndex = i;
        }
        _resolutionDropdown.AddOptions(options);

        // Set the initial value of the resolution dropdown to the current screen resolution.
        _defaultResolutionIndex = currentResolutionIndex;
        _resolutionDropdown.value = currentResolutionIndex;
        _resolutionDropdown.RefreshShownValue();


        // Set the initial values of the crosshair colour dropdowns.
        options.Clear();
        _crosshairColourDropdown.ClearOptions();
        _crosshairBorderColourDropdown.ClearOptions();
        for (int i = 0; i < CrosshairColours.Colours.Length; i++)
        {
            options.Add(CrosshairColours.Colours[i].Key);
        }
        _crosshairColourDropdown.AddOptions(options);
        _crosshairBorderColourDropdown.AddOptions(options);
        #endregion

        // Close the pause menu.
        ClosePauseMenu();
    }
    #endregion


    #region Handling Pausing
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
        // Ensure that displayed values are correct.
        ReconfirmDisplayedValues();

        // Ensure that only the main pause menu opens.
        foreach (Transform menu in _pauseMenuRoot.transform)
        {
            menu.gameObject.SetActive(false);
        }
        _mainPauseRoot.SetActive(true);

        // Open the pause menu.
        _pauseMenuRoot.SetActive(true);

        // Set the first button (Likely Resume) as the currently selected button to allow for Controller Navigation.
        EventSystem.current.SetSelectedGameObject(_mainPauseFirst);
        Debug.Log(EventSystem.current.currentSelectedGameObject.name);
    }
    public void ClosePauseMenu()
    {
        // Close the pause menu.
        _pauseMenuRoot.SetActive(false);
    }


    private void ReconfirmDisplayedValues()
    {
        // Audio.
        _audioMixer.GetFloat("masterVolume", out float masterVolume);
        _audioMixer.GetFloat("masterVolume", out float musicVolume);
        _audioMixer.GetFloat("masterVolume", out float effectsVolume);
        _masterSlider.value = masterVolume;
        _musicSlider.value = musicVolume;
        _soundEffectsSlider.value = effectsVolume;
        /*_masterSlider.value = PlayerPrefs.GetFloat("masterVolume");
        _musicSlider.value = PlayerPrefs.GetFloat("musicVolume");
        _soundEffectsSlider.value = PlayerPrefs.GetFloat("soundEffectsVolume");*/


        // Video.
        _fovSlider.value = PlayerManager.Instance.FieldOfView;

        for (int i = 0; i < _resolutions.Length; i++)
        {
            if (Screen.currentResolution.width == _resolutions[i].width && Screen.currentResolution.height == _resolutions[i].height)
                _resolutionDropdown.value = i;
        }

        _fullscreenToggle.isOn = Screen.fullScreen;

        _crosshairColourDropdown.value = CrosshairColours.FindIndexOfColour(PlayerManager.Instance.CrosshairColour);
        _crosshairBorderThicknessSlider.value = PlayerManager.Instance.CrosshairBorderThickness;
        _crosshairBorderColourDropdown.value = CrosshairColours.FindIndexOfColour(PlayerManager.Instance.CrosshairBorderColour);


        // Controls.
        _mouseSensitivityHorizontalSlider.value = PlayerManager.Instance.MouseSensitivity.x;
        _mouseSensitivityVerticalSlider.value = PlayerManager.Instance.MouseSensitivity.y;
        _invertMouseYToggle.isOn = PlayerManager.Instance.InvertMouseY;

        _gamepadSensitivityHorizontalSlider.value = PlayerManager.Instance.GamepadSensitivity.x;
        _gamepadSensitivityVerticalSlider.value = PlayerManager.Instance.GamepadSensitivity.y;
        _invertGamepadYToggle.isOn = PlayerManager.Instance.InvertGamepadY;


        // Keybindings.

    }
    #endregion


    #region Settings Menu
    public void OpenSettingsMenu()
    {
        // Ensure that only the settings menu is open.
        foreach (Transform menu in _pauseMenuRoot.transform)
        {
            menu.gameObject.SetActive(false);
        }
        _settingsMenuRoot.SetActive(true);

        // Allow for controller navigation.
        EventSystem.current.SetSelectedGameObject(_settingsMenuFirst);
    }
    public void BackToMain()
    {
        // Ensure that only the main pause menu opens.
        foreach (Transform menu in _pauseMenuRoot.transform)
        {
            menu.gameObject.SetActive(false);
        }
        _mainPauseRoot.SetActive(true);

        // Set the first button (Likely Resume) as the currently selected button to allow for Controller Navigation.
        EventSystem.current.SetSelectedGameObject(_mainPauseFirst);
    }

    
    #region Confirmation Box
    // Display a confirmation box for confirmationDelay seconds.
    private IEnumerator DisplayConfirmationBox()
    {
        _confirmationPrompt.SetActive(true);
        yield return new WaitForSecondsRealtime(_confirmationDelay);
        _confirmationPrompt.SetActive(false);
    }
    #endregion


    #region Audio
    public void SetMasterVolume(float newVolume) => _audioMixer.SetFloat("masterVolume", newVolume);
    public void SetMusicVolume(float newVolume) => _audioMixer.SetFloat("musicVolume", newVolume);
    public void SetSoundEffectVolume(float newVolume) => _audioMixer.SetFloat("soundEffectsVolume", newVolume);
    #endregion

    #region Video
    public void CrosshairColourChanged(int index) => _crosshairColourImage.color = CrosshairColours.Colours[index].Value;
    public void CrosshairBorderColourChanged(int index) => _crosshairBorderColourImage.color = CrosshairColours.Colours[index].Value;


    #region Apply/Reset
    public void ApplyVideoChanges()
    {
        // Set FoV.
        PlayerManager.Instance.FieldOfView = _fovSlider.value;

        // Set Resolution & Fullscreen.
        Screen.SetResolution(_resolutions[_resolutionDropdown.value].width, _resolutions[_resolutionDropdown.value].height, Screen.fullScreen);


        // Set Crosshair Values.
        PlayerManager.Instance.CrosshairColour = CrosshairColours.Colours[_crosshairColourDropdown.value].Value;
        PlayerManager.Instance.CrosshairBorderThickness = _crosshairBorderThicknessSlider.value;
        PlayerManager.Instance.CrosshairBorderColour = CrosshairColours.Colours[_crosshairBorderColourDropdown.value].Value;


        // Request Confirmation (Currently only confirmation box).
        StartCoroutine(DisplayConfirmationBox());
    }
    public void ResetVideoChanges()
    {
        // Reset FOV.
        _fovSlider.value = _defaultFOV;

        // Reset Resolution.
        _resolutionDropdown.value = _defaultResolutionIndex;

        // Reset Fullscreen.
        _fullscreenToggle.isOn = true;

        // Apply Resets.
        ApplyVideoChanges();
    }
    #endregion
    #endregion

    #region Controls
    #region Apply/Reset
    public void ApplyControlsChanges()
    {
        // Set Mouse Values.
        PlayerManager.Instance.MouseSensitivity = new Vector2(_mouseSensitivityHorizontalSlider.value, _mouseSensitivityVerticalSlider.value);
        PlayerManager.Instance.InvertMouseY = _invertMouseYToggle.isOn;

        // Set Gamepad Values.
        PlayerManager.Instance.GamepadSensitivity = new Vector2(_gamepadSensitivityHorizontalSlider.value, _gamepadSensitivityVerticalSlider.value);
        PlayerManager.Instance.InvertGamepadY = _invertGamepadYToggle.isOn;

        // Request Confirmation (Currently only confirmation box).
        StartCoroutine(DisplayConfirmationBox());
    }
    public void ResetControls()
    {
        // Reset Mouse Values.
        _mouseSensitivityHorizontalSlider.value = _defaultMouseSensitivity.x;
        _mouseSensitivityVerticalSlider.value = _defaultMouseSensitivity.y;
        _invertMouseYToggle.isOn = false;

        // Reset Gamepad Values.
        _gamepadSensitivityHorizontalSlider.value = _defaultGamepadSensitivity.x;
        _gamepadSensitivityVerticalSlider.value = _defaultGamepadSensitivity.y;
        _invertGamepadYToggle.isOn = false;

        // Apply Resets.
        ApplyControlsChanges();
    }
    #endregion
    #endregion

    // Note: Keybindings are handled elsewhere.
    #endregion
}