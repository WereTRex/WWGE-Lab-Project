using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary> A piece of a reticle of the player's crosshair</summary>
public class CrosshairReticlePiece : MonoBehaviour
{
    // Influences how this crosshair piece scales.
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
        // Get this component's RectTransform.
        _rectTransform = GetComponent<RectTransform>();

        // Set the initial thickness of the border.
        SetBorderThickness(_borderThickness);
    }


    /// <summary> Set the thickness of the crosshair piece.</summary>
    public void SetPieceThickness(float newThickness)
    {
        // Multiply the new thickness by our multiplier (As different pieces of the reticle have different sizes).
        newThickness *= _thicknessMultiplier;

        // Set the thickness (Dependent on the type of alignment).
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
    /// <summary> Set the thickness & length of the crosshair piece.</summary>
    public void SetPieceThickness(float newThickness, float newLength)
    {
        // Multiply the new thickness by our multiplier (As different pieces of the reticle have different sizes).
        newThickness *= _thicknessMultiplier;

        // Set the thickness (Dependent on the type of alignment).
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

    /// <summary> Set the thickness of the border of the reticle piece</summary>
    public void SetBorderThickness(float newThickness)
    {
        // We don't multiply this thickness by our thickness multiplier.
        _borderThickness = newThickness;
        _borderTransform.offsetMax = new Vector2(_borderThickness, _borderThickness);
        _borderTransform.offsetMin = new Vector2(-_borderThickness, -_borderThickness);
    }


    public void SetPieceColour(Color newColor) => _interiorImage.color = newColor; // Set the colour of the inner piece.
    public void SetBorderColour(Color newColor) => _borderImage.color = newColor; // Set the colour of the border.
}