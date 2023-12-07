using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WaveCounterUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _currentWaveText;
    [SerializeField] private TMP_Text _timeTillNextWaveText;
    [SerializeField] private GameObject _divider;

    public void SetTimer(float timeRemaining)
    {
        _timeTillNextWaveText.enabled = true;
        _divider.SetActive(true);
        
        int seconds = Mathf.FloorToInt(timeRemaining);
        int decimals = Mathf.FloorToInt((timeRemaining - seconds) * 100);
        _timeTillNextWaveText.text = string.Format("{0:00}:{1:00}", seconds, decimals);
    }
    public void HideTimer()
    {
        _timeTillNextWaveText.enabled = false;
        _divider.SetActive(false);
    }

    public void SetWaveText(int currentWave, int totalWaveCount) => _currentWaveText.text = string.Format("{0}/{1}", currentWave, totalWaveCount);    
}
