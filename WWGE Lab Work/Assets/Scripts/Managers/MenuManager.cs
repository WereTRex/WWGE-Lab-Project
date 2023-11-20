using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;

public class MenuManager : MonoBehaviour
{
    #region Variables
    [Header("Pause Menu Variables")]
    [SerializeField] private GameObject _pauseMenuRoot;
    [SerializeField] private GameObject _mainPauseRoot;


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
        for (int i = 0; i < System.Enum.GetValues(typeof(CrosshairColours)).Length; i++)
        {
            options.Add(((CrosshairColours)i).ToString());
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
        _fovSlider.value = PlayerPrefs.GetFloat("fieldOfView");

        for (int i = 0; i < _resolutions.Length; i++)
        {
            if (Screen.currentResolution.width == _resolutions[i].width && Screen.currentResolution.height == _resolutions[i].height)
                _resolutionDropdown.value = i;
        }

        _fullscreenToggle.isOn = Screen.fullScreen;

        _crosshairColourDropdown.value = GetIndexFromColour(PlayerManager.Instance.CrosshairColour);
        _crosshairBorderThicknessSlider.value = PlayerManager.Instance.CrosshairBorderThickness;
        _crosshairBorderColourDropdown.value = GetIndexFromColour(PlayerManager.Instance.CrosshairBorderColour);


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
    public void BackToMain()
    {
        // Ensure that only the main pause menu opens.
        foreach (Transform menu in _pauseMenuRoot.transform)
        {
            menu.gameObject.SetActive(false);
        }
        _mainPauseRoot.SetActive(true);
    }

    
    #region Confirmation Box
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
    private Color GetCrosshairColour(int index)
    {
        switch((CrosshairColours)index)
        {
            case CrosshairColours.White:
                return Color.white;
            case CrosshairColours.Black:
                return Color.black;
            case CrosshairColours.Yellow:
                return Color.yellow;
            case CrosshairColours.Blue:
                return Color.blue;
            case CrosshairColours.Pink:
                return new Color(r: 252, g: 21, b: 132);
            default:
                return Color.white;
        }
    }
    private int GetIndexFromColour(Color color)
    {
        if (color == Color.white)
            return (int)CrosshairColours.White;
        else if (color == Color.black)
            return (int)CrosshairColours.Black;
        else if (color == Color.yellow)
            return (int)CrosshairColours.Yellow;
        else if (color == Color.blue)
            return (int)CrosshairColours.Blue;
        else if (color == new Color(r: 252, g: 21, b: 132))
            return (int)CrosshairColours.Pink;
        else
            return 0;
    }
    public void CrosshairColourChanged(int index) => _crosshairColourImage.color = GetCrosshairColour(index);
    
    public void CrosshairBorderColourChanged(int index) => _crosshairBorderColourImage.color = GetCrosshairColour(index);


    #region Apply/Reset
    /*public void ApplyVideoChanges()
    {
        // FoV.
        PlayerPrefs.SetFloat("fieldOfView", _fovSlider.value);

        // Resolution & Fullscreen.
        PlayerPrefs.SetInt("resolutionWidth", _resolutions[_resolutionDropdown.value].width);
        PlayerPrefs.SetInt("resolutionHeight", _resolutions[_resolutionDropdown.value].height);
        PlayerPrefs.SetInt("isFullscreen", _fullscreenToggle.isOn ? 1 : 0);

        Screen.SetResolution(_resolutions[_resolutionDropdown.value].width, _resolutions[_resolutionDropdown.value].height, Screen.fullScreen);


        // Request Confirmation.
        StartCoroutine(DisplayConfirmationBox());
    }*/
    public void ApplyVideoChanges()
    {
        // FoV.
        PlayerManager.Instance.FieldOfView = _fovSlider.value;

        // Resolution & Fullscreen.
        Screen.SetResolution(_resolutions[_resolutionDropdown.value].width, _resolutions[_resolutionDropdown.value].height, Screen.fullScreen);


        // Crosshair.
        PlayerManager.Instance.CrosshairColour = GetCrosshairColour(_crosshairColourDropdown.value);
        PlayerManager.Instance.CrosshairBorderThickness = _crosshairBorderThicknessSlider.value;
        PlayerManager.Instance.CrosshairBorderColour = GetCrosshairColour(_crosshairBorderColourDropdown.value);


        // Request Confirmation.
        StartCoroutine(DisplayConfirmationBox());
    }
    public void ResetVideoChanges()
    {
        // FOV.
        _fovSlider.value = _defaultFOV;

        // Resolution.
        _resolutionDropdown.value = _defaultResolutionIndex;

        // Fullscreen.
        _fullscreenToggle.isOn = true;

        // Apply.
        ApplyVideoChanges();
    }
    #endregion
    #endregion

    #region Controls
    #region Apply/Reset
    /*public void ApplyControlsChanges()
    {
        // Mouse.
        PlayerPrefs.SetFloat("mouseHorizontalSensitivity", _mouseSensitivityHorizontalSlider.value);
        PlayerPrefs.SetFloat("mouseVerticalSensitivity", _mouseSensitivityVerticalSlider.value);
        PlayerPrefs.SetInt("invertMouseY", _invertMouseYToggle.isOn ? 1 : 0);

        // Gamepad.
        PlayerPrefs.SetFloat("gamepadHorizontalSensitivity", _gamepadSensitivityHorizontalSlider.value);
        PlayerPrefs.SetFloat("gamepadVerticalSensitivity", _gamepadSensitivityVerticalSlider.value);
        PlayerPrefs.SetInt("invertGamepadY", _invertGamepadYToggle.isOn ? 1 : 0);

        // Request Confirmation.
        StartCoroutine(DisplayConfirmationBox());
    }*/
    public void ApplyControlsChanges()
    {
        // Mouse.
        PlayerManager.Instance.MouseSensitivity = new Vector2(_mouseSensitivityHorizontalSlider.value, _mouseSensitivityVerticalSlider.value);
        PlayerManager.Instance.InvertMouseY = _invertMouseYToggle.isOn;

        // Gamepad.
        PlayerManager.Instance.GamepadSensitivity = new Vector2(_gamepadSensitivityHorizontalSlider.value, _gamepadSensitivityVerticalSlider.value);
        PlayerManager.Instance.InvertGamepadY = _invertGamepadYToggle.isOn;

        // Request Confirmation.
        StartCoroutine(DisplayConfirmationBox());
    }
    public void ResetControls()
    {
        // Reset Mouse.
        _mouseSensitivityHorizontalSlider.value = _defaultMouseSensitivity.x;
        _mouseSensitivityVerticalSlider.value = _defaultMouseSensitivity.y;
        _invertMouseYToggle.isOn = false;

        // Reset Gamepad.
        _gamepadSensitivityHorizontalSlider.value = _defaultGamepadSensitivity.x;
        _gamepadSensitivityVerticalSlider.value = _defaultGamepadSensitivity.y;
        _invertGamepadYToggle.isOn = false;

        // Apply.
        ApplyControlsChanges();
    }
    #endregion
    #endregion

    #region Keybindings

    #endregion
    #endregion
}

enum CrosshairColours
{
    White,
    Black,
    Yellow,
    Blue,
    Pink,
}