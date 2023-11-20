using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CrosshairReticlePiece : MonoBehaviour
{
    [System.Serializable]
    enum ReticlePieceAlignment
    {
        Vertical = 0,
        Horizontal = 1,
        Corner = 2,
        Centre = 3
    }
    [SerializeField] private ReticlePieceAlignment _type;
    [SerializeField] private float _thicknessMultiplier = 1;
    private RectTransform _rectTransform;
    [SerializeField] private Image _interiorImage;

    [Space(5)]

    private float _borderThickness = 1f;
    [SerializeField] private RectTransform _borderTransform;
    [SerializeField] private Image _borderImage;

    private void Start()
    {
        _rectTransform = GetComponent<RectTransform>();

        SetBorderThickness(_borderThickness);
    }


    public void SetPieceThickness(float newThickness)
    {
        newThickness *= _thicknessMultiplier;

        switch (_type)
        {
            case ReticlePieceAlignment.Vertical:
                _rectTransform.sizeDelta = new Vector2(newThickness, _rectTransform.sizeDelta.y);
                break;
            case ReticlePieceAlignment.Horizontal:
                _rectTransform.sizeDelta = new Vector2(_rectTransform.sizeDelta.x, newThickness);
                break;
            case ReticlePieceAlignment.Corner:
                _rectTransform.sizeDelta = new Vector2(newThickness, newThickness);
                break;
            case ReticlePieceAlignment.Centre:
                _rectTransform.sizeDelta = new Vector2(newThickness, newThickness);
                break;
        }
    }
    public void SetPieceThickness(float newThickness, float newLength)
    {
        newThickness *= _thicknessMultiplier;

        switch (_type)
        {
            case ReticlePieceAlignment.Vertical:
                _rectTransform.sizeDelta = new Vector2(newThickness, newLength);
                break;
            case ReticlePieceAlignment.Horizontal:
                _rectTransform.sizeDelta = new Vector2(newLength, newThickness);
                break;
            case ReticlePieceAlignment.Corner:
                _rectTransform.sizeDelta = new Vector2(newThickness, newThickness);
                break;
            case ReticlePieceAlignment.Centre:
                _rectTransform.sizeDelta = new Vector2(newThickness, newThickness);
                break;
        }
    }

    public void SetBorderThickness(float newThickness)
    {
        _borderThickness = newThickness;
        _borderTransform.offsetMax = new Vector2(_borderThickness, _borderThickness);
        _borderTransform.offsetMin = new Vector2(-_borderThickness, -_borderThickness);
    }


    public void SetPieceColour(Color newColor) => _interiorImage.color = newColor;
    public void SetBorderColour(Color newColor) => _borderImage.color = newColor;
}