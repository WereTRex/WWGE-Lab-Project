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


    public void UpdateProgressBar(float maximum, float current)
    {
        _fillTarget = Mathf.Clamp01(current / maximum);
        drainHealthBarCoroutine = StartCoroutine(DrainHealthBar());
    }

    private IEnumerator DrainHealthBar()
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
