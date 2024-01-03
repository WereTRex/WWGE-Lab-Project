using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class VictoryManager : MonoBehaviour
{
    [SerializeField] private HealthComponent _playerHealth;
    [SerializeField] private PlayerInput _playerInput;


    [Header("Victory")]
    [SerializeField] private GameObject _victoryScreen;
    [SerializeField] private TMP_Text _victoryScoreText;
    [SerializeField] private string _victoryScorePretext = "Time: ";
    private float _startTime;


    [Header("Defeat")]
    [SerializeField] private GameObject _defeatScreen;
    [SerializeField] private TMP_Text _defeatScoreText;
    [SerializeField] private string _defeatScorePretext = "Waves Completed: ";


    private void OnEnable()
    {
        WaveManager.OnFinalWaveCompleted += ShowVictoryScreen;
        _playerHealth.OnDead += ShowDefeatScreen;
    }
    private void OnDisable()
    {
        WaveManager.OnFinalWaveCompleted -= ShowVictoryScreen;
        _playerHealth.OnDead -= ShowDefeatScreen;
    }

    private void Start()
    {
        _victoryScreen.SetActive(false);
        _defeatScreen.SetActive(false);

        _startTime = Time.time;
    }


    void ShowVictoryScreen()
    {
        StartCoroutine(LerpTime());
        _playerInput.DeactivateInput();


        // Enable the victory screen.
        _victoryScreen.SetActive(true);

        // Set the score text.
        int minutes = Mathf.FloorToInt(Time.time / 60f);
        int seconds = Mathf.FloorToInt(Time.time - (minutes * 60f));
        _victoryScoreText.text = _victoryScorePretext + minutes + ":" + seconds;
    }
    void ShowDefeatScreen()
    {
        StartCoroutine(LerpTime());
        _playerInput.DeactivateInput();

        
        // Enable the defeat screen.
        _defeatScreen.SetActive(true);

        // Set the score text.
        _defeatScoreText.text = _defeatScorePretext + WaveManager.Instance.CurrentWave + "/" + WaveManager.Instance.MaxWaves;
    }

    private IEnumerator LerpTime()
    {
        while (Time.timeScale > 0)
        {
            Time.timeScale = Mathf.Clamp01(Time.timeScale - Time.unscaledDeltaTime);
            yield return null;
        }
    }
}
