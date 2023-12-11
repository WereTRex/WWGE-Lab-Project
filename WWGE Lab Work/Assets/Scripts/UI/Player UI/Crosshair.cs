using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StaticCrosshair : MonoBehaviour
{
    private RectTransform _reticle;
    private CrosshairReticlePiece[] _childPieces;

    [Header("Size Values")]
    [Tooltip("The size of the crosshair"), Range(50, 250)]
        [SerializeField] private float _size = 100f;
    
    [Tooltip("The thickness of the crosshair segments"), Range(1, 5)]
    [SerializeField] private float _thickness = 2f;


    private void Start()
    {
        // Get the reticle transform.
        _reticle = GetComponent<RectTransform>();

        // Get all crosshair reticle pieces in the children of the reticle.
        _childPieces = _reticle.GetComponentsInChildren<CrosshairReticlePiece>();

        // Set the reticle size & thickness to the initial.
        UpdateReticleValues();
    }


    // Update the Reticle Size & Thickness to the current values.
    [ContextMenu("Update Reticle")]
    private void UpdateReticleValues()
    {
        SetReticleSize(_size);
        SetReticleThickness(_thickness);
    }

    
    // Set the size of the reticle.
    public void SetReticleSize(float newSize) => _reticle.sizeDelta = new Vector2(newSize, newSize);
    
    // Set the thickness of the child pieces.
    public void SetReticleThickness(float newThickness)
    {
        for (int i = 0; i < _childPieces.Length; i++)
        {
            _childPieces[i].SetPieceThickness(newThickness);
        }
    }
}
