using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary> A Dynamic Crosshair that changes size based on values recieved from a WeaponManager.</summary>
public class DynamicCrosshair : MonoBehaviour
{
    private RectTransform _reticle;
    [SerializeField] private WeaponManager _connectedWeaponManager;

    [SerializeField] private List<CrosshairReticlePiece> _reticlePieces;

    [Space(5)]

    [SerializeField] private Color _defaultCrosshairColour = Color.yellow;

    [SerializeField] private float _defaultReticleThickness = 1f;
    [SerializeField] private Color _defaultBorderColour = Color.black;


    private void Start()
    {
        // Get the base reticle RectTransform.
        _reticle = GetComponent<RectTransform>();

        // Get the initial values from the PlayerManager.
        InitializeReticleValues();
    }
    private void Update() => SetCrosshairValues();

    /// <summary> Get values from the PlayerManager.</summary>
    private void InitializeReticleValues()
    {
        PlayerManager.Instance.CrosshairColour = _defaultCrosshairColour;

        PlayerManager.Instance.CrosshairBorderThickness = _defaultReticleThickness;
        PlayerManager.Instance.CrosshairBorderColour = _defaultBorderColour;
    }


    // Note: We should find a more efficient way to receive this, rather than doing this all every frame.
    // Set the current values for each part of the crosshair..
    private void SetCrosshairValues()
    {
        // Set the overall reticle size.
        SetReticleSize(_connectedWeaponManager.GetCrosshairSize());

        // Set the colour & thickness of each reticle piece.
        foreach (CrosshairReticlePiece reticlePiece in _reticlePieces)
        {
            reticlePiece.SetPieceColour(PlayerManager.Instance.CrosshairColour);
            
            reticlePiece.SetBorderThickness(PlayerManager.Instance.CrosshairBorderThickness);
            reticlePiece.SetBorderColour(PlayerManager.Instance.CrosshairBorderColour);
        }
    }

    private void SetReticleSize(float newSize) => _reticle.sizeDelta = new Vector2(newSize, newSize);
}
