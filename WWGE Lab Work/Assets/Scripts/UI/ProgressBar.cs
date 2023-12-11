using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
    [SerializeField] private Image _mask;
    [SerializeField] private Image _fill;

    [Space(5)]

    [SerializeField] private Gradient _gradient;

    [Space(5)]

    [SerializeField] float _timeToDrain = 0.25f;
    private float _fillTarget;
    private Coroutine drainHealthBarCoroutine;


    // Update the progress bar.
    public void UpdateProgressBar(float maximum, float current)
    {
        // Set our current fill target.
        _fillTarget = Mathf.Clamp01(current / maximum);

        // Stop any current drain coroutines.
        if (drainHealthBarCoroutine != null)
            StopCoroutine(drainHealthBarCoroutine);

        // Start draining the progress bar.
        drainHealthBarCoroutine = StartCoroutine(DrainProgressBar());
    }

    // Drain/Fill the Progress Bar over time.
    private IEnumerator DrainProgressBar()
    {
        float initialFillAmount = _mask.fillAmount;
        
        float elapsedTime = 0f;
        while(elapsedTime < _timeToDrain)
        {
            elapsedTime += Time.deltaTime;

            _mask.fillAmount = Mathf.Lerp(initialFillAmount, _fillTarget, (elapsedTime / _timeToDrain));
            _fill.color = _gradient.Evaluate(_mask.fillAmount);
            
            yield return null;
        }
    }
}
